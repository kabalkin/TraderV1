using Addons;
using System;
using System.Net;
using System.Threading;

namespace CustomWebClient
{
    public class CustomWebClient
    {
        private ILogger logger;
        WebProxy webProxy = null;
        CookieContainer coockie = null;

        public CustomWebClient(ILogger logger, WebProxy webProxy)
        {
            this.logger = logger;
            this.webProxy = webProxy;
        }


        public string Download(string url)
        {
        Start:

        string result = "";

            try
            {
                if (coockie != null)
                {
                    WebClient modedWebClient = new WebClientEx(coockie);
                    modedWebClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0");
                    modedWebClient.Headers.Add("Referer", url);
                    if (webProxy != null)
                    {
                        modedWebClient.Proxy = webProxy;
                    }

                    try
                    {
                        result = modedWebClient.DownloadString(url);
                    }
                    catch (Exception ex)
                    {
                        //Send(ex.Message);
                        logger.Log(ex.Message);
                        //TODO 429 need proxy
                        coockie = null;
                        goto Start;
                    }
                }
                else
                {
                    coockieAgain:
                    coockie = GetCookie(url);
                    if (coockie == null)
                    {
                        //Send("cookieAgain after 10 sec");
                        logger.Log("cookieAgain after 10 sec");
                        Thread.Sleep(10000);
                        goto coockieAgain;
                    }
                    WebClient modedWebClient = new WebClientEx(coockie);
                    modedWebClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0");
                    modedWebClient.Headers.Add("Referer", url);

                    if (webProxy != null)
                    {
                        modedWebClient.Proxy = webProxy;
                    }

                    result = modedWebClient.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                logger.Log("Fix for proxy TODO");
                logger.Log(ex.Message);
                coockie = null;

                if (webProxy==null)
                {
                    throw new NeedableProxyException("Need add a proxy to request");
                }
                else
                {
                    throw new BadProxyException("Bad proxy server");
                }

              


                //Send("Fix for proxy TODO");
                //Send(ex.Message);
               // goto Start;
            }

            if (result == "Ddos, 20-50 mins.\n")
            {
                logger.Log("Ddos, 20-50 mins");
                //Send("Ddos, 20-50 mins");
                coockie = null;
                goto Start;
            }
            else
            {
                return result;
            }
        }

        public CookieContainer GetCookie(string url)
        {
            return new CloudflareEvader(logger).CreateBypassedWebClient(url, webProxy);
        }
    }
}
