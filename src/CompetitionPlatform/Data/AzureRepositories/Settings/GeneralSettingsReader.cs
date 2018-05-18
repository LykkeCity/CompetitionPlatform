using System.Net.Http;
using System.Text;
using AzureStorage.Blob;
using Common;
using Newtonsoft.Json;

namespace CompetitionPlatform.Data.AzureRepositories.Settings
{
    public static class GeneralSettingsReader
    {
        public static T ReadGeneralSettingsFromUrl<T>(string settingsUrl)
        {
            var httpClient = new HttpClient();
            var settingsString = httpClient.GetStringAsync(settingsUrl).Result;
            var serviceBusSettings = JsonConvert.DeserializeObject<T>(settingsString);
            return serviceBusSettings;
        }
    }
}
