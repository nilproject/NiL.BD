﻿using System;
using System.Collections.Generic;
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
            var elen = records.Length - 1;
            if (records.Length == 0)
                elen = increaseSize() - 1;
            int hash;
            int index;
            hash = computeHash(key);
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
                    index = (index + 87) & elen;
            }
            records[index].hash = hash;
            records[index].key = key;
            records[index].value = value;
            if (prewIndex >= 0)
                records[prewIndex].next = index + 1;
            count++;

            if (colisionCount > 14)
                increaseSize();
        }

        public int stat0;
        public int stat1;

        private static int computeHash(string key)
        {
            int hash;
            var keyLen = key.Length;
            hash = (keyLen * 0x51) ^ 0xecb901;
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
            return false;
        }

        private int increaseSize()
        {
            if (records.Length == 0)
            {
                records = new Record[1];
                return 1;
            }
            //if (count > 100 && records.Length / count > 50)
            //    throw new InvalidOperationException();
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
            return records.Length;
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