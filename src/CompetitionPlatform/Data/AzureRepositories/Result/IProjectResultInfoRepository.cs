using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Result
{
    public interface IProjectResultInfoData
    {
        string Id { get; }
        string ProjectId { get; set; }
        string User { get; set; }
        DateTime Submitted { get; set; }
    }

    public interface IProjectResultInfoRepository
    {
        Task<IEnumerable<IProjectResultInfoData>> GetResultsAsync(string resultId);
        Task SaveAsync(IProjectResultInfoData resultInfoData);
    }
}
