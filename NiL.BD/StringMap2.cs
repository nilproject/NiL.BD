using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.BD
{
    public sealed class StringMap2<TValue> : IDictionary<string, TValue>
    {
        private const bool firstCharSearch = false;

        private struct Record
        {
            public int next;
            public int hash;
            public string key;
            public TValue value;
        }

        private static readonly Record[] emptyRecords = new Record[0];

        private bool emptyKeyValueExists = false;
        private TValue emptyKeyValue;

        private Record[] records = emptyRecords;

        private int count;

        private void insert(string key, TValue value, bool @throw)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (key.Length == 0)
            {
                if (@throw && emptyKeyValueExists)
                    throw new InvalidOperationException("Item already exists");
                emptyKeyValueExists = true;
                emptyKeyValue = value;
                //count++;
                return;
            }
            var elen = records.Length;
            if (records.Length == 0)
            {
                increaseSize();
                elen = records.Length;
            }
            elen--;
            int hash;
            int index;
            if (firstCharSearch)
            {
                hash = key[0];
                index = hash & elen;
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    if (@throw)
                        throw new InvalidOperationException("Item already exists");
                    records[index].value = value;
                    return;
                }
            }
            var keyLen = key.Length;
            hash = (keyLen | (keyLen << 10)) ^ 0xe0e0e0;
            for (var i = 0; i < keyLen; i++)
                hash += (hash >> 27) + (hash << 5) + key[i];
            //hash = key.GetHashCode();
            int colisionCount = 0;
            index = hash & elen;
            do
            {
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    if (@throw)
                        throw new InvalidOperationException("Item already exists");
                    records[index].value = value;
                    return;
                }
                index = records[index].next - 1;
            } while (index >= 0);
            // не нашли

            if (count == elen + 1)
            {
                increaseSize();
                elen = ((elen + 1) << 1) - 1;
            }
            if (firstCharSearch)
            {
                index = key[0] & elen;
                if (records[index].key == null)
                {
                    records[index].key = key;
                    records[index].hash = key[0];
                    records[index].value = value;
                    count++;
                    return;
                }
            }
            int prewIndex = -1;
            index = hash & elen;
            for (; ; )
            {
                if (records[index].key == null)
                {
                    records[index].hash = hash;
                    records[index].key = key;
                    records[index].value = value;
                    if (prewIndex >= 0)
                        records[prewIndex].next = index + 1;
                    count++;
                    break;
                }
                while (records[index].next > 0)
                {
                    index = records[index].next - 1;
                    colisionCount++;
                }
                prewIndex = index;
                while (records[index].key != null)
                {
                    index = (index + 19) & elen;
                    colisionCount++;
                }
            }
            if (colisionCount > 100)
                increaseSize();
        }

        public bool TryGetValue(string key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (key.Length == 0)
            {
                if (!emptyKeyValueExists)
                {
                    value = default(TValue);
                    return false;
                }
                value = emptyKeyValue;
                return true;
            }
            if (records.Length == 0)
            {
                value = default(TValue);
                return false;
            }
            int hash;
            int index;
            hash = key[0];
            index = (hash & int.MaxValue) % records.Length;
            if (firstCharSearch)
            {
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    value = records[index].value;
                    return true;
                }
            }
            var keyLen = key.Length;
            hash = (keyLen | (keyLen << 10)) ^ 0xe0e0e0;
            for (var i = 0; i < keyLen; i++)
                hash += (hash >> 27) + (hash << 5) + key[i];
            //hash = key.GetHashCode();
            for (index = hash & (records.Length - 1); index >= 0; index = records[index].next - 1)
            {
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    value = records[index].value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        public bool Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (key.Length == 0)
            {
                if (!emptyKeyValueExists)
                    return false;
                emptyKeyValue = default(TValue);
                emptyKeyValueExists = false;
                return true;
            }
            if (records.Length == 0)
                return false;
            int hash;
            int index;
            hash = key[0];
            index = (hash & int.MaxValue) % records.Length;
            if (firstCharSearch)
            {
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    value = records[index].value;
                    return true;
                }
            }
            var keyLen = key.Length;
            hash = (keyLen | (keyLen << 10)) ^ 0xe0e0e0;
            for (var i = 0; i < keyLen; i++)
                hash += (hash >> 27) + (hash << 5) + key[i];
            //hash = key.GetHashCode();
            for (index = hash & (records.Length - 1); index >= 0; index = records[index].next - 1)
            {
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    value = records[index].value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        private void increaseSize()
        {
            if (records.Length == 0)
            {
                records = new Record[1];
                //values = new TValue[1];
                return;
            }
            var oldRecords = records;
            records = new Record[records.Length << 1];
            count = 0;
            if (emptyKeyValueExists)
                count++;
            for (var i = oldRecords.Length; i-- > 0; )
            {
                if (oldRecords[i].key != null)
                    insert(oldRecords[i].key, oldRecords[i].value, false);
            }
        }

        public void Add(string key, TValue value)
        {
            insert(key, value, true);
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public ICollection<string> Keys
        {
            get { throw new NotImplementedException(); }
        }
        
        public ICollection<TValue> Values
        {
            get { throw new NotImplementedException(); }
        }

        public TValue this[string key]
        {
            get
            {
                TValue result;
                if (!TryGetValue(key, out result))
                    throw new KeyNotFoundException();
                return result;
            }
            set
            {
                insert(key, value, false);
            }
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            int c = 0;
            if (emptyKeyValueExists)
            {
                yield return new KeyValuePair<string, TValue>("", emptyKeyValue);
                c++;
            }
            for (int i = 0; i < records.Length; i++)
            {
                if (records[i].key != null)
                {
                    c++;
                    yield return new KeyValuePair<string, TValue>(records[i].key, records[i].value);
                    if (c == count)
                        yield break;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
