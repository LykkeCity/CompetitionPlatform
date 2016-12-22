using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Vote
{
    public interface IProjectVoteData
    {
        string ProjectId { get; set; }
        string VoterUserId { get; set; }
        int ForAgainst { get; set; }
        string UserAgent { get; set; }
    }

    public interface IProjectVoteRepository
    {
        Task SaveAsync(IProjectVoteData projectVoteData);
        Task<IProjectVoteData> GetAsync(string projectId, string user);
        Task<IEnumerable<IProjectVoteData>> GetProjectVotesAsync(string projectId);
        Task UpdateAsync(IProjectVoteData projectVoteData);
    }
}
