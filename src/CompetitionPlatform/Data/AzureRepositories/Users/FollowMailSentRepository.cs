using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class FollowMailSentEntity : TableEntity, IFollowMailSentData
    {
        public static string GeneratePartitionKey()
        {
            return "Follow";
        }

        public static string GenerateRowKey(string userId, string projectId)
        {
            return userId + projectId;
        }

        public static FollowMailSentEntity Create(string userId, string projectId = "")
        {
            var result = new FollowMailSentEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(userId, projectId),
                UserId = userId,
                ProjectId = projectId
            };

            return result;
        }

        public string UserId { get; set; }
        public string ProjectId { get; set; }
    }

    public class FollowMailSentRepository : IFollowMailSentRepository
    {
        private readonly IAzureTableStorage<FollowMailSentEntity> _mailSentStorage;

        public FollowMailSentRepository(IAzureTableStorage<FollowMailSentEntity> mailSentStorage)
        {
            _mailSentStorage = mailSentStorage;
        }

        public async Task SaveFollowAsync(string userId, string projectId)
        {
            var newEntity = FollowMailSentEntity.Create(userId, projectId);
            await _mailSentStorage.InsertAsync(newEntity);
        }

        public async Task<IEnumerable<IFollowMailSentData>> GetFollowAsync()
        {
            var partitionKey = FollowMailSentEntity.GeneratePartitionKey();

            return await _mailSentStorage.GetDataAsync(partitionKey);
        }
    }
}
