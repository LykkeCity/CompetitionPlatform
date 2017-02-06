using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Result
{
    public class ProjectResultEntity : TableEntity, IProjectResultData
    {
        public static string GeneratePartitionKey(string projectId)
        {
            return projectId;
        }

        public static string GenerateRowKey(string participantId)
        {
            return participantId;
        }

        public string ProjectId { get; set; }
        public string ParticipantId { get; set; }
        public string ParticipantFullName { get; set; }
        public string Link { get; set; }
        public DateTime Submitted { get; set; }
        public int Score { get; set; }
        public int Votes { get; set; }
        public string UserAgent { get; set; }

        public static ProjectResultEntity Create(IProjectResultData src)
        {
            var result = new ProjectResultEntity
            {
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                RowKey = GenerateRowKey(src.ParticipantId),
                ProjectId = src.ProjectId,
                ParticipantId = src.ParticipantId,
                ParticipantFullName = src.ParticipantFullName,
                Link = src.Link,
                Submitted = src.Submitted,
                Score = src.Score,
                UserAgent = src.UserAgent
            };

            return result;
        }

        internal void Update(IProjectResultData src)
        {
            Link = src.Link;
            Score = src.Score;
            Votes = src.Votes;
            UserAgent = src.UserAgent;
        }
    }

    public class ProjectResultRepository : IProjectResultRepository
    {
        private readonly INoSQLTableStorage<ProjectResultEntity> _projectResultInfoTableStorage;

        public ProjectResultRepository(INoSQLTableStorage<ProjectResultEntity> projectResultInfoTableStorage)
        {
            _projectResultInfoTableStorage = projectResultInfoTableStorage;
        }

        public async Task<IProjectResultData> GetAsync(string projectId, string participantId)
        {
            var partitionKey = ProjectResultEntity.GeneratePartitionKey(projectId);
            var rowKey = ProjectResultEntity.GenerateRowKey(participantId);

            return await _projectResultInfoTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IProjectResultData>> GetResultsAsync(string projectId)
        {
            var partitionKey = ProjectResultEntity.GeneratePartitionKey(projectId);
            return await _projectResultInfoTableStorage.GetDataAsync(partitionKey);
        }

        public async Task<int> GetResultsCountAsync(string projectId)
        {
            var results = await GetResultsAsync(projectId);
            return results.ToList().Count;
        }

        public async Task SaveAsync(IProjectResultData resultData)
        {
            var newEntity = ProjectResultEntity.Create(resultData);
            await _projectResultInfoTableStorage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(IProjectResultData resultData)
        {
            var partitionKey = ProjectResultEntity.GeneratePartitionKey(resultData.ProjectId);
            var rowKey = ProjectResultEntity.GenerateRowKey(resultData.ParticipantId);

            return _projectResultInfoTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(resultData);
                return itm;
            });
        }
    }
}
