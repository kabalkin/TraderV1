using Addons;
using ProxyChecker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace TestApp
{
    class Program
    {
        static string url = "https://yobit.net/api/3/ticker/btc_usd";

        static void Main(string[] args)
        {            
            ConsoleLogger logger = new ConsoleLogger();
            string filePath = Path.Combine(Environment.CurrentDirectory, "list.txt");

            ProxyChecker.ProxyChecker checker = new ProxyChecker.ProxyChecker(logger, filePath);

            var listProxy = checker.ProxyList;

            var res2 = checker.Check(listProxy, url);

            foreach (var item in res2)
            {
                Console.WriteLine(item.ToString() + "\t" + item.IsWork + "\t" + item.Ping + " ms");                
            }

           var  res = res2.Where(s=>s.IsWork).OrderBy(s=>s.Ping).First();


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("The best --> " + res.ToString() + "\t ping --> " + res.Ping);
            Console.ForegroundColor = ConsoleColor.Gray;
            
        }
    }
}
