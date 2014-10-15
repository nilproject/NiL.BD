using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NiL.BD
{
    public sealed class NamedStorage<T> : IEnumerable<NamedStorage<T>.Record>, IDictionary<string, T>, IDisposable
    {
        private enum RecordType : byte
        {
            Data = 0,
            Empty = 1
        }

        private static BinaryFormatter formatter = new BinaryFormatter();

        public sealed class Record
        {
            private NamedStorage<T> owner;
            private long position;
            private int size;

            private bool isValueValid;
            private T value;
            public T Value
            {
                get
                {
                    if (!isValueValid)
                        validateValue();
                    return value;
                }
                set
                {
                    if (position != -1)
                    {
                        owner.tempStream.SetLength(0);
                        formatter.Serialize(owner.tempStream, value);
                        if (owner.tempStream.Length > size && position + size < owner.data.Length)
                        {
                            owner.data.Position = position;
                            owner.data.WriteByte((byte)RecordType.Empty);
                            position = owner.data.Length + 4 + name.Length * sizeof(char);
                            owner.data.Position = position;
                            owner.writeRecord(name);
                            owner.nameIndex[name] = position;
                        }
                        else
                        {
                            if (owner.tempStream.Length != size)
                            {
                                owner.data.Position = position - name.Length * sizeof(char) - sizeof(ushort);
                                size = (int)owner.tempStream.Length;
                                owner.writeUShort(owner.data, (ushort)size);
                            }
                            owner.data.Position = position;
                            owner.data.Write(owner.tempStream.GetBuffer(), 0, size);
                        }
                    }
                    isValueValid = true;
                    this.value = value;
                }
            }
            private string name;
            public string Name
            {
                get { return name; }
            }

            private void validateValue()
            {
                owner.data.Position = position;
                value = (T)formatter.Deserialize(owner.data);
                isValueValid = true;
            }

            internal Record(string name, NamedStorage<T> owner, long position, int size)
            {
                if (name.Length > 255)
                    throw new ArgumentException();
                this.name = name;
                this.owner = owner;
                this.position = position;
                this.size = size;
            }
        }

        private readonly byte[] longBuf = new byte[8];
        private long readLong(Stream stream)
        {
            ulong res = 0;
            stream.Read(longBuf, 0, 8);
            for (int i = 0; i < 8; i++)
                res = res | ((ulong)longBuf[i] << (8 * i));
            return (long)res;
        }

        private ushort readUShort(Stream stream)
        {
            int res = 0;
            stream.Read(longBuf, 0, 2);
            for (int i = 0; i < 2; i++)
                res = res | ((byte)longBuf[i] << (8 * i));
            return (ushort)res;
        }

        private void writeLong(Stream stream, long value)
        {
            for (int i = 0; i < 8; i++)
            {
                stream.WriteByte((byte)(value & 0xff));
                value >>= 8;
            }
        }

        private void writeUShort(Stream stream, ushort value)
        {
            for (int i = 0; i < 2; i++)
            {
                stream.WriteByte((byte)(value & 0xff));
                value >>= 8;
            }
        }

        [NonSerialized]
        private MemoryStream tempStream;
        [NonSerialized]
        private Stream data;
        [NonSerialized]
        private Stream nameIndexDump;
        private BinaryTree<long> nameIndex;

        public bool IsDisposed { get; private set; }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return this; } }
        public bool IsReadOnly { get { return false; } }
        /// <summary>
        /// Количество элементов в коллекции.
        /// </summary>
        public int Count { get { return nameIndex.Count; } }

        /// <summary>
        /// Создаёт коллекцию именованных элементов, используя для хранения указанный поток.
        /// </summary>
        /// <param name="dataBase">Поток, используемый для хранения элементов коллекции.</param>
        public NamedStorage(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException();
            if (!typeof(T).IsSerializable)
                throw new ArgumentException(typeof(T) + " is not serializable.");
            IsDisposed = false;
            tempStream = new MemoryStream(65535);
            data = new FileStream(directory + "/data.db", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            nameIndexDump = new FileStream(directory + "/index.db", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            GC.SuppressFinalize(nameIndexDump);
            byte[] buf = new byte[4];
            if (data.Length != 0)
            {
                try
                {
                    data.Position = 0;
                    data.Read(buf, 0, 4);
                    if (buf[0] == 0xFA
                        && buf[1] == 0x1D
                        && buf[2] == 0xA1
                        && buf[3] == 0xA0)
                    {
                        nameIndex = (BinaryTree<long>)formatter.Deserialize(nameIndexDump);
                        return;
                    }
                }
                catch
                {

                }
            }
            Clear();
        }

        ~NamedStorage()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                nameIndexDump.Position = 0;
                formatter.Serialize(nameIndexDump, nameIndex);
                nameIndexDump.Close();
                nameIndexDump.Dispose();
                data.Close();
                tempStream.Close();
                data.Dispose();
                tempStream.Dispose();
            }
        }
        private static readonly Func<int, string> fastAllocateString = typeof(string).GetMethod("FastAllocateString", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate(typeof(Func<int, string>)) as Func<int, string>;

        private Record readRecord(long position)
        {
            data.Position = position;
            data.Read(longBuf, 0, 4);
            RecordType rt = (RecordType)longBuf[0];
            var size = longBuf[2] + longBuf[3] * 256;
            if (rt != RecordType.Data)
                return null;
            byte[] buf = tempStream.GetBuffer();
            data.Read(buf, 0, longBuf[1] * sizeof(char));
            string name = fastAllocateString(longBuf[1]);
            var sptr = getPtr(name) + 4 + IntPtr.Size;
            for (var i = 0; i < name.Length * 2; i++)
            {
                Marshal.WriteByte(sptr, buf[i]);
                sptr += 1;
            }
            return new Record(name, this, data.Position, size);
        }

        public IEnumerator<NamedStorage<T>.Record> GetEnumerator()
        {
            foreach (var e in nameIndex)
                yield return readRecord(e.Value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<NamedStorage<T>.Record>).GetEnumerator();
        }

        /// <summary>
        /// Копирует элементы коллекции System.Collections.ICollection в массив System.Array,
        /// начиная с указанного индекса массива System.Array.
        /// </summary>
        /// <param name="array">Одномерный массив System.Array, в который копируются элементы из интерфейса
        ///     System.Collections.ICollection. Массив System.Array должен иметь индексацию,
        ///     начинающуюся с нуля.</param>
        /// <param name="index">Отсчитываемый от нуля индекс в массиве array, указывающий начало копирования.</param>
        public void CopyTo(Array array, int index)
        {
            foreach (var r in this)
                array.SetValue(r, index++);
        }

        /// <summary>
        /// Копирует элементы коллекции System.Collections.ICollection в массив System.Array,
        /// начиная с указанного индекса массива System.Array.
        /// </summary>
        /// <param name="array">Одномерный массив System.Array, в который копируются элементы из интерфейса
        ///     System.Collections.ICollection. Массив System.Array должен иметь индексацию,
        ///     начинающуюся с нуля.</param>
        /// <param name="index">Отсчитываемый от нуля индекс в массиве array, указывающий начало копирования.</param>
        public void CopyTo(NamedStorage<T>.Record[] array, int index)
        {
            foreach (var r in this)
                array.SetValue(r, index++);
        }

        private static readonly Func<object, IntPtr> getPtr = Activator.CreateInstance(typeof(Func<object, IntPtr>), null, (new Func<IntPtr, IntPtr>(x => x)).Method.MethodHandle.GetFunctionPointer()) as Func<object, IntPtr>;

        /// <summary>
        /// Записывает в текующую позицию data структуру со значением, сериализованным в tempStream и указанным именем.
        /// </summary>
        /// <param name="name">Имя записываемой структуры.</param>
        private void writeRecord(string name)
        {
            data.WriteByte((byte)RecordType.Data);
            data.WriteByte((byte)name.Length);
            writeUShort(data, (ushort)tempStream.Length);
            var sptr = getPtr(name) + 4 + IntPtr.Size;
            for (var i = name.Length * sizeof(char); i-- > 0; )
            {
                data.WriteByte(Marshal.ReadByte(sptr));
                sptr += 1;
            }
            data.Write(tempStream.GetBuffer(), 0, (int)tempStream.Length);
        }

        /// <summary>
        /// Добавляет запись в конец файла.
        /// </summary>
        /// <param name="name">Название записи</param>
        /// <param name="value">Содержимое записи</param>
        public void Add(string name, T value)
        {
            if (name.Length > 250 || nameIndex.ContainsKey(name))
                throw new ArgumentException();
            tempStream.SetLength(0);
            formatter.Serialize(tempStream, value);
            if (tempStream.Length > 65535)
                throw new ArgumentException("Data too learge");
            var pos = data.Length;
            data.Position = pos;
            writeRecord(name);
            nameIndex.Add(name, pos);
        }

        public void Clear()
        {
            byte[] buf = new byte[4];
            data.SetLength(0);
            data.Position = 0;
            buf[0] = 0xFA;
            buf[1] = 0x1D;
            buf[2] = 0xA1;
            buf[3] = 0xA0;
            data.Write(buf, 0, 4);
            nameIndex = new BinaryTree<long>();
        }

        /// <summary>
        /// Always return false.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(Record item)
        {
            return false;
        }

        public IEnumerable<Record> ByPrefix(string prefix)
        {
            return ByPrefix(prefix, false, long.MaxValue, 0);
        }

        public IEnumerable<Record> ByPrefix(string prefix, bool reversed)
        {
            return ByPrefix(prefix, reversed, int.MaxValue, 0);
        }

        public IEnumerable<Record> ByPrefix(string prefix, bool reversed, long count)
        {
            return ByPrefix(prefix, reversed, count, 0);
        }

        public IEnumerable<Record> ByPrefix(string prefix, bool reversed, long count, int offset)
        {
            foreach (var i in nameIndex.StartedWith(prefix, reversed, offset, count))
            {
                yield return readRecord(i.Value);
            }
        }

        public int CountWithPrefix(string prefix)
        {
            var res = 0;
            foreach (var i in nameIndex.StartedWith(prefix))
                res++;
            return res;
        }

        #region Члены IDictionary<string,T>


        public bool ContainsKey(string key)
        {
            return nameIndex.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return nameIndex.Keys; }
        }

        public bool Remove(string key)
        {
            long position = 0;
            if (!nameIndex.TryGetValue(key, out position))
                return false;
            data.Position = position;
            data.WriteByte((byte)RecordType.Empty);
            nameIndex.Remove(key);
            return true;
        }

        public bool TryGetValue(string key, out T value)
        {
            value = default(T);
            long position = 0;
            if (!nameIndex.TryGetValue(key, out position))
                return false;
            value = readRecord(position).Value;
            return true;
        }

        public ICollection<T> Values
        {
            get { throw new NotImplementedException(); }
        }

        public T this[string key]
        {
            get
            {
                return readRecord(nameIndex[key]).Value;
            }
            set
            {
                readRecord(nameIndex[key]).Value = value;
            }
        }

        #endregion

        #region Члены ICollection<KeyValuePair<string,T>>

        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Члены IEnumerable<KeyValuePair<string,T>>

        IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
