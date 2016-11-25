using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class UserRoleEntity : TableEntity, IUSerRoleData
    {
        public static string GeneratePartitionKey()
        {
            return "Role";
        }

        public static string GenerateRowKey(string userId)
        {
            return userId.ToLower();
        }

        public string Role { get; set; }
    }

    public class UserRolesRepository : IUserRolesRepository
    {
        private readonly IAzureTableStorage<UserRoleEntity> _userRoleTableStorage;

        public UserRolesRepository(IAzureTableStorage<UserRoleEntity> userRoleTableStorage)
        {
            _userRoleTableStorage = userRoleTableStorage;
        }

        public async Task<IUSerRoleData> GetAsync(string userId)
        {
            var partitionKey = UserRoleEntity.GeneratePartitionKey();
            var rowKey = UserRoleEntity.GenerateRowKey(userId);

            return await _userRoleTableStorage.GetDataAsync(partitionKey, rowKey);
        }
    }
}
