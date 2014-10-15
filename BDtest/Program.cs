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

        private static void benchmark4()
        {
            GC.Collect();
            var tree = new SortedDictionary<string, int>();
            tree["0"] = 0;
            tree.Clear();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10000000; i++)
            {
                var temp = i.ToString();
                tree[temp] = i;
            }
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
            var test = tree["12345"];
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static void benchmark3()
        {
            GC.Collect();
            var tree = new System.Collections.Specialized.StringDictionary();
            tree["0"] = "0";
            tree.Clear();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10000000; i++)
            {
                var temp = i.ToString();
                tree[temp] = temp;
            }
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
            var test = tree["12345"];
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        /// <summary>
        /// Dictionary
        /// </summary>
        private static void benchmark2()
        {
            GC.Collect();
            var tree = new Dictionary<int, int>();
            tree[default(int)] = 0;
            tree.Clear();
            var keys = new int[10000000];
            for (int i = 0; i < 10000000; i++)
                keys[i] = i;
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
                var j = isqrt(i);
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
            //benchmark();
            //benchmark2();
            //benchmark5();
            var id = new IndexedDictionary<int, int>();
            var index = 0;
            for (; index < 100; index++)
            {
                id.Add(index
                    //* 137 % 10000
                    , isPrime(index));
            }
            foreach (var i in id)
            {
                index--;
                Console.WriteLine(index + " " + i + " " + isPrime(index));
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
