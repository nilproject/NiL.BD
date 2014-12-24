using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.BD
{
    public sealed class StringMap<TValue> : IDictionary<string, TValue>
    {
        private static readonly Entry[] emptyEntries = new Entry[0];

        private const int ListLimit = 17;

        private enum EntryState
        {
            Empty = 0,
            Filled
        }

        private struct Entry
        {
            public EntryState state;
            /// <summary>
            /// Для сравнения в режиме хэш-таблицы
            /// </summary>
            public int hash;
            public int next;
            public string key;
        }

        private Entry[] entries;
        private TValue[] values;
        private int count;

        public static int missCount = 0;

        private int find(string key, bool create)
        {
            if (entries.Length == 0)
            {
                if (create)
                    increase();
                else
                    return -1;
            }
            int hash = 0x0e0e0;
            var createIndex = -1;
            var elen = entries.Length;
            int i = 0;
            int index = 0;
            int keyLen = key.Length;
            int prewIndex;
            if (entries.Length < ListLimit)
            {
                for (i = 0; i < elen; i++)
                {
                    if (entries[i].state == EntryState.Empty)
                    {
                        if (create)
                        {
                            entries[i].hash = key[0];
                            return i;
                        }
                        else
                            return -1;
                    }
                    else if (key[0] == entries[i].hash
                        && string.CompareOrdinal(entries[i].key, key) == 0)
                        return i;
                }
                if (create)
                {
                    increase();
                    return find(key, true);
                }
                return createIndex;
            }
            else
            {
                for (i = keyLen; i-- > 0; )
                {
                    hash += key[i] * 0x101;
                    hash += hash >> 28;
                    hash += hash << 4;
                    index = (hash & int.MaxValue) % elen;
                    if (entries[index].state == EntryState.Empty)
                    {
                        if (create)
                        {
                            entries[index].hash = hash;
                            return index;
                        }
                        return -1;
                    }
                    else
                    {
                        if (entries[index].hash == hash)
                        {
                            if (string.CompareOrdinal(entries[index].key, key) == 0)
                                return index;
                            if (create)
                                missCount++; // вот в этом корень тормозов
                        }
                    }
                }
                prewIndex = index;
                index = entries[index].next - 1;
                while (index >= 0) // для next нумерация будет с 1
                {
                    if (entries[index].state == EntryState.Filled
                        && string.CompareOrdinal(entries[index].key, key) == 0)
                        return index;

                    prewIndex = index;
                    index = entries[index].next - 1;
                }
                if (create)
                {
                    if (count >= elen)
                    {
                        // Здесь, так как всё предшествующее время мы убеждались, что такой ключ ещё не добавлен
                        increase();
                        return find(key, true);
                    }
                    var startIndex = prewIndex;
                    createIndex = (prewIndex + 1) % elen;
                    index = 0;
                    while (entries[createIndex].state != EntryState.Empty && createIndex != startIndex)
                    {
                        createIndex = (createIndex + 61) % elen;
                        index++;
                    }
                    if (entries[createIndex].state != EntryState.Empty
                        || index > 50)
                    {
                        increase();
                        return find(key, true);
                    }
                    entries[createIndex].hash = hash;
                    entries[prewIndex].next = createIndex + 1;
                    return createIndex;
                }
                return -1;
            }
        }

        private void increase()
        {
            if (entries.Length == 0)
            {
                entries = new Entry[2];
                values = new TValue[3];
                return;
            }
            if (entries.Length > 20000000)
                throw new Exception();
            var oldEntries = entries;
            var oldValues = values;
            if (entries.Length >= ListLimit || entries.Length <= ListLimit >> 1)
            {
                entries = new Entry[entries.Length << 1];
                values = new TValue[entries.Length + 1];
            }
            else
            {
                entries = new Entry[ListLimit];
                values = new TValue[ListLimit + 1];
            }
            values[entries.Length] = oldValues[oldEntries.Length];
            for (var i = 0; i < oldEntries.Length; i++)
            {
                if (oldEntries[i].state == EntryState.Filled)
                {
                    var index = find(oldEntries[i].key, true);
                    entries[index].state = EntryState.Filled;
                    entries[index].key = oldEntries[i].key;
                    values[index] = oldValues[i];
                }
            }
        }

        public StringMap()
        {
            entries = emptyEntries;
        }

        #region Члены IDictionary<string,TValue>

        public void Add(string key, TValue value)
        {
            var index = find(key, true);
            if (entries[index].state == EntryState.Filled)
                throw new ArgumentException("Key already exists");
            entries[index].state = EntryState.Filled;
            entries[index].key = key;
            values[index] = value;
            count++;
        }

        public bool ContainsKey(string key)
        {
            return find(key, false) != -1;
        }

        public ICollection<string> Keys
        {
            get { return (from e in entries where e.state == EntryState.Filled select e.key).ToArray(); }
        }

        public bool Remove(string key)
        {
            /*var index = find(key, false);
            if (index == -1)
                return false;
            entries[index].state = EntryState.Deleted;
            entries[index].key = null;
            values[index] = default(TValue);
            count--;
            return true;*/
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out TValue value)
        {
            var index = find(key, false);
            if (index == -1)
            {
                value = default(TValue);
                return false;
            }
            value = values[index];
            return true;
        }

        public ICollection<TValue> Values
        {
            get
            {
                TValue[] result = new TValue[count];
                for (int i = 0, j = 0; i < entries.Length && j < count; i++)
                {
                    if (entries[i].state == EntryState.Filled)
                        result[j++] = values[i];
                }
                return result;
            }
        }

        public TValue this[string key]
        {
            get
            {
                var index = find(key, false);
                if (index == -1)
                    throw new KeyNotFoundException();
                return values[index];
            }
            set
            {
                var index = find(key, true);
                if (entries[index].state != EntryState.Filled)
                {
                    entries[index].state = EntryState.Filled;
                    count++;
                }
                entries[index].key = key;
                values[index] = value;
            }
        }

        #endregion

        #region Члены ICollection<KeyValuePair<string,TValue>>

        public void Add(KeyValuePair<string, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            count = 0;
            for (var i = entries.Length; i-- > 0; )
            {
                entries[i] = default(Entry);
                values[i] = default(TValue);
            }
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return ContainsKey(item.Key);
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
            return Remove(item.Key);
        }

        #endregion

        #region Члены IEnumerable<KeyValuePair<string,TValue>>

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].state == EntryState.Filled)
                    yield return new KeyValuePair<string, TValue>(entries[i].key, values[i]);
            }
        }

        #endregion

        #region Члены IEnumerable

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
