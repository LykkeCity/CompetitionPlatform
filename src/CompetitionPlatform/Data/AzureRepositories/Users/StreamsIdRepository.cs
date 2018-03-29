using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class StreamsIdEntity : TableEntity, IStreamsIdData
    {
        public static string GeneratePartitionKey()
        {
            return "StreamsId";
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }

        public static StreamsIdEntity Create(IStreamsIdData src)
        {
            var streamsId = Guid.NewGuid().ToString("D");

            var result = new StreamsIdEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.ClientId),
                ClientId = src.ClientId,
                StreamsId = streamsId
            };

            return result;
        }

        public string ClientId { get; set; }
        public string StreamsId { get; set; }
    }

    public class StreamsIdRepository : IStreamsIdRepository
    {
        private readonly INoSQLTableStorage<StreamsIdEntity> _streamsIdTableStorage;

        public StreamsIdRepository(INoSQLTableStorage<StreamsIdEntity> streamsIdTableStorage)
        {
            _streamsIdTableStorage = streamsIdTableStorage;
        }

        public async Task SaveAsync(IStreamsIdData streamsIdData)
        {
            var newEntity = StreamsIdEntity.Create(streamsIdData);
            await _streamsIdTableStorage.InsertAsync(newEntity);
        }

        public async Task<IStreamsIdData> GetOrCreateAsync(string clientId)
        {
            var partitionKey = StreamsIdEntity.GeneratePartitionKey();
            var rowKey = StreamsIdEntity.GenerateRowKey(clientId);

            var data = await _streamsIdTableStorage.GetDataAsync(partitionKey, rowKey);

            if (data != null)
                return data;

            var newEntity = StreamsIdEntity.Create(new StreamsIdEntity { ClientId = clientId });
            await _streamsIdTableStorage.InsertAsync(newEntity);
            return newEntity;
        }

        public async Task<IEnumerable<IStreamsIdData>> GetStreamsIdsAsync()
        {
            var partitionKey = StreamsIdEntity.GeneratePartitionKey();
            return await _streamsIdTableStorage.GetDataAsync(partitionKey);
        }
    }
}
