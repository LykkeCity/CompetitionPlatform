using System.Collections.Generic;
using Lykke.EmailSenderProducer.Interfaces;

namespace CompetitionPlatform.Data.AzureRepositories.Settings
{
    public class BaseSettings
    {
        public SlackNotificationSettings SlackNotifications { get; set; }
        public StreamsSettings LykkeStreams { get; set; }
        public EmailServiceBus EmailServiceBus { get; set; }
    }

    public class AuthenticationSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string PostLogoutRedirectUri { get; set; }
        public string Authority { get; set; }
    }

    public class AzureSettings
    {
        public string StorageConnString { get; set; }
        public string StorageLogConnString { get; set; }
    }

    public class EmailServiceBus : IServiceBusEmailSettings
    {
        public string Key { get; set; }
        public string QueueName { get; set; }
        public string NamespaceUrl { get; set; }
        public string PolicyName { get; set; }
    }

    public class SlackNotificationSettings
    {
        public SlackNotificationsAzureSettings AzureQueue { get; set; }
        public int ThrottlingLimitSeconds { get; set; }
    }

    public class SlackNotificationsAzureSettings
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }

    public class StreamsSettings
    {
        public AzureSettings Azure { get; set; }
        public AuthenticationSettings Authentication { get; set; }
        public List<string> ProjectCreateNotificationReceiver { get; set; }
    }
}
