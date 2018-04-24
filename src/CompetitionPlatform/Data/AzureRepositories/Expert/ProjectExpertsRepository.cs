using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Expert
{
    public class ProjectExpertEntity : TableEntity, IProjectExpertData
    {
        public static string GeneratePartitionKey(string projectId)
        {
            return projectId;
        }

        public static string GenerateRowKey(string userId)
        {
            return userId;
        }

        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string UserIdentifier { get; set; }
        public string StreamsId { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }

        public static ProjectExpertEntity Create(IProjectExpertData src)
        {
            var result = new ProjectExpertEntity
            {
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                RowKey = GenerateRowKey(src.UserId),
                ProjectId = src.ProjectId,
                UserId = src.UserId,
                UserIdentifier = src.UserIdentifier,
                StreamsId = src.StreamsId,
                FullName = src.FullName,
                Description = src.Description,
                Priority = src.Priority
            };

            return result;
        }

        internal void Update(IProjectExpertData src)
        {
            FullName = src.FullName;
            Description = src.Description;
            UserIdentifier = src.UserIdentifier;
            Priority = src.Priority;
        }
    }
    public class ProjectExpertsRepository : IProjectExpertsRepository
    {
        private readonly INoSQLTableStorage<ProjectExpertEntity> _projectExpertsTableStorage;

        public ProjectExpertsRepository(INoSQLTableStorage<ProjectExpertEntity> projectExpertsTableStorage)
        {
            _projectExpertsTableStorage = projectExpertsTableStorage;
        }

        public async Task<IProjectExpertData> GetAsync(string projectId, string participantId)
        {
            var partitionKey = ProjectExpertEntity.GeneratePartitionKey(projectId);
            var rowKey = ProjectExpertEntity.GenerateRowKey(participantId);

            return await _projectExpertsTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IProjectExpertData>> GetProjectExpertsAsync(string projectId)
        {
            var partitionKey = ProjectExpertEntity.GeneratePartitionKey(projectId);
            return await _projectExpertsTableStorage.GetDataAsync(partitionKey);
        }

        public async Task SaveAsync(IProjectExpertData projectExpertData)
        {
            var newEntity = ProjectExpertEntity.Create(projectExpertData);
            await _projectExpertsTableStorage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(IProjectExpertData projectExpertData)
        {
            var partitionKey = ProjectExpertEntity.GeneratePartitionKey(projectExpertData.ProjectId);
            var rowKey = ProjectExpertEntity.GenerateRowKey(projectExpertData.UserId);

            return _projectExpertsTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(projectExpertData);
                return itm;
            });
        }
    }
}
