using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class UserEntity : TableEntity, IUser
    {
        public string Id => RowKey;
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public string Hash { get; set; }
        public string Salt { get; set; }

        public static string GeneratePartitionKey()
        {
            return "User";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        internal void Edit(IUser src)
        {
            IsAdmin = src.IsAdmin;
            FullName = src.FullName;
        }

        public static UserEntity Create(IUser src, string password)
        {
            var result = new UserEntity()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.Id)
            };

            result.Edit(src);
            result.SetPassword(password);

            return result;
        }
    }

    public static class UserExt
    {
        private static string CalcHash(string password, string salt)
        {
            var cryptoTransformSha1 = SHA1.Create();

            var passwordSaltHash = Encoding.UTF8.GetBytes(password + salt);

            var sha1 = cryptoTransformSha1.ComputeHash(passwordSaltHash);

            return Convert.ToBase64String(sha1);
        }

        public static void SetPassword(this UserEntity entity, string password)
        {
            entity.Salt = Guid.NewGuid().ToString();
            entity.Hash = CalcHash(password, entity.Salt);
        }

        public static bool CheckPassword(this UserEntity entity, string password)
        {
            var hash = CalcHash(password, entity.Salt);
            return entity.Hash == hash;
        }
    }

    public class UsersRepository : IUsersRepository
    {
        private readonly INoSQLTableStorage<UserEntity> _tableStorage;

        public UsersRepository(INoSQLTableStorage<UserEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        private Task CreateAsync(IUser user, string password)
        {
            var newUser = UserEntity.Create(user, password);
            return _tableStorage.InsertOrReplaceAsync(newUser);
        }

        private Task UpdateAsync(IUser user)
        {
            var partitionKey = UserEntity.GeneratePartitionKey();
            var rowKey = UserEntity.GenerateRowKey(user.Id);

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.Edit(user);
                return entity;
            });
        }

        public Task SaveAsync(IUser user, string password)
        {
            //if (string.IsNullOrEmpty(user.Id))
            if (!string.IsNullOrEmpty(password))
                return CreateAsync(user, password);

            return UpdateAsync(user);
        }

        public async Task<IUser> AuthenticateAsync(string username, string password)
        {
            if (username == null || password == null)
                return null;

            var partitionKey = UserEntity.GeneratePartitionKey();
            var rowKey = UserEntity.GenerateRowKey(username);

            var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            if (entity == null)
                return null;

            return entity.CheckPassword(password) ? entity : null;
        }

        public async Task<IUser> GetAsync(string id)
        {
            if (id == null)
                return null;

            var partitionKey = UserEntity.GeneratePartitionKey();
            var rowKey = UserEntity.GenerateRowKey(id);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<bool> UserExists(string id)
        {
            var partitionKey = UserEntity.GeneratePartitionKey();
            var rowKey = UserEntity.GenerateRowKey(id);

            return (await _tableStorage.GetDataAsync(partitionKey, rowKey)) != null;
        }

        public Task ChangePasswordAsync(string id, string newPassword)
        {
            var partitionKey = UserEntity.GeneratePartitionKey();
            var rowKey = UserEntity.GenerateRowKey(id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.SetPassword(newPassword);
                return itm;
            });
        }

        public async Task<IEnumerable<IUser>> GetAllAsync()
        {
            var partitionKey = UserEntity.GeneratePartitionKey();
            return await _tableStorage.GetDataAsync(partitionKey);
        }
    }
}
