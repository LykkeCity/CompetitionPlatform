using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class PublicFeedbackEntity : TableEntity, IPublicFeedbackData
    {
        public static string GeneratePartitionKey()
        {
            return "PublicFeedback";
        }

        public static string GenerateRowKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public string User { get; set; }
        public string Title { get; set; }
        public string Feedback { get; set; }

        public static PublicFeedbackEntity Create(IPublicFeedbackData src)
        {
            var result = new PublicFeedbackEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = src.RowKey ?? GenerateRowKey(),
                Title = src.Title,
                User = src.User,
                Feedback = src.Feedback
            };

            return result;
        }
    }

    public class PublicFeedbackRepository : IPublicFeedbackRepository
    {
        private readonly INoSQLTableStorage<PublicFeedbackEntity> _publicFeedbackTableStorage;

        public PublicFeedbackRepository(INoSQLTableStorage<PublicFeedbackEntity> publicFeedbackTableStorage)
        {
            _publicFeedbackTableStorage = publicFeedbackTableStorage;
        }

        public async Task SaveAsync(IPublicFeedbackData feedbackData)
        {
            var newEntity = PublicFeedbackEntity.Create(feedbackData);
            await _publicFeedbackTableStorage.InsertOrMergeAsync(newEntity);
        }

        public async Task<IPublicFeedbackData> GetFeedbackAsync(string rowkey)
        {
            var partitionKey = PublicFeedbackEntity.GeneratePartitionKey();

            return await _publicFeedbackTableStorage.GetDataAsync(partitionKey, rowkey);
        }

        public async Task<IEnumerable<IPublicFeedbackData>> GetFeedbacksAsync()
        {
            var partitionKey = PublicFeedbackEntity.GeneratePartitionKey();

            return await _publicFeedbackTableStorage.GetDataAsync(partitionKey);
        }

        public async Task DeleteFeedbacksAsync(string rowkey)
        {
            var partitionKey = PublicFeedbackEntity.GeneratePartitionKey();

            await _publicFeedbackTableStorage.DeleteAsync(partitionKey, rowkey);
        }
    }
}
