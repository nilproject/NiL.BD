using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NiL.BD
{
    public sealed class IndexedStorage<T> : IEnumerable<IndexedStorage<T>.Record>, ICollection<IndexedStorage<T>.Record>, IDisposable
    {
        private enum RecordType : byte
        {
            Data = 0,
            Empty = 1
        }

        private static BinaryFormatter formatter = new BinaryFormatter();

        public sealed class Record
        {
            private IndexedStorage<T> owner;
            private long position;
            private int size;

            public long Index { get; internal set; }
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
                    if (Index != -1)
                    {
                        owner.tempStream.SetLength(0);
                        formatter.Serialize(owner.tempStream, value);
                        if (owner.tempStream.Length > size && Index != owner.count - 1)
                        {
                            owner.cindex.Position = 8 * Index;
                            var oldPos = owner.readLong(owner.cindex);
                            owner.data.Position = oldPos;
                            owner.data.WriteByte((byte)RecordType.Empty);
                            position = owner.data.Length;
                            owner.cindex.Position = 8 * Index;
                            owner.writeLong(owner.cindex, position);
                            owner.data.Position = position;
                            owner.writeData(Name);
                            size = (int)owner.tempStream.Length;
                            position += 4 + Encoding.Default.GetByteCount(name);
                        }
                        else
                        {
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

            internal Record(string name, IndexedStorage<T> owner, long position, int size, long index)
            {
                if (name.Length > 255)
                    throw new ArgumentException();
                this.name = name;
                this.owner = owner;
                this.position = position;
                this.size = size;
                this.Index = index;
            }

            public Record(string name, T value)
            {
                if (name.Length > 255)
                    throw new ArgumentException();
                isValueValid = true;
                this.value = value;
                this.name = name;
                Index = -1;
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

        private MemoryStream tempStream;
        private Stream data;
        private Stream cindex;
        private Stream nameIndexDump;
        private BinaryTree<long> nameIndex;
        private long count;

        public Record this[long index]
        {
            get
            {
                if (index < 0 || index > count)
                    throw new ArgumentOutOfRangeException();
                cindex.Position = 8 * index;
                return readRecord(readLong(cindex), index);
            }
        }

        public Record this[string name]
        {
            get
            {
                return this[nameIndex[name]];
            }
        }

        public bool IsDisposed { get; private set; }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return this; } }
        public bool IsReadOnly { get { return false; } }
        /// <summary>
        /// Количество элементов в коллекции.
        /// </summary>
        public int Count { get { return (int)count; } }

        /// <summary>
        /// Количество элементов в коллекции.
        /// </summary>
        public long LongCount { get { return count; } }

        /// <summary>
        /// Создаёт коллекцию именованных элементов, используя для хранения указанный поток.
        /// </summary>
        /// <param name="dataBase">Поток, используемый для хранения элементов коллекции.</param>
        public IndexedStorage(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException();
            if (!typeof(T).IsSerializable)
                throw new ArgumentException(typeof(T) + " is not serializable.");
            IsDisposed = false;
            tempStream = new MemoryStream(65535);
            this.data = new FileStream(directory + "/data.db", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            this.cindex = new FileStream(directory + "/cindex.db", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            nameIndexDump = new FileStream(directory + "/nindex.db", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            byte[] buf = new byte[4];
            if (data.Length != 0)
            {
                data.Position = 0;
                data.Read(buf, 0, 4);
                if (buf[0] == 0xFA
                    && buf[1] == 0x1D
                    && buf[2] == 0xA1
                    && buf[3] == 0xA0)
                {
                    count = cindex.Length / 8;
                    nameIndex = (BinaryTree<long>)formatter.Deserialize(nameIndexDump);
                    return;
                }
            }
            Clear();
        }

        ~IndexedStorage()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                data.Close();
                tempStream.Close();
                data.Dispose();
                tempStream.Dispose();
                nameIndexDump.Position = 0;
                formatter.Serialize(nameIndexDump, nameIndex);
                nameIndexDump.Close();
                IsDisposed = true;
            }
        }

        private Record readRecord(long position, long index)
        {
            data.Position = position;
            data.Read(longBuf, 0, 4);
            RecordType rt = (RecordType)longBuf[0];
            var size = longBuf[2] + longBuf[3] * 256;
            position += size + longBuf[1] + 4 + 8 + 1;
            if (rt != RecordType.Data)
                return null;
            byte[] buf = tempStream.GetBuffer();
            data.Read(buf, 0, longBuf[1]);
            string name = Encoding.Default.GetString(buf, 0, longBuf[1]);
            return new Record(name, this, data.Position, size, index);
        }

        public IEnumerator<IndexedStorage<T>.Record> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IndexedStorage<T>.Record>).GetEnumerator();
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
        public void CopyTo(IndexedStorage<T>.Record[] array, int index)
        {
            foreach (var r in this)
                array.SetValue(r, index++);
        }

        /// <summary>
        /// Записывает в текующую позицию data структуру со значением, сериализованным в tempStream и указанным именем.
        /// </summary>
        /// <param name="name">Имя записываемой структуры.</param>
        private void writeData(string name)
        {
            byte[] nameBuf = Encoding.Default.GetBytes(name);
            data.WriteByte((byte)RecordType.Data);
            data.WriteByte((byte)nameBuf.Length);
            writeUShort(data, (ushort)tempStream.Length);
            data.Write(nameBuf, 0, nameBuf.Length);
            data.Write(tempStream.GetBuffer(), 0, (int)tempStream.Length);
        }

        /// <summary>
        /// Добавляет запись в конец файла.
        /// </summary>
        /// <param name="item">Запись, которую следует добавить.</param>
        public void Add(IndexedStorage<T>.Record item)
        {
            if (nameIndex.ContainsKey(item.Name))
                throw new ArgumentException();
            tempStream.SetLength(0);
            formatter.Serialize(tempStream, item.Value);
            if (tempStream.Length > 65535)
                throw new ArgumentException("Data too learge");
            var pos = data.Length;
            data.Position = pos;
            writeData(item.Name);
            cindex.Position = cindex.Length;
            writeLong(cindex, pos);
            item.Index = cindex.Length / 8 - 1;
            count++;
            nameIndex.Add(item.Name, item.Index);
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
            cindex.SetLength(0);
            nameIndex = new BinaryTree<long>();
        }

        /// <summary>
        /// Always return false.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(IndexedStorage<T>.Record item)
        {
            return false;
        }

        /// <summary>
        /// Always return false.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(IndexedStorage<T>.Record item)
        {
            return false;
        }
    }
}
