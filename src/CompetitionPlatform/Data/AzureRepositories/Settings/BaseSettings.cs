namespace CompetitionPlatform.Data.AzureRepositories.Settings
{
    public class BaseSettings
    {
        public AzureSettings Azure { get; set; }
        public AuthenticationSettings Authentication { get; set; }
        public NotificationsSettings Notifications { get; set; }
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
}
