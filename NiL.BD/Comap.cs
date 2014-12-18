using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NiL.BD
{
    public sealed class Comap<TKey, TValue> : IDictionary<TKey, TValue>, IList<TValue>
    {
        private struct _Bucket
        {
            public int hash;
            public int next;
            public int index;
            public TKey key;
        }

        private static readonly _Bucket[] emptyBuckets = new _Bucket[0];
        private static readonly TValue[] emptyValues = new TValue[0];

        private readonly int blockSize = Marshal.SizeOf<_Bucket>() * 256;
        private TValue[] values;
        private _Bucket[] indexes;
        private int count;

        public Comap()
        {
            values = emptyValues;
            indexes = emptyBuckets;
        }

        private void insert(TKey key, TValue value, bool @throw)
        {

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

        #region Члены IList<TValue>

        public int IndexOf(TValue item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, TValue item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public TValue this[int index]
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

        #region Члены ICollection<TValue>

        public void Add(TValue item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(TValue item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TValue item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Члены IEnumerable<TValue>

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
