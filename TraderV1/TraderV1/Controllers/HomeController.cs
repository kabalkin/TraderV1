using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Addons;
using Jint.Native;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using TraderV1.BL;
using TraderV1.Models;
using ProxyChecker;
using TraderV1.Addons;

namespace TraderV1.Controllers
{        
    [Authorize]
    public class HomeController : Controller
    {
        private IHostingEnvironment _env;
        
        public HomeController(IHostingEnvironment env, IHubContext<MessagesHub> hubContext)
        {
            _env = env;
            _hubContext = hubContext;

        }
        
        public static void Send(string message)
        {
            _hubContext.Clients.All.SendAsync("Send", message);
        }
        
        public static void SendData(string message)
        {
            _hubContext.Clients.All.SendAsync("SendData", message);
        }
        
        private static  IHubContext<MessagesHub> _hubContext;

       
               
        
        public IActionResult Index()
        {
            ViewData["UserName"] = User.Identity.Name;

            return View();
        }
            
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }


        public bool Start()
        {
            flagToStop = false;   
            Send("Try Start Procces");
            
            Task.Run(() => { Run(); });
    
            return true;
        }
        
        
        public bool StopProcces()
        {
            flagToStop = true;   
            Send("TryStop Task");
            
            return true;
        }

      
        static bool flagToStop = false;
        private static CookieContainer _coockie = null;
        private static WebProxy webProxy = null;
        static decimal precentOut = 0;
        readonly AdditionalConsoleLogger conLogger = new AdditionalConsoleLogger(_hubContext);


        public static CookieContainer GetCookie(string valuets)
        {
            return CloudflareEvader.CreateBypassedWebClient("https://yobit.net/api/3/ticker/" + valuets, webProxy);
        }


        private void Run()
        {
            if (true)
            {
                string proxyFilePath = Path.Combine(_env.WebRootPath, "proxyServers.txt");         
                ProxyChecker.ProxyChecker checker = new ProxyChecker.ProxyChecker(conLogger, proxyFilePath);
                Proxy[] proxys = checker.Check(checker.ProxyList, "https://yobit.net/api/3/ticker/btc_usd");
                Proxy bestProxy = proxys.Where(s=>s.IsWork).OrderBy(s=>s.Ping).First();

                if (!bestProxy.IsWork)
                {
                    throw new BadProxyException("Нет подходящего прокси сервера, нужно обновить список");
                }


                foreach (var prox in proxys)
                {
                    conLogger.Log(prox.ToString() + "\t" + prox.IsWork + "\t" + prox.Ping + " ms");    
                }
                
                conLogger.Log("          *****          ");
                conLogger.Log("The best --> " + bestProxy.ToString() + "\t ping --> " + bestProxy.Ping);
                
                
                
                webProxy = new WebProxy(bestProxy.ToString());
            }
           
            
            string valuets = "btc_usd";

            //bool isBuy = comboBox1.SelectedIndex == 0;
            bool isBuy = true;

           // pairs[valuets] = new Pair(valuets, isBuy, decimal.Parse(textBox3.Text), 0);           

            Start:

            if (flagToStop)
            {
                goto End;
            }

            string result = "";
            
            try
            {                
                if (_coockie != null)
                {
                    WebClient modedWebClient = new WebClientEx(_coockie);
                    modedWebClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0");
                    modedWebClient.Headers.Add("Referer", "https://yobit.net/api/3/ticker/" + valuets);
                    if (webProxy != null)
                    {
                        modedWebClient.Proxy = webProxy;
                    }

                    try
                    {
                        result = modedWebClient.DownloadString("https://yobit.net/api/3/ticker/" + valuets);
                    }
                    catch (Exception ex)
                    {
                        Send(ex.Message);
                        //TODO 429 need proxy
                        _coockie = null;
                        goto Start;
                    }
                }
                else
                {
                    coockieAgain:
                    _coockie = GetCookie(valuets);
                    if (_coockie == null)
                    {
                        Send("cookieAgain after 10 sec");
                        Thread.Sleep(10000);
                        goto coockieAgain;
                    }
                    WebClient modedWebClient = new WebClientEx(_coockie);
                    modedWebClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0");
                    modedWebClient.Headers.Add("Referer", "https://yobit.net/api/3/ticker/" + valuets);

                    if (webProxy != null)
                    {
                        modedWebClient.Proxy = webProxy;
                    }

                    result = modedWebClient.DownloadString("https://yobit.net/api/3/ticker/" + valuets);
                }
            }
            catch (Exception ex)
            {
                Send("Fix for proxy TODO");
                Send(ex.Message);
                _coockie = null;
                goto Start;
            }

            if (result == "Ddos, 20-50 mins.\n")
            {
                Send("Ddos, 20-50 mins");
                _coockie = null;
                goto Start;
            }

            dynamic stuff = JsonConvert.DeserializeObject(result);

            if (stuff == null || stuff["success"] == "0")
            {
                if (stuff == null)
                {
                    Send("Неизвестная ошибка при получении данных");
                }
                else
                {
                    Send((string)(stuff["error"]));
                }
            }
            else
            {
                var item = new
                {
                    Name = valuets,
                    //Avg = (decimal)stuff[valuets]["avg"],
                    //Buy = (decimal)stuff[valuets]["buy"],
                    //High = (decimal)stuff[valuets]["high"],
                    //Last = (decimal)stuff[valuets]["last"],
                    //Low = (decimal)stuff[valuets]["low"],
                    Sell = (decimal)stuff[valuets]["sell"],
                    //Vol = (decimal)stuff[valuets]["vol"],
                    //Vol_Cur = (decimal)stuff[valuets]["vol_cur"],
                    //DateUpdate = (long)stuff[valuets]["updated"]
                };

                // pairs[item.Name].Update(item.Sell);

                
                var data = new {dateTome = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"), coast = item.Sell};
                string jsonString = JsonConvert.SerializeObject(data);
                
                SendData(jsonString);


              
                
               // precentOut = pairs[item.Name].PrecentCoast;

                



                //TODO фикс график прямых линий (мб добавлять в список)
//                if (firstCoastRun == 0)
//                {
//                    firstCoastRun = item.Sell;
//                    firstDateRun = DateTime.Now;
//                }
//
//                DrawGraph(new XDate(DateTime.Now), item.Sell);

               

                Thread.Sleep(1900);
                goto Start;
            }

            End:
            Send("Proccess was stoped");
            //list = null;
            //firstCoastRun = 0;
            _coockie = null;
          
        }

       

        // static Dictionary<string, Pair> pairs = new Dictionary<string, Pair>();

    }
}