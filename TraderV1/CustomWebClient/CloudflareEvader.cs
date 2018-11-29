using Addons;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;


namespace CustomWebClient
{
   public class CloudflareEvader
    {
        public CloudflareEvader(ILogger logger)
        {
            this.logger = logger;
        }

        private ILogger logger;

        public  CookieContainer CreateBypassedWebClient(string url, WebProxy proxy)
        {
            var JSEngine = new Jint.Engine();
            var uri = new Uri(url);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            if (proxy != null)
            {
                req.Proxy = proxy;
            }
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0";

            try
            {
                // req.GetResponse().Close();              
                var res = req.GetResponse(); //TODO:  proxxy
                string html = "";
                using (var reader = new StreamReader(res.GetResponseStream()))
                    html = reader.ReadToEnd();

                //TODO:   Program.messages.Add(Program.GetDateFormat(DateTime.Now) + "\tCloudflareEvader status --> OK\n", ConsoleColor.Magenta);
                // HomeController.Send("CloudflareEvader status --> OK");
                logger.Log("CloudflareEvader status --> OK");



                return new CookieContainer();
            }
            catch (WebException ex) //503
            {
                if (ex.Message == "Удаленный сервер возвратил ошибку: (429) Too Many Requests.")
                {
                    logger.Log("Have banned Sleep 10 minutes");
                    //HomeController.Send("Have banned Sleep 10 minutes");

                   
                   //TODO: Program.messages.Add(Program.GetDateFormat(DateTime.Now) + "\tHave banned Sleep 10 minutes\n", ConsoleColor.Red);
                    //TODO: Console.WriteLine("\nHave banned sleep 10 minutes");
                    Thread.Sleep(600000);
                }

                logger.Log(ex.Message);
                logger.Log("Try AntiDDoS CloudflareEvader | status --> Try");

                //HomeController.Send("ex.Message");
                //HomeController.Send("Try AntiDDoS CloudflareEvader | status --> Try");

               


                //TODO: Program.messages.Add(Program.GetDateFormat(DateTime.Now) + $"\t{ex.Message}\n", ConsoleColor.Red);
                //TODO:  Program.messages.Add(Program.GetDateFormat(DateTime.Now) + "\tTry AntiDDoS CloudflareEvader | status --> Try\n", ConsoleColor.Magenta);

                string html = "";
                using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                    html = reader.ReadToEnd();


                var cookie_container = new CookieContainer();
                var initial_cookies = GetAllCookiesFromHeader(ex.Response.Headers["Set-Cookie"], uri.Host);
                foreach (Cookie init_cookie in initial_cookies)
                    cookie_container.Add(init_cookie);

                var challenge = Regex.Match(html, "name=\"jschl_vc\" value=\"(\\w+)\"").Groups[1].Value;
                var challenge_pass = Regex.Match(html, "name=\"pass\" value=\"(.+?)\"").Groups[1].Value;

                var builder = Regex.Match(html, @"setTimeout\(function\(\){\s+(var s,t,o,p.+?\r?\n[\s\S]+?a\.value =.+?)\r?\n").Groups[1].Value;
                builder = Regex.Replace(builder, @"a\.value =(.+?) \+ .+?;", "$1");
                builder = Regex.Replace(builder, @"\s{3,}[a-z](?: = |\.).+", "");
                builder = Regex.Replace(builder, @"[\n\\']", "");
                builder = builder.Substring(0, builder.Length - 3);

                long solved = long.Parse(JSEngine.Execute(builder).GetCompletionValue().ToObject().ToString());
                solved += uri.Host.Length;

                Thread.Sleep(5000);

                string cookie_url = string.Format("{0}://{1}/cdn-cgi/l/chk_jschl", uri.Scheme, uri.Host);
                var uri_builder = new UriBuilder(cookie_url);
                var query = HttpUtility.ParseQueryString(uri_builder.Query);

                query["jschl_vc"] = challenge;
                query["jschl_answer"] = solved.ToString();
                query["pass"] = challenge_pass;
                uri_builder.Query = query.ToString();

                HttpWebRequest cookie_req = (HttpWebRequest)WebRequest.Create(uri_builder.Uri);
                if (proxy != null)
                {
                    cookie_req.Proxy = proxy;
                }
                cookie_req.AllowAutoRedirect = false;
                cookie_req.CookieContainer = cookie_container;
                cookie_req.Referer = url;
                cookie_req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0";
                var cookie_resp = (HttpWebResponse)cookie_req.GetResponse();

                if (cookie_resp.Cookies.Count != 0)
                    foreach (Cookie cookie in cookie_resp.Cookies)
                        cookie_container.Add(cookie);
                else
                {
                    if (cookie_resp.Headers["Set-Cookie"] != null)
                    {
                        var cookies_parsed = GetAllCookiesFromHeader(cookie_resp.Headers["Set-Cookie"], uri.Host);
                        foreach (Cookie cookie in cookies_parsed)
                            cookie_container.Add(cookie);
                    }
                    else
                    {
                        return null;
                    }
                }

                logger.Log("AntiDDoS CloudflareEvader | status --> Success");
                //HomeController.Send("AntiDDoS CloudflareEvader | status --> Success");

                
               //TODO:  Program.messages.Add(Program.GetDateFormat(DateTime.Now) + "\tTry AntiDDoS CloudflareEvader | status --> Success\n", ConsoleColor.Magenta);

                return cookie_container;
            }
        }


        public CookieCollection GetAllCookiesFromHeader(string strHeader, string strHost)
        {
            ArrayList al = new ArrayList();
            CookieCollection cc = new CookieCollection();
            if (strHeader != string.Empty)
            {
                al = ConvertCookieHeaderToArrayList(strHeader);
                cc = ConvertCookieArraysToCookieCollection(al, strHost);
            }

            return cc;
        }


        private  ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
        {
            strCookHeader = strCookHeader.Replace("\r", "");
            strCookHeader = strCookHeader.Replace("\n", "");
            string[] strCookTemp = strCookHeader.Split(',');
            ArrayList al = new ArrayList();
            int i = 0;
            int n = strCookTemp.Length;
            while (i < n)
            {
                if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    al.Add(strCookTemp[i] + "," + strCookTemp[i + 1]);
                    i = i + 1;
                }
                else
                    al.Add(strCookTemp[i]);
                i = i + 1;
            }

            return al;
        }


        private  CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
        {
            CookieCollection cc = new CookieCollection();

            int alcount = al.Count;
            string strEachCook;
            string[] strEachCookParts;
            for (int i = 0; i < alcount; i++)
            {
                strEachCook = al[i].ToString();
                strEachCookParts = strEachCook.Split(';');
                int intEachCookPartsCount = strEachCookParts.Length;
                string strCNameAndCValue = string.Empty;
                string strPNameAndPValue = string.Empty;
                string strDNameAndDValue = string.Empty;
                string[] NameValuePairTemp;
                Cookie cookTemp = new Cookie();

                for (int j = 0; j < intEachCookPartsCount; j++)
                {
                    if (j == 0)
                    {
                        strCNameAndCValue = strEachCookParts[j];
                        if (strCNameAndCValue != string.Empty)
                        {
                            int firstEqual = strCNameAndCValue.IndexOf("=");
                            string firstName = strCNameAndCValue.Substring(0, firstEqual);
                            string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
                            cookTemp.Name = firstName;
                            cookTemp.Value = allValue;
                        }

                        continue;
                    }
                    if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');
                            if (NameValuePairTemp[1] != string.Empty)
                                cookTemp.Path = NameValuePairTemp[1];
                            else
                                cookTemp.Path = "/";
                        }

                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');

                            if (NameValuePairTemp[1] != string.Empty)
                                cookTemp.Domain = NameValuePairTemp[1];
                            else
                                cookTemp.Domain = strHost;
                        }

                        continue;
                    }
                }

                if (cookTemp.Path == string.Empty)
                    cookTemp.Path = "/";
                if (cookTemp.Domain == string.Empty)
                    cookTemp.Domain = strHost;
                cc.Add(cookTemp);
            }

            return cc;
        }
    }
    
    public class WebClientEx : WebClient
    {
        public WebClientEx(CookieContainer container)
        {
            this.container = container;
        }


        public CookieContainer CookieContainer
        {
            get { return container; }
            set { container = value; }
        }


        private CookieContainer container = new CookieContainer();


        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }

            return r;
        }


        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }


        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);

            return response;
        }


        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }
}