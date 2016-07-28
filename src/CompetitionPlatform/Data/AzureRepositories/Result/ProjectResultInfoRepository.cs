using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Result
{
    public class ProjectResultInfoEntity : TableEntity, IProjectResultInfoData
    {
        public static string GeneratePartitionKey(string projectId)
        {
            return projectId;
        }

        public static string GenerateRowKey()
        {
            return Guid.NewGuid().ToString();
        }

        public string Id => RowKey;
        public string ProjectId { get; set; }
        public string User { get; set; }
        public DateTime Submitted { get; set; }

        public static ProjectResultInfoEntity Create(IProjectResultInfoData src)
        {
            var result = new ProjectResultInfoEntity
            {
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                RowKey = GenerateRowKey(),
                User = src.User,
                Submitted = src.Submitted
            };

            return result;
        }
    }

    public class ProjectResultInfoRepository : IProjectResultInfoRepository
    {
        private readonly IAzureTableStorage<ProjectResultInfoEntity> _projectResultInfoTableStorage;

        public ProjectResultInfoRepository(IAzureTableStorage<ProjectResultInfoEntity> projectResultInfoTableStorage)
        {
            _projectResultInfoTableStorage = projectResultInfoTableStorage;
        }

        public async Task<IEnumerable<IProjectResultInfoData>> GetResultsAsync(string projectId)
        {
            var partitionKey = ProjectResultInfoEntity.GeneratePartitionKey(projectId);
            return await _projectResultInfoTableStorage.GetDataAsync(partitionKey);
        }

        public async Task SaveAsync(IProjectResultInfoData resultInfoData)
        {
            var newEntity = ProjectResultInfoEntity.Create(resultInfoData);
            await _projectResultInfoTableStorage.InsertAsync(newEntity);
        }
    }
}
