using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.BD
{
    public unsafe sealed class Btree<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private struct _Item
        {
            public int hash;
            public TKey key;
            public TValue value;
            /// <summary>
            /// less
            /// </summary>
            public int childs;
        }

        private const int levelSize = 6;

        private int alocatedLevels;
        private _Item[] data;
        private int rootIndex;

        public Btree()
        {
            alocatedLevels = 1;
            data = new _Item[levelSize];
            for (var i = 0; i < levelSize; i++)
            {
                data[i].childs = -1;
                data[i].hash = -1;
            }
        }

        private void insert(TKey key, TValue value, bool @throw)
        {
            var hash = unchecked(key.GetHashCode() & ((1 << 31) - 1));
            bool inserted = false;
            int index = rootIndex;
            while (!inserted)
            {
                for (int i = levelSize; i-- > 0; )
                {
                    if (data[index].hash < 0)
                    {
                        inserted = true;
                        data[index].hash = hash;
                        data[index].key = key;
                        data[index].value = value;
                    }
                    if (data[index].hash < hash)
                    {
                        if (data[index - 1].childs == -1)
                        {
                            // TODO
                        }
                        else
                        {
                            index = data[index - 1].childs;
                            break;
                        }
                    }
                    index++;
                }
            }
        }

        private int alocateLevel()
        {
            if (data.Length == alocatedLevels * levelSize)
            {
                var newdata = new _Item[data.Length * 2];
                var i = 0;
                for (; i < data.Length; i++)
                    newdata[i] = data[i];
                for (; i < newdata.Length; i++)
                {
                    newdata[i].hash = -1;
                    newdata[i].childs = -1;
                }
                data = newdata;
            }
            throw new NotImplementedException();
        }

        #region Члены IDictionary<TKey,TValue>

        public void Add(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(TKey key)
        {
            throw new NotImplementedException();
        }

        public ICollection<TKey> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        public ICollection<TValue> Values
        {
            get { throw new NotImplementedException(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Члены ICollection<KeyValuePair<TKey,TValue>>

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Члены IEnumerable<KeyValuePair<TKey,TValue>>

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Члены IEnumerable

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
