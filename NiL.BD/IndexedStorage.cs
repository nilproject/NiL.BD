using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NiL.WBE.DataBase
{
    public sealed class IndexedStorage<T> : IEnumerable<IndexedStorage<T>.Record>, ICollection<IndexedStorage<T>.Record>, IDisposable
    {
        private enum RecordType : byte
        {
            Data,
            Label
        }

        private static BinaryFormatter formatter = new BinaryFormatter();

        public sealed class Record
        {
            private IndexedStorage<T> owner;
            private byte[] buf;

            public long Index { get; internal set; }
            private bool isValueValid;
            private T value;
            public T Value
            {
                get
                {
                    if (!isValueValid)
                    {
                        owner.tempStream.Position = 0;
                        owner.tempStream.Write(buf, 1 + buf[0], buf.Length - 1 - buf[0]);
                        owner.tempStream.Position = 0;
                        value = (T)formatter.Deserialize(owner.tempStream);
                        isValueValid = true;
                    }
                    return value;
                }
            }
            private string name;
            public string Name
            {
                get { return name; }
                private set
                {
                    if (value.Length > 255)
                        throw new ArgumentException();
                    name = value;
                }
            }

            internal Record(string name, IndexedStorage<T> owner, byte[] buf)
            {
                this.Name = name;
                this.owner = owner;
                this.buf = buf;
            }

            public Record(string name, T value)
            {
                isValueValid = true;
                this.value = value;
                this.Name = name;
            }
        }

        public enum Direction
        {
            FromStartToEnd,
            FromEndToStart
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
        private long count;

        public Record this[int index]
        {
            get
            {
                if (index < 0 || index > count)
                    throw new ArgumentOutOfRangeException();
                cindex.Position = 8 * index;
                return readRecord(readLong(cindex));
            }
        }

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
        /// Направление перебора элементов по-умолчанию.
        /// </summary>
        public Direction DefaultDirection { get; set; }

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
            tempStream = new MemoryStream(65535);
            this.data = new FileStream(directory + "/data.db", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            this.cindex = new FileStream(directory + "/cindex.db", FileMode.OpenOrCreate, FileAccess.ReadWrite);
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
                    data.Position = data.Length - 8 - 2 - 1;
                    count = readLong(data);
                    if (cindex.Length != count * 8)
                        Reindex();
                    return;
                }
            }
            Clear();
        }

        public void Dispose()
        {
            data.Close();
            tempStream.Close();
            data.Dispose();
            tempStream.Dispose();
        }

        public void Reindex()
        {
            cindex.Position = 0;
            foreach (var r in this)
                writeLong(cindex, data.Position);
        }

        public IEnumerable<Record> Select(string name)
        {
            foreach(var rec in this)
            {
                if (rec.Name == name)
                    yield return rec;
            }
        }

        private Record readRecord(long position)
        {
            var t = position;
            return readRecord(ref t);
        }

        private Record readRecord(ref long position)
        {
            data.Position = position;
            data.Read(longBuf, 0, 3);
            RecordType rt = (RecordType)longBuf[0];
            var size = longBuf[1] + longBuf[2] * 256;
            position += size + 8 + 2 + 2 + 1 + 1;
            if (rt != RecordType.Data)
                return null;
            byte[] buf = new byte[size];
            data.Read(buf, 0, size);
            var nlen = buf[0];
            string name = Encoding.Default.GetString(buf, 1, nlen);
            return new Record(name, this, buf)
            {
                Index = readLong(data) - 1
            };
        }

        public IEnumerator<IndexedStorage<T>.Record> GetEnumerator()
        {
            switch (DefaultDirection)
            {
                case Direction.FromStartToEnd:
                    {
                        long pos = 14;
                        data.Position = pos;
                        for (; data.Position < data.Length; )
                        {
                            var res = readRecord(ref pos);
                            if (res == null)
                                continue;
                            yield return res;
                        }
                        break;
                    }
                case Direction.FromEndToStart:
                    {
                        long pos = data.Length;
                        for (; ; )
                        {
                            data.Position = pos - 2 - 1;
                            data.Read(longBuf, 0, 3);
                            var size = longBuf[0] + longBuf[1] * 256;
                            if (size == 0)
                                break;
                            pos -= size + 8 + 2 + 2 + 1 + 1;
                            RecordType rt = (RecordType)longBuf[2];
                            if (rt != RecordType.Data)
                                continue;
                            data.Position = pos + 2 + 1;
                            byte[] buf = new byte[size];
                            data.Read(buf, 0, size);
                            var nlen = buf[0];
                            string name = Encoding.Default.GetString(buf, 1, nlen);
                            yield return new Record(name, this, buf)
                            {
                                Index = readLong(data) - 1
                            };
                        }
                        break;
                    }
                default:
                    throw new ArgumentException();
            }
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
        /// Добавляет запись в конец файла.
        /// </summary>
        /// <param name="item">Запись, которую следует добавить.</param>
        public void Add(IndexedStorage<T>.Record item)
        {
            data.Position = data.Length;
            byte[] name = Encoding.Default.GetBytes(item.Name);
            tempStream.SetLength(0);
            formatter.Serialize(tempStream, item.Value);
            if (name.Length + tempStream.Length > 65534)
                throw new ArgumentException("Data too learge");
            writeLong(cindex, data.Position);
            data.WriteByte((byte)RecordType.Data);
            writeUShort(data, (ushort)(name.Length + tempStream.Length + 1));
            data.WriteByte((byte)name.Length);
            data.Write(name, 0, name.Length);
            data.Write(tempStream.GetBuffer(), 0, (int)tempStream.Length);
            writeLong(data, ++count);
            writeUShort(data, (ushort)(name.Length + tempStream.Length + 1));
            data.WriteByte((byte)RecordType.Data);
            tempStream.SetLength(0);
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
            buf[0] = buf[1] = buf[2] = buf[3] = 0;
            data.Write(buf, 0, 4);
            data.Write(buf, 0, 4);
            data.Write(buf, 0, 2);
            cindex.SetLength(0);
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
