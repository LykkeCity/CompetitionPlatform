using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class RegisterMailSentEntity : TableEntity, IRegisterMailSentData
    {
        public static string GeneratePartitionKey()
        {
            return "Register";
        }

        public static string GenerateRowKey(string userId)
        {
            return userId;
        }

        public static RegisterMailSentEntity Create(string userId)
        {
            var result = new RegisterMailSentEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(userId),
                UserId = userId
            };

            return result;
        }

        public string UserId { get; set; }
    }

    public class RegisterMailSentRepository : IRegisterMailSentRepository
    {
        private readonly IAzureTableStorage<RegisterMailSentEntity> _mailSentStorage;

        public RegisterMailSentRepository(IAzureTableStorage<RegisterMailSentEntity> mailSentStorage)
        {
            _mailSentStorage = mailSentStorage;
        }

        public async Task SaveRegisterAsync(string userId)
        {
            var newEntity = RegisterMailSentEntity.Create(userId);
            await _mailSentStorage.InsertAsync(newEntity);
        }

        public async Task<IRegisterMailSentData> GetRegisterAsync(string userId)
        {
            var partitionKey = RegisterMailSentEntity.GeneratePartitionKey();

            return await _mailSentStorage.GetDataAsync(partitionKey, userId);
        }
    }
}
