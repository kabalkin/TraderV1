using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jint.Native;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using TraderV1.BL;
using TraderV1.Models;

namespace TraderV1.Controllers
{        
    [Authorize]
    public class HomeController : Controller
    {
        public static void Send(string message)
        {
            _hubContext.Clients.All.SendAsync("Send", message);
        }
        
        public static void SendData(string message)
        {
            _hubContext.Clients.All.SendAsync("SendData", message);
        }
        
        private static  IHubContext<MessagesHub> _hubContext;

        public HomeController(IHubContext<MessagesHub> hubContext)
        {
            _hubContext = hubContext;
        }
               
        
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
        public static CookieContainer coockie = null;
        private static WebProxy webProxy = new WebProxy("46.63.58.139:53281");
        static decimal precentOut = 0;


        public static CookieContainer GetCookie(string valuets)
        {
            return CloudflareEvader.CreateBypassedWebClient("https://yobit.net/api/3/ticker/" + valuets, webProxy);
        }


        private void Run()
        {    
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
                if (coockie != null)
                {
                    WebClient modedWebClient = new WebClientEx(coockie);
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
                        coockie = null;
                        goto Start;
                    }
                }
                else
                {
                    coockieAgain:
                    coockie = GetCookie(valuets);
                    if (coockie == null)
                    {
                        Send("cookieAgain after 10 sec");
                        Thread.Sleep(10000);
                        goto coockieAgain;
                    }
                    WebClient modedWebClient = new WebClientEx(coockie);
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
                coockie = null;
                goto Start;
            }

            if (result == "Ddos, 20-50 mins.\n")
            {
                Send("Ddos, 20-50 mins");
                coockie = null;
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
                //TODO: возможно ускориться если к типизированному объекту ItemYobit
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

                DateTime date = DateTime.Now;
                var data = new {dateTome = date, coast = item.Sell};
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
            coockie = null;
          
        }

       

        // static Dictionary<string, Pair> pairs = new Dictionary<string, Pair>();

    }
}