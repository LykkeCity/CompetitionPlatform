using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Vote
{
    public class ProjectResultVoteEntity : TableEntity, IProjectResultVoteData
    {
        public static string GeneratePartitionKey(string projectId)
        {
            return projectId;
        }

        public static string GenerateRowKey(string participantId, string voterId)
        {
            return participantId + voterId;
        }

        public string ProjectId { get; set; }
        public string VoterUserId { get; set; }
        public string ParticipantId { get; set; }
        public string UserAgent { get; set; }
        public string Type { get; set; }

        public static ProjectResultVoteEntity Create(IProjectResultVoteData src)
        {
            var result = new ProjectResultVoteEntity
            {
                RowKey = GenerateRowKey(src.ParticipantId, src.VoterUserId),
                VoterUserId = src.VoterUserId,
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                ParticipantId = src.ParticipantId,
                UserAgent = src.UserAgent,
                Type = src.Type
            };

            return result;
        }
    }

    public class ProjectResultVoteRepository : IProjectResultVoteRepository
    {
        private readonly IAzureTableStorage<ProjectResultVoteEntity> _projectResultVoteTableStorage;

        public ProjectResultVoteRepository(IAzureTableStorage<ProjectResultVoteEntity> projectResultVoteTableStorage)
        {
            _projectResultVoteTableStorage = projectResultVoteTableStorage;
        }

        public async Task SaveAsync(IProjectResultVoteData projectResultVoteData)
        {
            var newEntity = ProjectResultVoteEntity.Create(projectResultVoteData);
            await _projectResultVoteTableStorage.InsertAsync(newEntity);
        }

        public async Task<IEnumerable<IProjectResultVoteData>> GetProjectResultVotesAsync(string projectId)
        {
            var partitionKey = ProjectResultVoteEntity.GeneratePartitionKey(projectId);
            return await _projectResultVoteTableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IProjectResultVoteData> GetAsync(string projectId, string participantId, string voterId)
        {
            var partitionKey = ProjectResultVoteEntity.GeneratePartitionKey(projectId);
            var rowKey = ProjectResultVoteEntity.GenerateRowKey(participantId, voterId);

            return await _projectResultVoteTableStorage.GetDataAsync(partitionKey, rowKey);
        }
    }
}
