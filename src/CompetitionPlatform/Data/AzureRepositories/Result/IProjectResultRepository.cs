using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Result
{
    public interface IProjectResultData
    {
        string ProjectId { get; set; }
        string ParticipantId { get; set; }
        string ParticipantFullName { get; set; }
        string Link { get; set; }
        DateTime Submitted { get; set; }
        int Score { get; set; }
        int Votes { get; set; }
    }

    public interface IProjectResultRepository
    {
        Task<IProjectResultData> GetAsync(string projectId, string participantId);
        Task<IEnumerable<IProjectResultData>> GetResultsAsync(string projectId);
        Task SaveAsync(IProjectResultData resultData);
        Task UpdateAsync(IProjectResultData resultData);
    }
}
