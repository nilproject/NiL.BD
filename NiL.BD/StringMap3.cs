using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.BD
{
    public sealed class StringMap3<TValue> : IDictionary<string, TValue>
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
        private Record[] collisedRecords = emptyRecords;

        private int count;
        private int collisedCount;

        public StringMap3()
        {
        }

        public StringMap3(int p)
        {
            records = new Record[p];
        }

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
            //hash = computeHash(key);
            hash = key.GetHashCode();
            index = hash & elen;
            var store = records;
            do
            {
                if (store[index].key != null
                    && store[index].hash == hash
                    && string.CompareOrdinal(store[index].key, key) == 0)
                {
                    if (@throw)
                        throw new InvalidOperationException("Item already exists");
                    store[index].value = value;
                    return;
                }
                index = store[index].next - 1;
                store = collisedRecords;
            } while (index >= 0);
            // не нашли

            if (elen <= collisedCount
                || (collisedCount >= 50 && elen <= collisedCount >> 2))
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
            var prewStore = records;
            index = hash & elen;
            store = records;
            int colisionCount = 0;
            for (; ; )
            {
                if (store[index].key == null)
                {
                    store[index].hash = hash;
                    store[index].key = key;
                    store[index].value = value;
                    if (prewIndex >= 0)
                        prewStore[prewIndex].next = index + 1;
                    if (store == collisedRecords)
                        collisedCount++;
                    count++;
                    break;
                }
                while (store[index].next > 0)
                {
                    index = store[index].next - 1;
                    store = collisedRecords;
                    colisionCount++;
                }
                if (collisedCount == collisedRecords.Length)
                    increaseColisedBufferSize();
                prewIndex = index;
                if (colisionCount != 0)
                    prewStore = collisedRecords;
                store = collisedRecords;
                index = collisedCount;
                while (store[index].key != null)
                {
                    index = (index + 1) % store.Length;
                }
            }
            if (colisionCount > 10)
                increaseSize();
        }

        private static int computeHash(string key)
        {
            int hash;
            var keyLen = key.Length;
            hash = (keyLen | (keyLen << 10)) ^ 0xe0e0e0;
            for (var i = 0; i < keyLen; i++)
                hash += (hash >> 27) + (hash << 5) + key[i];
            return hash;
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
            var store = records;
            if (firstCharSearch)
            {
                hash = key[0];
                index = (hash & int.MaxValue) % records.Length;
                if (store[index].hash == hash && string.CompareOrdinal(store[index].key, key) == 0)
                {
                    value = store[index].value;
                    return true;
                }
            }
            //hash = computeHash(key);
            hash = key.GetHashCode();
            for (index = hash & (records.Length - 1); index >= 0; index = store[index].next - 1, store = collisedRecords)
            {
                if (store[index].hash == hash && string.CompareOrdinal(store[index].key, key) == 0)
                {
                    value = store[index].value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        public bool Remove(string key)
        {
            return false;
        }

        private void increaseColisedBufferSize()
        {
            var oldColisedRecords = collisedRecords;
            collisedRecords = new Record[oldColisedRecords.Length == 0 ? 1 : oldColisedRecords.Length << 1];
            Array.Copy(oldColisedRecords, collisedRecords, oldColisedRecords.Length);
            //for (var i = 0; i < oldColisedRecords.Length; i++)
            //    collisedRecords[i] = oldColisedRecords[i];
        }

        private void increaseSize()
        {
            if (records.Length == 0)
            {
                records = new Record[1];
                //values = new TValue[1];
                return;
            }
            if (count > 100 && records.Length / count > 2)
                throw new InvalidOperationException();
            var oldRecords = records;
            var oldColisedRecords = collisedRecords;
            collisedRecords = emptyRecords;
            records = new Record[records.Length << 1];
            count = 0;
            collisedCount = 0;
            if (emptyKeyValueExists)
                count++;
            for (var i = oldRecords.Length; i-- > 0; )
            {
                if (oldRecords[i].key != null)
                    insert(oldRecords[i].key, oldRecords[i].value, false);
            }
            for (var i = oldColisedRecords.Length; i-- > 0; )
            {
                if (oldColisedRecords[i].key != null)
                    insert(oldColisedRecords[i].key, oldColisedRecords[i].value, false);
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
