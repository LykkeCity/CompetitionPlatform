using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Vote
{
    public interface IProjectResultVoteData
    {
        string ProjectId { get; set; }
        string VoterUserId { get; set; }
        string ParticipantUserId { get; set; }
    }

    public interface IProjectResultVoteRepository
    {
        Task SaveAsync(IProjectResultVoteData projectResultVoteData);
        Task<IEnumerable<IProjectResultVoteData>> GetProjectResultVotesAsync(string projectId);
    }
}
