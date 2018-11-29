using Addons;
using System;
using System.IO;
using System.Reflection;

namespace TestApp
{
    class Program
    {
        static string url = "https://yobit.net/api/3/ticker/btc_usd";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ConsoleLogger logger = new ConsoleLogger();
            string filePath = Path.Combine(Environment.CurrentDirectory, "list.txt");

            ProxyChecker.ProxyChecker checker = new ProxyChecker.ProxyChecker(logger, filePath);

            var listProxy = checker.ProxyList;

            var res2 = checker.Check(listProxy, url);

            foreach (var item in res2)
            {
                Console.WriteLine(item.ToString() + "\t" + item.IsWork + "\t" + item.Ping + " ms");
            }
        }
    }
}
