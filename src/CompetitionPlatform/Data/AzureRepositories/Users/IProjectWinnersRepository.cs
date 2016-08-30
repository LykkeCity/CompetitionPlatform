using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IWinnerData
    {
        string ProjectId { get; set; }
        string WinnerId { get; set; }
        string FullName { get; set; }
        int Place { get; set; }
        string Result { get; set; }
        int Votes { get; set; }
        int Score { get; set; }
    }

    public interface IProjectWinnersRepository
    {
        Task<IWinnerData> GetAsync(string projectId, string userId);
        Task<IEnumerable<IWinnerData>> GetWinnersAsync(string projectId);
        Task SaveAsync(IWinnerData winnerData);
    }
}
