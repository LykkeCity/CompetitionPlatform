using System.Collections.Generic;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;
using Lykke.SettingsReader.Attributes;

namespace CompetitionPlatform.Data.AzureRepositories.Settings
{
    public class BaseSettings
    {
        public SlackNotificationSettings SlackNotifications { get; set; }
        public StreamsSettings LykkeStreams { get; set; }
        public KycServiceClientSettings KycServiceClient { get; set; } 
    }

    public class AuthenticationSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string PostLogoutRedirectUri { get; set; }
        [HttpCheck("/api/isalive")]
        public string Authority { get; set; }
    }

    public class AzureSettings
    {
        [AzureBlobCheck]
        public string StorageConnString { get; set; }
        [AzureTableCheck]
        public string StorageLogConnString { get; set; }
        [AzureQueueCheck]
        public string MessageQueueConnString { get; set; }
    }
    
    public class SlackNotificationSettings
    {
        public SlackNotificationsAzureSettings AzureQueue { get; set; }
        public int ThrottlingLimitSeconds { get; set; }
    }

    public class SlackNotificationsAzureSettings
    {
        [AzureQueueCheck]
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }

    public class StreamsSettings
    {
        public AzureSettings Azure { get; set; }
        public AuthenticationSettings Authentication { get; set; }
        public List<string> ProjectCreateNotificationReceiver { get; set; }
        public PersonalDataServiceSettings PersonalDataService { get; set; }
    }

    public class PersonalDataServiceSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUri { get; set; }
        [HttpCheck("/api/isalive")]
        public string ServiceExternalUri { get; set; }
        public string ApiKey { get; set; }
    }
}
