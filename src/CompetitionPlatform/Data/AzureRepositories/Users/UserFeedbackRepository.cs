using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class UserFeedbackEntity : TableEntity, IUserFeedbackData
    {
        public static string GeneratePartitionKey()
        {
            return "Feedback";
        }

        public static string GenerateRowKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public string Email { get; set; }
        public string Name { get; set; }
        public string Feedback { get; set; }
        public DateTime Created { get; set; }

        public static UserFeedbackEntity Create(IUserFeedbackData src)
        {
            var result = new UserFeedbackEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                Email = src.Email,
                Name = src.Name,
                Feedback = src.Feedback,
                Created = src.Created
            };

            return result;
        }
    }

    public class UserFeedbackRepository : IUserFeedbackRepository
    {
        private readonly IAzureTableStorage<UserFeedbackEntity> _userFeedbackTableStorage;

        public UserFeedbackRepository(IAzureTableStorage<UserFeedbackEntity> userFeedbackTableStorage)
        {
            _userFeedbackTableStorage = userFeedbackTableStorage;
        }

        public async Task SaveAsync(IUserFeedbackData feedbackData)
        {
            var newEntity = UserFeedbackEntity.Create(feedbackData);
            await _userFeedbackTableStorage.InsertAsync(newEntity);
        }

        public async Task<IEnumerable<IUserFeedbackData>> GetFeedbacksAsync()
        {
            var partitionKey = UserFeedbackEntity.GeneratePartitionKey();

            return await _userFeedbackTableStorage.GetDataAsync(partitionKey);
        }
    }
}
