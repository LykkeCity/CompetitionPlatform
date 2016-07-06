using System.Globalization;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace AzureStorage.Tables.Templates.Setup{
    public static class SetupEntityExt{
        
            public static async Task<string> GenerateIdAsync(this IAzureTableStorage<SetupEntity> tableStorage,
           string partitionKey, string rowKey, int fromId)
        {

            while (true)
            {
                try
                {
                    var result = await tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
                    {
                        int i;

                        try
                        {
                            i = int.Parse(itm.Value);
                        }
                        catch (System.Exception)
                        {
                            i = fromId;
                        }

                        itm.Value = (i + 1).ToString(CultureInfo.InvariantCulture);
                        return itm;
                    });

                    if (result != null)
                        return result.Value;


                    var idEntity = SetupEntity.Create(partitionKey, rowKey,
                        fromId.ToString(CultureInfo.InvariantCulture));

                    await tableStorage.InsertAsync(idEntity);
                }

                catch (StorageException e)
                {
                    if (e.RequestInformation.HttpStatusCode != TableStorageUtils.Conflict)
                        throw;
                }
            }
        }

        
    }
    
}
