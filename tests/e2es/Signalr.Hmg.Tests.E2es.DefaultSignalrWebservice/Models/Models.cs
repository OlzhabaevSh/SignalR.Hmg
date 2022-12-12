namespace Signalr.Hmg.Tests.E2es.DefaultSignalrWebservice.Models
{
    public class ChatHubMessageReceivedModel
    {
        public string From { get; set; }

        public string Message { get; set; }
    }

    public class UserNotificationHubNotifiesTriggeredModel
    {
        public int RandomValue { get; set; }

        public DateTime CurrentDate { get; set; }
    }

    public class WrapperUserNotificationHubNotifiesTriggeredModel
    {
        public UserNotificationHubNotifiesTriggeredModel Data { get; set; }
    }

    public class WrapperModel<T> 
    {
        public T Data { get; set; }
    }
}
