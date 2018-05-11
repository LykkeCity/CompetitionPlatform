using System.Net.Http;
using System.Text;
using AzureStorage.Blob;
using Common;
using Newtonsoft.Json;

namespace CompetitionPlatform.Data.AzureRepositories.Settings
{
    public static class GeneralSettingsReader
    {
        //public static T ReadGeneralSettings<T>(string connectionString, string container, string fileName)
        //{
        //    //var settingsStorage = new AzureBlobStorage(connectionString);
        //    var settingsData = settingsStorage.GetAsync(container, fileName).Result.ToBytes();
        //    var str = Encoding.UTF8.GetString(settingsData);

        //    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
        //}

        public static T ReadGeneralSettingsFromUrl<T>(string settingsUrl)
        {
            var httpClient = new HttpClient();
            var settingsString = httpClient.GetStringAsync(settingsUrl).Result;
            var serviceBusSettings = JsonConvert.DeserializeObject<T>(settingsString);
            return serviceBusSettings;
        }
    }
}
