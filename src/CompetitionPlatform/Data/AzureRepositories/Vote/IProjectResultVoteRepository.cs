using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Vote
{
    public interface IProjectResultVoteData
    {
        string ProjectId { get; set; }
        string VoterUserId { get; set; }
        string ParticipantId { get; set; }
        string UserAgent { get; set; }
        string Type { get; set; }
    }

    public interface IProjectResultVoteRepository
    {
        Task SaveAsync(IProjectResultVoteData projectResultVoteData);
        Task<IProjectResultVoteData> GetAsync(string projectId, string participantId, string voterId);
        Task<IEnumerable<IProjectResultVoteData>> GetProjectResultVotesAsync(string projectId);
    }
}
