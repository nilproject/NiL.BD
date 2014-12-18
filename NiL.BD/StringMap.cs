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

        private const int ListLimit = 10;

        private enum EntryState : short
        {
            Empty = 0,
            Deleted = 1,
            Filled = 2
        }

        private struct Entry
        {
            /// <summary>
            /// Для сравнения в режиме списка
            /// </summary>
            public char firstChar;
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
        private int insertIndex;

        private int find(string key, bool create)
        {
            if (create && entries.Length == 0)
                increase();
            if (key.Length == 0)
                return entries.Length;
            int hash = 0;
            var createIndex = -1;
            if (entries.Length < ListLimit)
            {
                var fc = key[0];
                for (var i = entries.Length; i-- > 0 && entries[i].state > 0; )
                {
                    if (entries[i].state == EntryState.Deleted)
                    {
                        if (create && createIndex == -1)
                            createIndex = i;
                    }
                    else if (fc == entries[i].firstChar && string.CompareOrdinal(entries[i].key, key) == 0)
                        return i;
                }
                if (create)
                {
                    if (createIndex == -1)
                    {
                        if (entries[entries.Length - 1].state == EntryState.Empty)
                            createIndex = entries.Length - 1;
                        else
                        {
                            increase();
                            return find(key, true);
                        }
                    }
                    entries[createIndex].firstChar = fc;
                }
                return createIndex;
            }
            else
            {
                for (var i = 0; i < key.Length; i++)
                {
                    hash += key[i];
                    hash ^= hash >> 27;
                    hash += hash << 5;
                    var index = (hash & int.MaxValue) % entries.Length;
                    switch (entries[index].state)
                    {
                        case EntryState.Empty:
                            {
                                if (create)
                                {
                                    entries[index].hash = hash;
                                    return index;
                                }
                                return -1;
                            }
                        case EntryState.Filled:
                            {
                                if (entries[index].hash == hash
                                    && string.CompareOrdinal(entries[index].key, key) == 0)
                                    return index;
                                break;
                            }
                        case EntryState.Deleted:
                            {
                                if (create)
                                {
                                    createIndex = index;
                                    entries[createIndex].hash = hash;
                                }
                                break;
                            }
                    }
                }
                {
                    var startIndex = (hash & int.MaxValue) % entries.Length;
                    var index = entries[startIndex].next;
                    if (entries[index].hash != hash)
                    {
                        if (create)
                        {
                            if (entries[insertIndex].state != EntryState.Empty)
                            {
                                increase();
                                return find(key, true);
                            }
                            createIndex = insertIndex;
                            entries[startIndex].next = createIndex;
                            entries[createIndex].hash = hash;
                            if (count + 1 < entries.Length)
                                do
                                {
                                    insertIndex = (insertIndex + 1) % entries.Length;
                                }
                                while (entries[insertIndex].state != EntryState.Empty);
                            return createIndex;
                        }
                        return -1;
                    }
                    bool wasNil = index == 0;
                    while (entries[index].hash == hash)
                    {
                        if (string.CompareOrdinal(entries[index].key, key) == 0)
                            return index;
                        if (entries[index].next == 0)
                            if (wasNil)
                                break;
                            else
                                wasNil = true;
                        startIndex = index;
                        index = entries[index].next;
                    }
                    if (create)
                    {
                        if (entries[insertIndex].state != EntryState.Empty)
                        {
                            increase();
                            return find(key, true);
                        }
                        createIndex = insertIndex;
                        entries[startIndex].next = createIndex;
                        entries[createIndex].hash = hash;
                        if (count + 1 < entries.Length)
                            do
                            {
                                insertIndex = (insertIndex + 1) % entries.Length;
                            }
                            while (entries[insertIndex].state != EntryState.Empty);
                        return createIndex;
                    }
                    return -1;
                }
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
            KeyValuePair<string, TValue>[] oldData = new KeyValuePair<string, TValue>[count];
            for (int i = entries.Length, j = 0; i-- > 0; )
            {
                if (entries[i].state == EntryState.Filled)
                    oldData[j++] = new KeyValuePair<string, TValue>(entries[i].key, values[i]);
            }
            if (entries.Length <= ListLimit >> 1 || entries.Length >= ListLimit)
            {
                entries = new Entry[entries.Length << 1];
                values = new TValue[entries.Length + 1];
            }
            else
            {
                entries = new Entry[ListLimit];
                values = new TValue[ListLimit + 1];
            }
            count = 0;
            insertIndex = 0;
            for (var i = oldData.Length; i-- > 0; )
                Add(oldData[i].Key, oldData[i].Value);
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
            insertIndex = 0;
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
