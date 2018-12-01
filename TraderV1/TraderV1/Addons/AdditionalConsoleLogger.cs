using System;
using Addons;
using Microsoft.AspNetCore.SignalR;
using TraderV1.Models;

namespace TraderV1.Addons
{
    public class AdditionalConsoleLogger:ILogger
    {
        private IHubContext<MessagesHub> _hubContext;
        
        public AdditionalConsoleLogger(IHubContext<MessagesHub>  hubContext)
        {
            _hubContext = hubContext;
        }


        public void Log(string message)
        {
            string resultMessage = "[" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + "]\t" + message;

            _hubContext.Clients.All.SendAsync("SendToConsole", resultMessage);
        }
    }
}