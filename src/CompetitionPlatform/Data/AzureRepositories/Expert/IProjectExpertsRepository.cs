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
        string StreamsId { get; set; }
        string FullName { get; set; }
        string Description { get; set; }
        int Priority { get; set; }
    }
    public interface IProjectExpertsRepository
    {
        Task<List<IProjectExpertData>> GetAllUniqueAsync();
        Task<IProjectExpertData> GetAsync(string userId);
        Task<IProjectExpertData> GetAsync(string projectId, string participantId);
        Task<IEnumerable<IProjectExpertData>> GetProjectExpertsAsync(string projectId);
        Task SaveAsync(IProjectExpertData projectExpertData);
        Task UpdateAsync(IProjectExpertData projectExpertData);
        Task DeleteAsync(string userId, string projectId);
    }
}
