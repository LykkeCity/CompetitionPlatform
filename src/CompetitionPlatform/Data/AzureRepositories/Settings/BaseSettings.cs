using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Settings
{
    public class BaseSettings
    {
        public AzureSettings Azure { get; set; }
        public AuthenticationSettings Authentication { get; set; }
        public NotificationsSettings Notifications { get; set; }
        public RecaptchaSettings Recaptcha { get; set; }
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

    public class NotificationsSettings
    {
        public string EmailsQueueConnString { get; set; }
        public string SlackQueueConnString { get; set; }
    }

    public class RecaptchaSettings
    {
        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
    }
}
