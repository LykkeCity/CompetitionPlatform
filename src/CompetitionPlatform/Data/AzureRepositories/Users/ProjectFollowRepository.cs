using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public class ProjectFollowEntity : TableEntity, IProjectFollowData
    {
        public static string GeneratePartitionKey()
        {
            return "ProjectFollow";
        }

        public static string GenerateRowKey(string projectId, string userId)
        {
            return projectId + userId;
        }

        public static ProjectFollowEntity Create(IProjectFollowData src)
        {
            var id = GenerateRowKey(src.ProjectId, src.UserId);

            var result = new ProjectFollowEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = id,
                Id = id,
                FullName = src.FullName,
                UserId = src.UserId,
                ProjectId = src.ProjectId,
                UserAgent = src.UserAgent
            };

            return result;
        }

        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserId { get; set; }
        public string ProjectId { get; set; }
        public string UserAgent { get; set; }
    }
    public class ProjectFollowRepository : IProjectFollowRepository
    {
        private readonly INoSQLTableStorage<ProjectFollowEntity> _projectFollowTableStorage;

        public ProjectFollowRepository(INoSQLTableStorage<ProjectFollowEntity> projectFollowTableStorage)
        {
            _projectFollowTableStorage = projectFollowTableStorage;
        }

        public async Task SaveAsync(IProjectFollowData projectFollowData)
        {
            var newEntity = ProjectFollowEntity.Create(projectFollowData);
            await _projectFollowTableStorage.InsertAsync(newEntity);
        }

        public async Task<IProjectFollowData> GetAsync(string userId, string projectId)
        {
            var partitionKey = ProjectFollowEntity.GeneratePartitionKey();
            var rowKey = ProjectFollowEntity.GenerateRowKey(projectId, userId);

            return await _projectFollowTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IProjectFollowData>> GetFollowAsync()
        {
            var partitionKey = ProjectFollowEntity.GeneratePartitionKey();

            return await _projectFollowTableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IProjectFollowData> DeleteAsync(string userId, string projectId)
        {
            var partitionKey = ProjectFollowEntity.GeneratePartitionKey();
            var rowKey = ProjectFollowEntity.GenerateRowKey(projectId, userId);

            return await _projectFollowTableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
