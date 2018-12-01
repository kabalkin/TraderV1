using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;


namespace TraderV1.Models
{
    public class MessagesHub : Hub
    {
        public async Task Send(string message)
        {
            await this.Clients.All.SendAsync("Send", message);
        }
        
        public async Task SendData(string message)
        {
            await this.Clients.All.SendAsync("SendData", message);
        }

        public async Task SendToConsole(string message)
        {
            await this.Clients.All.SendAsync("SendToConsole", message);
        }
    }
}