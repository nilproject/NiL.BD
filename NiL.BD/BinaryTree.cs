using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NiL.BD
{
    /// <summary>
    /// Предоставляет реализацию бинарного дерева поиска со строковым аргументом.
    /// </summary>
    [Serializable]
    public class BinaryTree<T> : IDictionary<string, T>
    {
        private sealed class _Values : ICollection<T>
        {
            private BinaryTree<T> owner;

            public _Values(BinaryTree<T> owner)
            {
                this.owner = owner;
            }

            public int Count { get { return owner.Count; } }
            public bool IsReadOnly { get { return true; } }

            public void Add(T item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(T item)
            {
                foreach (var i in owner)
                {
                    if (i.Value.Equals(item))
                        return true;
                }
                return false;
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                if (array.Length - arrayIndex < owner.Count)
                    throw new ArgumentOutOfRangeException();
                foreach (var i in owner)
                    array[arrayIndex++] = i.Value;
            }

            public bool Remove(T item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var kvp in owner)
                {
                    yield return kvp.Value;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (this as IEnumerable<T>).GetEnumerator();
            }
        }

        private sealed class _Keys : ICollection<string>
        {
            private BinaryTree<T> owner;

            public _Keys(BinaryTree<T> owner)
            {
                this.owner = owner;
            }

            public int Count { get { return owner.Count; } }
            public bool IsReadOnly { get { return true; } }

            public void Add(string item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(string item)
            {
                return owner.ContainsKey(item);
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                if (array.Length - arrayIndex < owner.Count)
                    throw new ArgumentOutOfRangeException();
                foreach (var i in owner)
                    array[arrayIndex++] = i.Key;
            }

            public bool Remove(string item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<string> GetEnumerator()
            {
                foreach (var kvp in owner)
                {
                    yield return kvp.Key;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (this as IEnumerable<string>).GetEnumerator();
            }
        }

        [Serializable]
        private sealed class Node
        {
            public string key = null;
            public T value = default(T);
            public Node less = null;
            public Node greater = null;
            public int height;

            private void rotateLtM(ref Node _this)
            {
                var temp = less.greater;
                less.greater = _this;
                _this = less;
                less = temp;
            }

            private void rotateMtL(ref Node _this)
            {
                var temp = greater.less;
                greater.less = _this;
                _this = greater;
                greater = temp;
            }

            private void validateHeight()
            {
                height = Math.Max(less != null ? less.height : 0, greater != null ? greater.height : 0) + 1;
            }

            public void Balance(ref Node _this)
            {
                int lessH = 0;
                int greaterH = 0;
                if (less != null)
                {
                    lessH = less.height;
                    if (lessH == 0)
                    {
                        less.Balance(ref less);
                        lessH = less.height;
                    }
                }
                if (greater != null)
                {
                    greaterH = greater.height;
                    if (greaterH == 0)
                    {
                        greater.Balance(ref greater);
                        greaterH = greater.height;
                    }
                }
                int delta = lessH - greaterH;
                if (delta > 1)
                {
                    int llessH = less.less != null ? less.less.height : 0;
                    int lgreaterH = less.greater != null ? less.greater.height : 0;
                    if (llessH < lgreaterH)
                    {
                        less.rotateMtL(ref less);
                        less.less.validateHeight();
                        less.validateHeight();
                    }
                    _this.rotateLtM(ref _this);
                    validateHeight();
                    _this.validateHeight();
                }
                else if (delta < -1)
                {
                    int mlessH = greater.less != null ? greater.less.height : 0;
                    int ggreaterH = greater.greater != null ? greater.greater.height : 0;
                    if (mlessH > ggreaterH)
                    {
                        greater.rotateLtM(ref greater);
                        greater.greater.validateHeight();
                        greater.validateHeight();
                    }
                    _this.rotateMtL(ref _this);
                    validateHeight();
                    _this.validateHeight();
                }
                else
                    height = Math.Max(less != null ? less.height : 0, greater != null ? greater.height : 0) + 1;
            }

            public override string ToString()
            {
                return key + ": " + value;
            }

            public Node()
            {
                height = 1;
            }
        }

        [NonSerialized]
        private Stack<Node> stack = new Stack<Node>();
        public int Height { get { return root.height; } }
        public int Count { get; private set; }
        public bool IsReadOnly { get { return false; } }
        [NonSerialized]
        private ICollection<string> keys;
        public ICollection<string> Keys { get { return keys ?? (keys = new _Keys(this)); } }
        [NonSerialized]
        private ICollection<T> values;
        public ICollection<T> Values { get { return values ?? (values = new _Values(this)); } }
        private Node root;

        public BinaryTree()
        {
            root = null;
            Count = 0;
        }

        public T this[string key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException();
                T res;
                if (!TryGetValue(key, out res))
                    throw new ArgumentException("Key not found.");
                return res;
            }
            set
            {
                if (key == null)
                    throw new ArgumentNullException();
                if (root == null)
                {
                    root = new Node() { value = value, key = key };
                    Count++;
                }
                else
                {
                    var c = root;
                    do
                    {
                        var cmp = string.Compare(key, c.key, StringComparison.Ordinal);
                        if (cmp == 0)
                        {
                            c.value = value;
                            return;
                        }
                        else if (cmp > 0)
                        {
                            if (c.greater == null)
                            {
                                c.greater = new Node() { key = key, value = value };
                                c.height = 0;
                                while (stack.Count != 0)
                                    stack.Pop().height = 0;
                                root.Balance(ref root);
                                Count++;
                                return;
                            }
                            stack.Push(c);
                            c = c.greater;
                        }
                        else if (cmp < 0)
                        {
                            if (c.less == null)
                            {
                                c.less = new Node() { key = key, value = value };
                                c.height = 0;
                                while (stack.Count != 0)
                                    stack.Pop().height = 0;
                                root.Balance(ref root);
                                Count++;
                                return;
                            }
                            stack.Push(c);
                            c = c.less;
                        }
                    }
                    while (true);
                }
            }
        }

        public void Clear()
        {
            Count = 0;
            root = null;
        }

        public void Add(KeyValuePair<string, T> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        public void Add(string key, T value)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (root == null)
            {
                root = new Node() { value = value, key = key };
                Count++;
            }
            else
            {
                var c = root;
                var stack = new Stack<Node>();
                do
                {
                    var cmp = string.Compare(key, c.key, StringComparison.Ordinal);
                    if (cmp == 0)
                        throw new ArgumentException();
                    else if (cmp > 0)
                    {
                        if (c.greater == null)
                        {
                            c.greater = new Node() { key = key, value = value };
                            c.height = 0;
                            while (stack.Count != 0)
                                stack.Pop().height = 0;
                            root.Balance(ref root);
                            Count++;
                            return;
                        }
                        stack.Push(c);
                        c = c.greater;
                    }
                    else if (cmp < 0)
                    {
                        if (c.less == null)
                        {
                            c.less = new Node() { key = key, value = value };
                            c.height = 0;
                            while (stack.Count != 0)
                                stack.Pop().height = 0;
                            root.Balance(ref root);
                            Count++;
                            return;
                        }
                        stack.Push(c);
                        c = c.less;
                    }
                }
                while (true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(string key, out T value)
        {
            if (root == null)
            {
                value = default(T);
                return false;
            }
            else
            {
                var c = root;
                do
                {
                    var cmp = string.CompareOrdinal(key, c.key);
                    if (cmp == 0)
                    {
                        value = c.value;
                        return true;
                    }
                    else if (cmp > 0)
                    {
                        if (c.greater == null)
                        {
                            value = default(T);
                            return false;
                        }
                        c = c.greater;
                    }
                    else
                    {
                        if (c.less == null)
                        {
                            value = default(T);
                            return false;
                        }
                        c = c.less;
                    }
                }
                while (true);
            }
        }

        public bool ContainsKey(string key)
        {
            T temp;
            return TryGetValue(key, out temp);
        }

        public bool Contains(KeyValuePair<string, T> keyValuePair)
        {
            T temp;
            return TryGetValue(keyValuePair.Key, out temp) && keyValuePair.Value.Equals(temp);
        }

        public bool Remove(string key)
        {
            Node prev = null;
            var c = root;
            var stack = new Stack<Node>();
            do
            {
                var cmp = string.CompareOrdinal(key, c.key);
                if (cmp == 0)
                {
                    if (c.greater == null)
                    {
                        if (prev == null)
                            root = c.less;
                        else
                        {
                            if (prev.greater == c)
                                prev.greater = c.less;
                            else
                                prev.less = c.less;
                        }
                    }
                    else if (c.less == null)
                    {
                        if (prev == null)
                            root = c.greater;
                        else
                        {
                            if (prev.greater == c)
                                prev.greater = c.greater;
                            else
                                prev.less = c.greater;
                        }
                    }
                    else
                    {
                        var caret = c.less;
                        if (caret.greater != null)
                        {
                            caret.height = 0;
                            var pcaret = c;
                            while (caret.greater != null)
                            {
                                pcaret = caret;
                                caret = caret.greater;
                                caret.height = 0;
                            }
                            pcaret.greater = caret.less;
                            caret.greater = c.greater;
                            caret.less = c.less;
                            if (prev == null)
                                root = caret;
                            else if (prev.greater == c)
                                prev.greater = caret;
                            else
                                prev.less = caret;
                        }
                        else
                        {
                            caret.height = 0;
                            caret.greater = c.greater;
                            if (prev == null)
                                root = caret;
                            else if (prev.greater == c)
                                prev.greater = caret;
                            else
                                prev.less = caret;
                        }
                    }
                    while (stack.Count > 0)
                        stack.Pop().height = 0;
                    root.Balance(ref root);
                    return true;
                }
                else if (cmp > 0)
                {
                    if (c.greater == null)
                        return false;
                    prev = c;
                    stack.Push(c);
                    c = c.greater;
                }
                else
                {
                    if (c.less == null)
                        return false;
                    prev = c;
                    stack.Push(c);
                    c = c.less;
                }
            }
            while (true);
        }

        public bool Remove(KeyValuePair<string, T> keyValuePair)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException();
            if (index < 0)
                throw new ArgumentOutOfRangeException();
            if (array.Length - index < Count)
                throw new ArgumentException();
            foreach (var kvp in this)
                array[index++] = kvp;
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            Node[] stack = new Node[root.height];
            int[] step = new int[root.height];
            int sindex = -1;
            if (root != null)
            {
                stack[++sindex] = root;
                while (sindex >= 0)
                {
                    if (step[sindex] == 0 && stack[sindex].less != null)
                    {
                        stack[sindex + 1] = stack[sindex].less;
                        step[sindex] = 1;
                        sindex++;
                        step[sindex] = 0;
                        continue;
                    }
                    if (step[sindex] < 2)
                    {
                        step[sindex] = 2;
                        yield return new KeyValuePair<string, T>(stack[sindex].key, stack[sindex].value);
                    }
                    if (step[sindex] < 3 && stack[sindex].greater != null)
                    {
                        stack[sindex + 1] = stack[sindex].greater;
                        step[sindex] = 3;
                        sindex++;
                        step[sindex] = 0;
                        continue;
                    }
                    sindex--;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<KeyValuePair<string, T>>).GetEnumerator();
        }
    }
}
