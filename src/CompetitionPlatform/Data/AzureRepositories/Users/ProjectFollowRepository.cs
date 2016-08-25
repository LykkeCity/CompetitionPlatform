using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class ProjectFollowEntity : TableEntity, IProjectFollowData
    {
        public static string GeneratePartitionKey(string userId)
        {
            return userId;
        }

        public static string GenerateRowKey(string projectId)
        {
            return projectId;
        }

        public static ProjectFollowEntity Create(string userId, string projectId)
        {
            var result = new ProjectFollowEntity
            {
                PartitionKey = GeneratePartitionKey(userId),
                RowKey = GenerateRowKey(projectId),
                UserId = userId,
                ProjectId = projectId
            };

            return result;
        }

        public string UserId { get; set; }
        public string ProjectId { get; set; }
    }
    public class ProjectFollowRepository : IProjectFollowRepository
    {
        private readonly IAzureTableStorage<ProjectFollowEntity> _projectFollowTableStorage;

        public ProjectFollowRepository(IAzureTableStorage<ProjectFollowEntity> projectFollowTableStorage)
        {
            _projectFollowTableStorage = projectFollowTableStorage;
        }

        public async Task SaveAsync(string userId, string projectId)
        {
            var newEntity = ProjectFollowEntity.Create(userId, projectId);
            await _projectFollowTableStorage.InsertAsync(newEntity);
        }

        public async Task<IProjectFollowData> GetAsync(string userId, string projectId)
        {
            var partitionKey = ProjectFollowEntity.GeneratePartitionKey(userId);
            var rowKey = ProjectFollowEntity.GenerateRowKey(projectId);

            return await _projectFollowTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IProjectFollowData>> GetProjectsFollowAsync(string userId)
        {
            var partitionKey = ProjectFollowEntity.GeneratePartitionKey(userId);

            return await _projectFollowTableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IProjectFollowData> DeleteAsync(string userId, string projectId)
        {
            var partitionKey = ProjectFollowEntity.GeneratePartitionKey(userId);
            var rowKey = ProjectFollowEntity.GenerateRowKey(projectId);

            return await _projectFollowTableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
