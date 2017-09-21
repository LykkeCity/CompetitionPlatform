using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.ProjectStream
{
    public class StreamEntity : TableEntity, IStreamData
    {
        public static string GeneratePartitionKey()
        {
            return "Stream";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Stream { get; set; }
        public string AuthorId { get; set; }
        public string AuthorEmail { get; set; }

        internal void Update(IStreamData src)
        {
            Name = src.Name;
            Stream = src.Stream;
        }

        public static StreamEntity Create(IStreamData src)
        {
            var id = Guid.NewGuid().ToString("N");
            var result = new StreamEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = id,
                Id = id,
                Name = src.Name,
                Stream = src.Stream,
                AuthorId = src.AuthorId,
                AuthorEmail = src.AuthorEmail
            };

            return result;
        }
    }

    public class StreamRepository : IStreamRepository
    {
        private readonly INoSQLTableStorage<StreamEntity> _streamsTableStorage;

        public StreamRepository(INoSQLTableStorage<StreamEntity> streamsTableStorage)
        {
            _streamsTableStorage = streamsTableStorage;
        }

        public async Task<IStreamData> GetAsync(string id)
        {
            var partitionKey = StreamEntity.GeneratePartitionKey();
            var rowKey = StreamEntity.GenerateRowKey(id);

            return await _streamsTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IStreamData>> GetStreamsAsync()
        {
            var partitionKey = StreamEntity.GeneratePartitionKey();

            return await _streamsTableStorage.GetDataAsync(partitionKey);
        }

        public async Task<string> SaveAsync(IStreamData streamData)
        {
            var newEntity = StreamEntity.Create(streamData);
            await _streamsTableStorage.InsertAsync(newEntity);
            return newEntity.Id;
        }

        public Task UpdateAsync(IStreamData streamData)
        {
            var partitionKey = StreamEntity.GeneratePartitionKey();
            var rowKey = StreamEntity.GenerateRowKey(streamData.Id);

            return _streamsTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(streamData);
                return itm;
            });
        }

        public async Task DeleteAsync(string id)
        {
            var partitionKey = StreamEntity.GeneratePartitionKey();
            var rowKey = StreamEntity.GenerateRowKey(id);

            await _streamsTableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
