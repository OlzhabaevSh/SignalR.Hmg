using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.Hubs;
using Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.Models;

namespace Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifiesController : ControllerBase
    {
        private readonly IHubContext<UserNotificationHub> hubContext;

        public NotifiesController(IHubContext<UserNotificationHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get() 
        {
            var data = new UserNotificationHubNotifiesTriggeredModel() 
            {
                CurrentDate = DateTime.Now,
                RandomValue = new Random().Next(1, 100)
            };

            await this.hubContext.Clients.All.SendAsync("notifiesTriggered", data);

            return Ok();
        }
    }
}
