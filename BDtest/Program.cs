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

        private static void benchmark2()
        {
            GC.Collect();
            var tree = new Dictionary<string, int>();
            tree["0"] = 0;
            tree.Clear();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10000000; i++)
                tree[i.ToString()] = i;
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

        private static void benchmark()
        {
            GC.Collect();
            var tree = new BinaryTree<int>();
            tree["0"] = 0;
            tree.Clear();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10000000; i++)
                tree[i.ToString()] = i;
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            GC.GetTotalMemory(true);
            sw.Restart();
            foreach(var t in tree)
            {

            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            var test = tree["12345"];
            GC.GetTotalMemory(true);
            sw.Restart();
            test = tree["12345"];
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
            string dirname = "F:/Users/Дмитрий/Documents/testdb";
            if (Directory.Exists(dirname))
                foreach (var file in Directory.EnumerateFiles(dirname))
                    File.Delete(file);
            Directory.CreateDirectory(dirname);
            using (var bdata = new IndexedStorage<string>(dirname))
            {
                var sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < 100000; i++)
                {
                    var t = i.ToString();
                    bdata.Add(new IndexedStorage<string>.Record(t, t));
                }
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
                sw.Restart();
                var test = bdata["19263"].Value;
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
            }
        }

        static void Main(string[] args)
        {
            var bt = new BinaryTree<int>();
            for (int i = 0; i < 6; i++)
                bt.Add((9 - i).ToString(), i);
            bt.Remove("5");
        }
    }
}
