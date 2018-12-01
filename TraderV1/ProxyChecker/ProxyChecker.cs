using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Addons;
using CustomWebClient;

namespace ProxyChecker
{
    public class ProxyChecker
    {
        private ILogger logger;

        public ProxyChecker(ILogger logger, string filePath = "list.txt")
        {
            this.filePath = filePath;
            this.logger = logger;
        }

        private string filePath;

        public string[] ProxyList {
            get
            {
                if (File.Exists(filePath))
                {
                    return File.ReadAllLines(filePath);
                }
                else
                {
                    throw new FileNotFoundException("Не удалось найти файл list.txt со списком прокси серверов");
                }
            }
        }

        public Proxy[] Check (string[] proxyList, string url)
        {
            logger.Log("Check proxy");
            List<Proxy> list = new List<Proxy>();

            foreach (var item in proxyList)
            {
                string[] proxyAsArray = item.Split(':');
                Proxy proxy = new Proxy() {IP = proxyAsArray[0], Port = proxyAsArray[1] };
                logger.Log("Check --> "+proxy.ToString());

                WebProxy webProxy = new WebProxy(item);
                CustomWebClient.CustomWebClient client = new CustomWebClient.CustomWebClient(logger, webProxy);

                try
                {
                    Stopwatch sw = new Stopwatch();
                    var res1 = client.Download(url);

                    sw.Start();
                    var res2 = client.Download(url);
                    sw.Stop();

                    proxy.Ping = sw.ElapsedMilliseconds;
                    proxy.IsWork = true;
                    sw.Reset();

                }
                catch (BadProxyException e)
                {
                    proxy.Ping = long.MaxValue;
                    proxy.IsWork = false;
                    logger.Log(e.Message);
                }
                catch (NeedableProxyException e)
                {
                    proxy.Ping = long.MaxValue;
                    proxy.IsWork = false;
                    logger.Log(e.Message);

                }
                catch (Exception e)
                {
                    proxy.Ping = long.MaxValue;
                    proxy.IsWork = false;
                    logger.Log(e.Message);

                }

                list.Add(proxy);
                logger.Log("-----------------------------------------------------------------------");
            }

            return list.ToArray();
        }
        
       
    }
}
