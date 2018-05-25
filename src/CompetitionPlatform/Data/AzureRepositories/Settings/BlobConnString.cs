using Lykke.SettingsReader;
using System;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Settings
{
    public class BlobConnString : IReloadingManager<string>
    {
        public string _setting;
        public BlobConnString(string setting)
        {
            _setting = setting;
        }

        public bool HasLoaded => !string.IsNullOrEmpty(_setting);

        public string CurrentValue => _setting;

        public Task<string> Reload()
        {
            throw new System.NotImplementedException();
        }

        public bool WasReloadedFrom(DateTime dateTime)
        {
            throw new NotImplementedException();
        }
    }
}
