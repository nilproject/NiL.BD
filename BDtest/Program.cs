using NiL.BD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BDtest
{
    class Program
    {
        private static int psrandmul = 137153;

        /// <summary>
        /// StringMap3
        /// </summary>
        private static void benchmark9(int size)
        {
            GC.Collect();
            var dictionary = new StringMap3<int>();
            var keys = new string[size];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = i.ToString();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < keys.Length; i++)
                dictionary.Add(keys[(long)i * psrandmul % keys.Length], (int)(((long)i * psrandmul) % keys.Length));
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            foreach (var t in dictionary)
            {
                // System.Diagnostics.Debugger.Break(); // порядок не соблюдается. Проверка бессмыслена
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            for (int i = 0; i < keys.Length; i++)
            {
                if (dictionary[keys[i]] != i)
                    throw new Exception();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        /// <summary>
        /// StringMap2
        /// </summary>
        private static void benchmark8(int size)
        {
            GC.Collect();
            var dictionary = new StringMap2<int>();
            var keys = new string[size];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = i.ToString();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < keys.Length; i++)
                dictionary.Add(keys[(long)i * psrandmul % keys.Length], (int)(((long)i * psrandmul) % keys.Length));
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            int c = 0;
            foreach (var t in dictionary)
            {
                if (t.Key.Length != 0)
                    c++;
                // System.Diagnostics.Debugger.Break(); // порядок не соблюдается. Проверка бессмыслена
            }
            if (c != dictionary.Count)
                System.Diagnostics.Debugger.Break();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            c = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                c++;
                if (dictionary[keys[i]] != i)
                    throw new Exception();
            }
            if (c != dictionary.Count)
                System.Diagnostics.Debugger.Break();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            Console.WriteLine(dictionary.stat0);
            Console.WriteLine(dictionary.stat1);
        }

        /// <summary>
        /// StringMap
        /// </summary>
        private static void benchmark7(int size)
        {
            GC.Collect();
            var dictionary = new StringMap<int>();
            var keys = new string[size];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = i.ToString();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < keys.Length; i++)
                dictionary.Add(keys[(long)i * psrandmul % keys.Length], (int)(((long)i * psrandmul) % keys.Length));
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            foreach (var t in dictionary)
            {
                // System.Diagnostics.Debugger.Break(); // порядок не соблюдается. Проверка бессмыслена
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            for (int i = 0; i < keys.Length; i++)
            {
                if (dictionary[keys[i]] != i)
                    throw new Exception();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            Console.WriteLine(StringMap<int>.missCount);
        }

        /// <summary>
        /// SparseArray
        /// </summary>
        private static void benchmark6()
        {
            GC.Collect();
            var array = new SparseArray<string>();
            var keys = new string[10000000];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = i.ToString();
            var sw = new Stopwatch();
            sw.Start();
            for (long i = 0; i < keys.Length; i++)
                array[(int)(i * psrandmul % keys.Length)] = keys[(int)(i * psrandmul % keys.Length)];
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            var index = 0;
            sw.Restart();
            foreach (var t in array)
            {
                if (t != keys[index++])
                    System.Diagnostics.Debugger.Break();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            for (int i = 0; i < keys.Length; i++)
            {
                if (array[i] != keys[i])
                    System.Diagnostics.Debugger.Break();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        /// <summary>
        /// IndexedDictionary
        /// </summary>
        private static void benchmark5()
        {
            GC.Collect();
            var tree = new IndexedDictionary<int, int>();
            tree[default(int)] = 0;
            tree.Clear();
            var keys = new int[10000000];
            for (int i = 0; i < 10000000; i++)
                keys[i] = i;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < keys.Length; i++)
                tree[keys[i]] = i;
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            foreach (var t in tree)
            {

            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            for (int i = 0; i < keys.Length; i++)
            {
                if (!tree.ContainsKey(keys[i]))
                    throw new Exception();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }
        /// <summary>
        /// SortedDictionary
        /// </summary>
        private static void benchmark3()
        {
            GC.Collect();
            var tree = new SortedDictionary<int, string>();
            var keys = new string[10000000];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = i.ToString();
            var sw = new Stopwatch();
            sw.Start();
            for (long i = 0; i < keys.Length; i++)
                tree[(int)(i * psrandmul % keys.Length)] = keys[i * psrandmul % keys.Length];
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            foreach (var t in tree)
            {

            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            for (int i = 0; i < keys.Length; i++)
            {
                if (tree[i] != keys[i])
                    throw new Exception();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        /// <summary>
        /// Dictionary
        /// </summary>
        private static void benchmark2(int size)
        {
            GC.Collect();
            var dictionary = new Dictionary<string, int>();
            var keys = new string[size];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = i.ToString();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < keys.Length; i++)
                dictionary.Add(keys[(long)i * psrandmul % keys.Length], (int)(((long)i * psrandmul) % keys.Length));
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            var c = 0;
            foreach (var t in dictionary)
            {
                if (t.Key.Length != 0)
                    c++;
                // System.Diagnostics.Debugger.Break(); // порядок не соблюдается. Проверка бессмыслена
            }
            if (c != dictionary.Count)
                System.Diagnostics.Debugger.Break();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            for (int i = 0; i < keys.Length; i++)
            {
                if (dictionary[keys[i]] != i)
                    throw new Exception();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        /// <summary>
        /// BinaryTree
        /// </summary>
        private static void benchmark()
        {
            GC.Collect();
            var tree = new BinaryTree<string, int>();
            tree["0"] = 0;
            tree.Clear();
            string[] keys = new string[10000000];
            for (int i = 0; i < 10000000; i++)
                keys[i] = i.ToString();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10000000; i++)
                tree[keys[i]] = i;
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            foreach (var t in tree)
            {

            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            for (int i = 0; i < 10000000; i++)
            {
                if (!tree.ContainsKey(keys[i]))
                    throw new Exception();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static void benchmarkStringComparision()
        {
            var sw = new Stopwatch();
            sw.Restart();
            for (int i = 0; i < 10000000; i++)
                string.CompareOrdinal("1234567890", "1234567890");
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static void indexedStorageTest()
        {
            //string dirname = "F:/Users/Дмитрий/Documents/testdb";
            //if (Directory.Exists(dirname))
            //    foreach (var file in Directory.EnumerateFiles(dirname))
            //        File.Delete(file);
            //Directory.CreateDirectory(dirname);
            //using (var bdata = new IndexedStorage<string>(dirname))
            //{
            //    var sw = new Stopwatch();
            //    sw.Start();
            //    for (int i = 0; i < 100000; i++)
            //    {
            //        var t = i.ToString();
            //        bdata.Add(t, t);
            //    }
            //    sw.Stop();
            //    Console.WriteLine(sw.Elapsed);
            //    sw.Restart();
            //    var test = bdata["19263"].Value;
            //    sw.Stop();
            //    Console.WriteLine(sw.Elapsed);
            //}
        }

        [Serializable]
        private struct DbItem
        {
            public int key;
            public string value;
        }

        static int isqrt(int n)
        {
            n = n * (1 - 2 * (n >> (sizeof(int) * 8 - 1)));
            if (n < 2)
                return n;
            var r = 0;
            var d = 1 << 3;
            while (d != 0)
            {
                do
                {
                    r += d;
                }
                while (r * r < n);
                r -= d;
                d >>= 1;
            }
            return r;
        }

        private static int getPrime(int min)
        {
            for (var i = min | 1; i < int.MaxValue; i += 2)
            {
                var j = isqrt(i) | 1;
                for (; j >= 3; j -= 2)
                {
                    if (i % j == 0)
                        break;
                }
                if (j == 1)
                    return i;
            }
            return 2;
        }

        private static int isPrime(int x)
        {
            for (var i = isqrt(x) | 1; i > 2; i -= 2)
                if (x % i == 0)
                    return 0;
            return x & 1;
        }

        static void Main(string[] args)
        {
            //var primes = new StringBuilder();
            //for (var i = 3; i < 10000000; i = getPrime(i + (i >> 2) + 3))
            //    primes.Append(i).Append(',');
            //var pv = primes.ToString();
            //Console.WriteLine(pv);
            //Console.WriteLine(getPrime(21));
            //return;

            if (false)
            {
                var sm2 = new StringMap2<string>();
                sm2["0"] = "world";
                sm2["1"] = "world1";
                sm2["2"] = "world2";
                sm2["3"] = "world3";
                sm2["4"] = "world4";
                sm2["5"] = "world5";
                sm2["6"] = "world6";
                sm2["7"] = "world7";
                sm2["8"] = "world8";
                Console.WriteLine(sm2["0"]);
                Console.WriteLine(sm2["1"]);
                Console.WriteLine(sm2["2"]);
                Console.WriteLine(sm2["3"]);
                Console.WriteLine(sm2["4"]);
                Console.WriteLine(sm2["5"]);
                Console.WriteLine(sm2["6"]);
                Console.WriteLine(sm2["7"]);
                Console.WriteLine(sm2["8"]);
            }

            for (var i = 3; i-- > 0; )
            {
                benchmark2(8000000);
                GC.Collect(0);
                GC.Collect(1);
                GC.Collect(2);
                GC.GetTotalMemory(true);
                benchmark8(8000000);
                Console.WriteLine("-------------------");
                GC.Collect(0);
                GC.Collect(1);
                GC.Collect(2);
                GC.GetTotalMemory(true);
            }
        }

        static void Main_(string[] args)
        {
            string dirname = "J:/Users/Дмитрий/Documents/testdb";
            if (Directory.Exists(dirname))
                foreach (var file in Directory.EnumerateFiles(dirname))
                    File.Delete(file);
            Directory.CreateDirectory(dirname);
            using (var bdata = new NamedStorage<string>(dirname))
            {
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 1000000; i-- > 0; )
                {
                    var t = i.ToString();
                    bdata.Add(t, t);
                }
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
                sw.Restart();
                foreach (var o in bdata)
                {
                    Console.WriteLine(o.Value);
                }
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
            }
        }
    }
}
