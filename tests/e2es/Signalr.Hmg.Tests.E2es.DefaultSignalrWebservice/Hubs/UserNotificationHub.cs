using Microsoft.AspNetCore.SignalR;

namespace Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.Hubs
{
    public class UserNotificationHub : Hub
    {
        public async Task SendTestNotification(string author) 
        {
            await this.Clients.All.SendAsync("testNotificationSent", author);
        }
    }
}
