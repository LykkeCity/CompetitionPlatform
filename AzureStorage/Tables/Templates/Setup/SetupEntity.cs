using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage.Tables.Templates.Setup
{
    public class SetupEntity : TableEntity
    {
        public string Value { get; set; }

        public static SetupEntity Create(string partitionKey, string rowKey, string value)
        {
            return new SetupEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Value = value
            };
        }
    }

}
