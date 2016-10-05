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

        public static string GenerateRowKey(string userId)
        {
            return userId;
        }

        public static MailSentEntity Create(string partition, string userId, string projectId = "")
        {
            var result = new MailSentEntity
            {
                PartitionKey = GeneratePartitionKey(partition),
                RowKey = GenerateRowKey(userId),
                UserId = userId
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

        public async Task SaveRegisterAsync(string userId)
        {
            var newEntity = MailSentEntity.Create("Register", userId);
            await _mailSentStorage.InsertAsync(newEntity);
        }

        public async Task SaveFollowAsync(string userId, string projectId)
        {
            var newEntity = MailSentEntity.Create("Follow", userId, projectId);
            await _mailSentStorage.InsertAsync(newEntity);
        }

        public async Task<IMailSentData> GetRegisterAsync(string userId)
        {
            var partitionKey = MailSentEntity.GeneratePartitionKey("Register");
            var rowKey = MailSentEntity.GenerateRowKey(userId);

            return await _mailSentStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IMailSentData>> GetFollowAsync()
        {
            var partitionKey = MailSentEntity.GeneratePartitionKey("Follow");

            return await _mailSentStorage.GetDataAsync(partitionKey);
        }
    }
}
