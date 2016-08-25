using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IProjectFollowData
    {
        string UserId { get; set; }
        string ProjectId { get; set; }
    }

    public interface IProjectFollowRepository
    {
        Task SaveAsync(string userId, string projectId);
        Task<IProjectFollowData> GetAsync(string userId, string projectId);
        Task<IEnumerable<IProjectFollowData>> GetProjectsFollowAsync(string userId);
        Task<IProjectFollowData> DeleteAsync(string userId, string projectId);
    }
}
