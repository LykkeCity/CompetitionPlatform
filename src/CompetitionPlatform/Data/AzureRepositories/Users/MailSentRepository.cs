using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class MailSentEntity : TableEntity, IMailSentData
    {
        public static string GeneratePartitionKey(string partition)
        {
            return partition;
        }

        public static string GenerateRowKey(string userId, string projectId)
        {
            return userId + projectId;
        }

        public static MailSentEntity Create(string partition, string userId, string projectId = "")
        {
            var result = new MailSentEntity
            {
                PartitionKey = GeneratePartitionKey(partition),
                RowKey = GenerateRowKey(userId, projectId),
                UserId = userId,
                ProjectId = projectId
            };

            return result;
        }

        public string UserId { get; set; }
        public string ProjectId { get; set; }
    }

    public class MailSentRepository : IMailSentRepository
    {
        private readonly IAzureTableStorage<MailSentEntity> _mailSentStorage;

        public MailSentRepository(IAzureTableStorage<MailSentEntity> mailSentStorage)
        {
            _mailSentStorage = mailSentStorage;
        }

        public async Task SaveRegisterAsync(string userId, string projectId)
        {
            var newEntity = MailSentEntity.Create("Register", userId, projectId);
            await _mailSentStorage.InsertAsync(newEntity);
        }

        public async Task SaveFollowAsync(string userId, string projectId)
        {
            var newEntity = MailSentEntity.Create("Follow", userId, projectId);
            await _mailSentStorage.InsertAsync(newEntity);
        }

        public async Task<IEnumerable<IMailSentData>> GetRegisterAsync(string userId)
        {
            var partitionKey = MailSentEntity.GeneratePartitionKey("Register");

            return await _mailSentStorage.GetDataAsync(partitionKey);
        }

        public async Task<IEnumerable<IMailSentData>> GetFollowAsync()
        {
            var partitionKey = MailSentEntity.GeneratePartitionKey("Follow");

            return await _mailSentStorage.GetDataAsync(partitionKey);
        }
    }
}
