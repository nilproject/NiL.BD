using NiL.BD;
using NiL.WBE.DataBase;
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
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            var tree = new BinaryTree<int>();
            for (int i = 0; i < 10000000; i++)
                tree[i.ToString("0000000")] = i;
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            /*foreach (var t in tree)
            {
                Console.WriteLine(t.Key + " " + t.Value);
            }*/
            /*
            string dirname = "I:/Users/Дмитрий/Documents/testdb";
            foreach (var file in Directory.EnumerateFiles(dirname))
                File.Delete(file);
            Directory.CreateDirectory(dirname);
            var bdata = new IndexedStorage<string>(dirname);
            var r = new Random(Environment.TickCount);
            for (int i = 0; i < 1000; i++)
                bdata.Add(new IndexedStorage<string>.Record(r.Next(65535).ToString("x2"), "Stres test"));
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine(bdata.Select("fefe").Count());
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            bdata.Dispose();
            */
        }
    }
}
