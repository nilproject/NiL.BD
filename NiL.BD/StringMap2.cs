using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.BD
{
    public sealed class StringMap2<TValue> : IDictionary<string, TValue>
    {
        private struct Record
        {
            public int next;
            public int hash;
            public string key;
            public TValue value;

#if DEBUG
            public override string ToString()
            {
                return "[" + key + ", " + value + "]";
            }
#endif
        }

        //private static readonly int[] primes = new[] { 3, 7, 11, 17, 29, 41, 59, 79, 101, 131, 167, 211, 269, 347, 439, 557, 701, 881, 1109, 1399, 1753, 2203, 2767, 3461, 4337, 5431, 6791, 8501, 10631, 13291, 16619, 20789, 25997, 32503, 40637, 50821, 63533, 79423, 99289, 124121, 155161, 193957, 242449, 303073, 378869, 473597, 592019, 740041, 925063, 1156333, 1445419, 1806781, 2258479, 2823101, 3528887, 4411117, 5513917, 6892477, 8615623 };

        //static int isqrt(int n)
        //{
        //    n = n * (1 - 2 * (n >> (sizeof(int) * 8 - 1)));
        //    if (n < 2)
        //        return n;
        //    var r = 0;
        //    var d = 1 << 3;
        //    while (d != 0)
        //    {
        //        do
        //        {
        //            r += d;
        //        }
        //        while (r * r < n);
        //        r -= d;
        //        d >>= 1;
        //    }
        //    return r;
        //}

        //private static int getPrime(int min)
        //{
        //    if (min < primes[primes.Length - 1])
        //        for (var i = 0; i < primes.Length; i++)
        //            if (primes[i] >= min)
        //                return primes[i];
        //    for (var i = min | 1; i < int.MaxValue; i += 2)
        //    {
        //        var j = isqrt(i);
        //        for (; j >= 3; j -= 2)
        //        {
        //            if (i % j == 0)
        //                break;
        //        }
        //        if (j == 1)
        //            return i;
        //    }
        //    return 2;
        //}

        private static readonly Record[] emptyRecords = new Record[0];

        private bool emptyKeyValueExists = false;
        private TValue emptyKeyValue;

        private Record[] records = emptyRecords;
        private int[] existedIndexes;

        private int count;
        private int eicount;

        private void insert(string key, TValue value, int hash, bool @throw, bool resizeMode)
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
            var elen = records.Length - 1;
            if (records.Length == 0)
                elen = increaseSize() - 1;
            int index;
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
            }
            while (index >= 0);
            // не нашли

            if (
                (count > 50 && count * 9 / 5 >= elen) ||
                count == elen + 1)
                elen = increaseSize() - 1;
            int prewIndex = -1;
            index = hash & elen;

            if (records[index].key != null)
            {
                stat0++;
                while (records[index].next > 0)
                {
                    stat1++;
                    index = records[index].next - 1;
                    colisionCount++;
                }
                prewIndex = index;
                while (records[index].key != null)
                    index = (index + 61) & elen;
            }
            records[index].hash = hash;
            records[index].key = key;
            records[index].value = value;
            if (prewIndex >= 0)
                records[prewIndex].next = index + 1;
            if (eicount == existedIndexes.Length)
            {
                var newEI = new int[existedIndexes.Length << 1];
                Array.Copy(existedIndexes, newEI, existedIndexes.Length);
                existedIndexes = newEI;
            }
            existedIndexes[eicount++] = index;
            count++;

            if (colisionCount > 17)
                increaseSize();
        }

        public int stat0;
        public int stat1;

        private static int computeHash(string key)
        {
            int hash;
            var keyLen = key.Length;
            hash = keyLen * 0x51 ^ 0xe5b5e5;
            for (var i = 0; i < keyLen; i++)
                hash += (hash >> 28) + (hash << 4) + key[i];
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
            var elen = records.Length - 1;
            int hash;
            int index;
            hash = key[0];
            index = hash & elen;
            hash = computeHash(key);
            //hash = key.GetHashCode();
            for (index = hash & elen; index >= 0; index = records[index].next - 1)
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
            /*
             * Нужно найти удаляемую запись, пометить её пустой и передвинуть всю цепочку next на один элемент назад по списку next.
             * При этом возможны ситуации когда элемент встанет на свою законную позицию (при вставке была псевдоколлизия).
             * в таком случае нужно убрать его из цепочки и таким образом уменьшить список коллизии.
             */
            if (key == null)
                throw new ArgumentNullException();
            if (key.Length == 0)
            {
                if (!emptyKeyValueExists)
                {
                    return false;
                }
                emptyKeyValue = default(TValue);
                emptyKeyValueExists = false;
                return true;
            }
            if (records.Length == 0)
                return false;
            var elen = records.Length - 1;
            int hash;
            int index;
            int targetIndex = -1, prewIndex;
            hash = key[0];
            index = hash & elen;
            hash = computeHash(key);
            //hash = key.GetHashCode();
            for (index = hash & elen; index >= 0; index = records[index].next - 1)
            {
                if (records[index].hash == hash && string.CompareOrdinal(records[index].key, key) == 0)
                {
                    if (records[index].next > 0)
                    {
                        prewIndex = targetIndex;
                        targetIndex = index;
                        index = records[index].next - 1;
                        do
                        {
                            if ((records[index].hash & elen) >= targetIndex)
                            {
                                records[targetIndex] = records[index];
                                records[targetIndex].next = index + 1;
                                prewIndex = targetIndex;
                                targetIndex = index;
                            }
                            index = records[index].next - 1;
                        }
                        while (index >= 0);
                        records[targetIndex].key = null;
                        records[targetIndex].value = default(TValue);
                        records[targetIndex].hash = 0;
                        if (prewIndex >= 0)
                            records[prewIndex].next = 0;
                    }
                    else
                    {
                        records[index].key = null;
                        records[index].value = default(TValue);
                        records[index].hash = 0;
                        if (targetIndex >= 0)
                            records[targetIndex].next = 0;
                    }
                    return true;
                }
                prewIndex = targetIndex;
                targetIndex = index;
            }
            return false;
        }

        private int increaseSize()
        {
            if (records.Length == 0)
            {
                records = new Record[1];
                existedIndexes = new int[1];
                return 1;
            }
            //if (count > 100 && records.Length / count > 50)
            //    throw new InvalidOperationException();
            var oldRecords = records;
            records = new Record[records.Length << 1];
            int i = 0, c = eicount;
            count = 0;
            eicount = 0;
            for (; i < c; i++)
            {
                var index = existedIndexes[i];
                if (oldRecords[index].key != null)
                    insert(oldRecords[index].key, oldRecords[index].value, oldRecords[index].hash, false, true);
            }
            return records.Length;
        }

        public void Add(string key, TValue value)
        {
            insert(key, value, computeHash(key), true, false);
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
                insert(key, value, computeHash(key), false, false);
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
            if (emptyKeyValueExists)
                yield return new KeyValuePair<string, TValue>("", emptyKeyValue);
            for (int i = 0; i < eicount; i++)
            {
                if (records[existedIndexes[i]].key != null)
                    yield return new KeyValuePair<string, TValue>(records[existedIndexes[i]].key, records[existedIndexes[i]].value);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
