using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Expert
{
    public interface IProjectExpertData
    {
        string ProjectId { get; set; }
        string UserId { get; set; }
        string UserIdentifier { get; set; }
        string FullName { get; set; }
        string Description { get; set; }
    }
    public interface IProjectExpertsRepository
    {
        Task<IProjectExpertData> GetAsync(string projectId, string participantId);
        Task<IEnumerable<IProjectExpertData>> GetProjectExpertsAsync(string projectId);
        Task SaveAsync(IProjectExpertData projectExpertData);
        Task UpdateAsync(IProjectExpertData projectExpertData);
    }
}
