using Newtonsoft.Json;

namespace CompetitionPlatform.Helpers
{
    public static class NotificationMessageHelper
    {
        public static string ProjectCreatedMessage(string userEmail, string userName, string projectName)
        {
            var messageData = new MessageData
            {
                Subject = "New Project Created - " + projectName,
                Text = "User " + userName + " (" + userEmail + ") " + "Has Created a new project - " + projectName
            };

            var data = new Data
            {
                BroadcastGroup = 600,
                MessageData = messageData
            };

            var plainTextBroadCast = new PlainTextBroadcast { Data = data };

            return "PlainTextBroadcast:" + JsonConvert.SerializeObject(plainTextBroadCast);
        }
    }

    public class PlainTextBroadcast
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public int BroadcastGroup { get; set; }
        public MessageData MessageData { get; set; }
    }

    public class MessageData
    {
        public string Subject { get; set; }
        public string Text { get; set; }
    }
}
