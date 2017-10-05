using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class ProjectParticipateEntity : TableEntity, IProjectParticipateData
    {
        public static string GeneratePartitionKey(string projectId)
        {
            return projectId;
        }

        public static string GenerateRowKey(string userId)
        {
            return userId;
        }

        public static ProjectParticipateEntity Create(IProjectParticipateData src)
        {
            var result = new ProjectParticipateEntity
            {
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                RowKey = GenerateRowKey(src.UserId),
                ProjectId = src.ProjectId,
                UserId = src.UserId,
                UserIdentifier = src.UserIdentifier,
                FullName = src.FullName,
                Registered = src.Registered,
                Result = src.Result,
                UserAgent = src.UserAgent
            };

            return result;
        }

        internal void Update(IProjectParticipateData src)
        {
            Result = src.Result;
            UserIdentifier = src.UserIdentifier;
        }

        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string UserIdentifier { get; set; }
        public string FullName { get; set; }
        public DateTime Registered { get; set; }
        public bool Result { get; set; }
        public string UserAgent { get; set; }
    }
    public class ProjectParticipantsRepository : IProjectParticipantsRepository
    {
        private readonly INoSQLTableStorage<ProjectParticipateEntity> _projectParticipateTableStorage;

        public ProjectParticipantsRepository(INoSQLTableStorage<ProjectParticipateEntity> projectParticipateTableStorage)
        {
            _projectParticipateTableStorage = projectParticipateTableStorage;
        }

        public async Task SaveAsync(IProjectParticipateData projectParticipateData)
        {
            var newEntity = ProjectParticipateEntity.Create(projectParticipateData);
            await _projectParticipateTableStorage.InsertAsync(newEntity);
        }

        public async Task<IProjectParticipateData> GetAsync(string projectId, string userId)
        {
            var partitionKey = ProjectParticipateEntity.GeneratePartitionKey(projectId);
            var rowKey = ProjectParticipateEntity.GenerateRowKey(userId);

            return await _projectParticipateTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IProjectParticipateData>> GetProjectParticipantsAsync(string projectId)
        {
            var partitionKey = ProjectParticipateEntity.GeneratePartitionKey(projectId);

            return await _projectParticipateTableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IProjectParticipateData> DeleteAsync(string projectId, string userId)
        {
            var partitionKey = ProjectParticipateEntity.GeneratePartitionKey(projectId);
            var rowKey = ProjectParticipateEntity.GenerateRowKey(userId);

            return await _projectParticipateTableStorage.DeleteAsync(partitionKey, rowKey);
        }

        public async Task<int> GetProjectParticipantsCountAsync(string projectId)
        {
            var participants = await GetProjectParticipantsAsync(projectId);
            return participants.ToList().Count;
        }

        public Task UpdateAsync(IProjectParticipateData projectParticipantData)
        {
            var partitionKey = ProjectParticipateEntity.GeneratePartitionKey(projectParticipantData.ProjectId);
            var rowKey = ProjectParticipateEntity.GenerateRowKey(projectParticipantData.UserId);

            return _projectParticipateTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(projectParticipantData);
                return itm;
            });
        }
    }
}
