using Microsoft.AspNetCore.SignalR;
using Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.Models;

namespace Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.Hubs
{
    public class ChatHub : Hub
    {
        public int PublicId { get; set; }

        private int PrivateId { get; set; }

        public async Task SendMessageAsync(string from, string message) 
        {
            var data = new ChatHubMessageReceivedModel() 
            {
                From= from,
                Message = message
            };

            await this.Clients.All.SendAsync("messageReceived", data);
        }

        public async Task LoginUser(string nickname, int age) 
        {
            await this.Clients.All.SendAsync("userLogined", nickname, age);
        }

        private Task DoPrivateThing() 
        {
            return Task.CompletedTask;
        }
    }
}
