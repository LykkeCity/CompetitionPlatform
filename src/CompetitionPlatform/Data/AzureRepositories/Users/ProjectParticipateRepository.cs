using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Tables;
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
                RowKey = GenerateRowKey(src.UserId)
            };

            return result;
        }

        public string UserId { get; set; }
        public string ProjectId { get; set; }
    }
    public class ProjectParticipateRepository : IProjectParticipateRepository
    {
        private readonly IAzureTableStorage<ProjectParticipateEntity> _projectParticipateTableStorage;

        public ProjectParticipateRepository(IAzureTableStorage<ProjectParticipateEntity> projectParticipateTableStorage)
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

        public async Task<IEnumerable<IProjectParticipateData>> GetProjectParticipants(string projectId)
        {
            var partitionKey = ProjectParticipateEntity.GeneratePartitionKey(projectId);

            return await _projectParticipateTableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IProjectParticipateData> DeleteAsync(string userId, string projectId)
        {
            var partitionKey = ProjectParticipateEntity.GeneratePartitionKey(projectId);
            var rowKey = ProjectParticipateEntity.GenerateRowKey(userId);

            return await _projectParticipateTableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
