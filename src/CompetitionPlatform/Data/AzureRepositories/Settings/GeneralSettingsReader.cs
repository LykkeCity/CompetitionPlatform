using System.Text;
using AzureStorage.Blobs;
using Common;

namespace CompetitionPlatform.Data.AzureRepositories.Settings
{
    public static class GeneralSettingsReader
    {
        public static T ReadGeneralSettings<T>(string connectionString, string container, string fileName)
        {
            var settingsStorage = new AzureBlobStorage(connectionString);
            var settingsData = settingsStorage.GetAsync(container, fileName).Result.ToBytes();
            var str = Encoding.UTF8.GetString(settingsData);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
        }
    }
}
