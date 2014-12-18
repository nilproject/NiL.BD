#define INVERSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.BD
{
    public sealed class IndexedDictionarySB<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private sealed class ValueCollection : ICollection<TValue>
        {
            private IndexedDictionarySB<TKey, TValue> owner;

            public ValueCollection(IndexedDictionarySB<TKey, TValue> owner)
            {
                this.owner = owner;
            }

            #region Члены ICollection<TValue>

            public void Add(TValue item)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(TValue item)
            {
                for (var i = owner.items.Length; i-- > 0; )
                {
                    if (object.Equals(owner.items[i].value, item) && owner.ContainsKey(owner.items[i].key))
                        return true;
                }
                return false;
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(TValue item)
            {
                throw new InvalidOperationException();
            }

            #endregion

            #region Члены IEnumerable<TValue>

            public IEnumerator<TValue> GetEnumerator()
            {
                foreach (var i in owner)
                    yield return i.Value;
            }

            #endregion

            #region Члены IEnumerable

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach (var i in owner)
                    yield return i.Value;
            }

            #endregion
        }

        private sealed class KeyCollection : ICollection<TKey>
        {
            private IndexedDictionarySB<TKey, TValue> owner;

            public KeyCollection(IndexedDictionarySB<TKey, TValue> owner)
            {
                this.owner = owner;
            }

            #region Члены ICollection<TKey>

            public void Add(TKey item)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(TKey item)
            {
                return owner.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(TKey item)
            {
                throw new InvalidOperationException();
            }

            #endregion

            #region Члены IEnumerable<TKey>

            public IEnumerator<TKey> GetEnumerator()
            {
                foreach (var i in owner)
                    yield return i.Key;
            }

            #endregion

            #region Члены IEnumerable

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach (var i in owner)
                    yield return i.Key;
            }

            #endregion
        }

        private sealed class _ListNode
        {
            public _ListNode next;
            public int index;
        }

        [Serializable]
        private struct _Node
        {
            public int hash;
            public int _0;
            public int _1;
            public TKey key;
            public TValue value;
            /// <summary>
            /// For linked list implementation
            /// </summary>
            public _ListNode list;

            public override string ToString()
            {
                return "[" + key + ", " + value + "]";
            }
        }

        private _Node[] items;
        private List<int> emptyIndexes;
        private int root;
        private int size;
        private IComparer<TKey> comparer;
        private static readonly _Node[] emptyArray = new _Node[0];

        public IndexedDictionarySB()
        {
            if (!typeof(IComparable).IsAssignableFrom(typeof(TKey)))
                throw new ArgumentException(typeof(TKey) + " do not immplement IComparable interface");
            comparer = Comparer<TKey>.Default;
            items = emptyArray;
            root = -1;
        }

        private int findIndex(TKey key)
        {
            if (root == -1)
                return -1;
            var hash = key.GetHashCode();
            var node = root;
#if INVERSE
            for (var i = sizeof(int) * 8; node != -1 && i-- > 0; )
#else
            for (var i = 0; node != null && i < sizeof(int) * 8; i++)
#endif
            {
                if (items[node].hash == hash)
                    break;
                if ((hash & (1 << i)) == 0)
                    node = items[node]._0;
                else
                    node = items[node]._1;
            }
            if (node == -1)
                return -1;
            if (comparer.Compare(items[node].key, key) != 0)
            {
                var listItem = items[node].list;
                while (listItem != null && comparer.Compare(items[listItem.index].key, key) != 0)
                    listItem = listItem.next;
                if (listItem == null)
                    return -1;
                return listItem.index;
            }
            else
                return node;
        }

        private void insert(TKey key, TValue value, bool @throw)
        {
            int prewNode = -1;
            _ListNode prew = null;
            _ListNode listItem = null;
            var node = root;
            var hash = key.GetHashCode();
            if (root == -1)
            {
                placeKVPair(ref root, hash, key, value);
                return;
            }
#if INVERSE
            var i = sizeof(int) * 8;
            for (; node != -1 && i-- >= 0; )
#else
            var i = 0;
            for (; node != null && i <= sizeof(int) * 8; i++)
#endif
            {
                if (items[node].hash == hash)
                {
                    if (comparer.Compare(items[node].key, key) != 0)
                    {
                        listItem = items[node].list;
                        while (listItem != null && comparer.Compare(items[listItem.index].key, key) != 0)
                        {
                            prew = listItem;
                            listItem = listItem.next;
                        }
                        if (listItem == null)
                        {
                            if (prew == null)
                                placeKVPair(ref items[node].list, hash, key, value);
                            else
                                placeKVPair(ref prew.next, hash, key, value);
                        }
                        else
                        {
                            if (@throw)
                                throw new InvalidOperationException();
                            items[listItem.index] = new _Node() { value = value, key = key };
                        }
                    }
                    else
                    {
                        if (@throw)
                            throw new InvalidOperationException();
                        items[node].value = value;
                    }
                    return;
                }
#if INVERSE
                if (i == -1)
                {
                    i++;
#else
                if (i == sizeof(int) * 8)
                {
#endif
                    break;
                }
                prewNode = node;
                if ((hash & (1 << i)) == 0)
                    node = items[node]._0;
                else
                    node = items[node]._1;
            }
            // here node is null reference
#if DEBUG
            if (node != -1)
                System.Diagnostics.Debugger.Break();
#endif
#if INVERSE
            if ((hash & (1 << i)) == 0)
#else
            if ((hash & (1 << --i)) == 0)
#endif
                placeKVPair(ref items[prewNode]._0, hash, key, value);
            else
                placeKVPair(ref items[prewNode]._1, hash, key, value);
        }

        private void placeKVPair(ref int dest, int hash, TKey key, TValue value)
        {
            dest = popEmptyIndex();
            items[dest] = new _Node()
            {
                hash = hash,
                key = key,
                value = value
            };
        }

        private void placeKVPair(ref _ListNode dest, int hash, TKey key, TValue value)
        {
            dest = new _ListNode()
            {
                index = popEmptyIndex()
            };
            items[dest.index] = new _Node() { key = key, value = value };
        }

        private int popEmptyIndex()
        {
            if (emptyIndexes != null)
            {
                int res = emptyIndexes[emptyIndexes.Count - 1];
                if (emptyIndexes.Count == 1)
                    emptyIndexes = null;
                else
                    emptyIndexes.RemoveAt(emptyIndexes.Count - 1);
                size++;
                return res;
            }
            if (size == items.Length)
            {
                var newItems = new _Node[Math.Max(2, items.Length * 2)];
                for (var i = 0; i < items.Length; i++)
                    newItems[i] = items[i];
                items = newItems;
            }
            size++;
            return size - 1;
        }

        #region Члены IDictionary<TKey,TValue>

        public void Add(TKey key, TValue value)
        {
            insert(key, value, true);
        }

        public bool ContainsKey(TKey key)
        {
            return findIndex(key) != -1;
        }

        public ICollection<TKey> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(TKey key)
        {
            if (root == -1)
                return false;
            var hash = key.GetHashCode();
            var node = root;
#if INVERSE
            var i = sizeof(int) * 8;
            for (; node != -1 && i-- > 0; )
#else
            var i = 0;
            for (; node != null && i < sizeof(int) * 8; i++)
#endif
            {
                if (items[node].hash == hash)
                    break;
                if ((hash & (1 << i)) == 0)
                    node = items[node]._0;
                else
                    node = items[node]._1;
            }
            if (node == -1)
                return false;
            if (comparer.Compare(items[node].key, key) != 0)
            {
                _ListNode prewList = null;
                var listItem = items[node].list;
                while (listItem != null && comparer.Compare(items[listItem.index].key, key) != 0)
                {
                    prewList = listItem;
                    listItem = listItem.next;
                }
                if (listItem == null)
                    return false;
                if (prewList == null)
                    items[node].list = null;
                else
                    prewList.next = listItem.next;
                pushEmptyIndex(listItem.index);
            }
            else
            {
                pushEmptyIndex(node);
                if (items[node].list != null)
                {
                    var li = items[node].list.index;
                    items[node].key = items[li].key;
                    items[node].list = items[li].list;
                    items[node].value = items[li].value;
                }
                //else
                //    node.index = -1; // make zombie
            }
            size--;
            return true;
        }

        private void pushEmptyIndex(int index)
        {
            items[index] = default(_Node);
            (emptyIndexes ?? (emptyIndexes = new List<int>())).Add(index);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            var index = findIndex(key);
            if (index == -1)
                return false;
            value = items[index].value;
            return true;
        }

        public ICollection<TValue> Values
        {
            get { throw new NotImplementedException(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                var index = findIndex(key);
                if (index == -1)
                    throw new KeyNotFoundException();
                return items[index].value;
            }
            set
            {
                insert(key, value, false);
            }
        }

        #endregion

        #region Члены ICollection<KeyValuePair<TKey,TValue>>

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            size = 0;
            items = emptyArray;
            emptyIndexes = null;
            root = -1;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue value = default(TValue);
            return TryGetValue(item.Key, out value) && object.Equals(value, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return size; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region Члены IEnumerable<KeyValuePair<TKey,TValue>>

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (root == -1 || size == 0)
                yield break;
            var stack = new int[33];
            var stepstack = new byte[33];
            int stackIndex = 0;
            var node = root;
            for (; ; )
            {
                if ((stepstack[stackIndex] & 1) == 0)
                {
                    while (items[node]._1 != -1)
                    {
                        stepstack[stackIndex] |= 1;
                        stack[stackIndex++] = node;
                        stepstack[stackIndex] = 0;
                        node = items[node]._1;
                    }
                }
                if ((stepstack[stackIndex] & 2) == 0)
                {
                    if (items[node]._0 != -1)
                    {
                        stepstack[stackIndex] |= 2;
                        stack[stackIndex++] = node;
                        stepstack[stackIndex] = 0;
                        node = items[node]._0;
                        continue;
                    }
                }
                if ((stepstack[stackIndex] & 4) == 0)
                {
                    stepstack[stackIndex] |= 4;
                    yield return new KeyValuePair<TKey, TValue>(items[node].key, items[node].value);
                    var list = items[node].list;
                    while (list != null)
                    {
                        yield return new KeyValuePair<TKey, TValue>(items[list.index].key, items[list.index].value);
                        list = list.next;
                    }
                }
                if (stackIndex == 0)
                    yield break;
                node = stack[--stackIndex];
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
