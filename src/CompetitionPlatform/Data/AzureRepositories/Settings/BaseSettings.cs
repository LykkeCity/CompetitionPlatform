using Lykke.EmailSenderProducer.Interfaces;

namespace CompetitionPlatform.Data.AzureRepositories.Settings
{
    public class BaseSettings
    {
        public AzureSettings Azure { get; set; }
        public AuthenticationSettings Authentication { get; set; }
        public NotificationsSettings Notifications { get; set; }

        public string EmailServiceBusSettingsUrl { get; set; }
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
    }

    public class NotificationsSettings
    {
        public string EmailsQueueConnString { get; set; }
        public string SlackQueueConnString { get; set; }
    }

    public class EmailServiceBusSettings
    {
        public EmailServiceBus EmailServiceBus;
    }

    public class EmailServiceBus : IServiceBusEmailSettings
    {
        public string Key { get; set; }
        public string QueueName { get; set; }
        public string NamespaceUrl { get; set; }
        public string PolicyName { get; set; }
    }
}
