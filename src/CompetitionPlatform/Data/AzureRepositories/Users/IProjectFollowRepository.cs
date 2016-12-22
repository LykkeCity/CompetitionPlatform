using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IProjectFollowData
    {
        string Id { get; set; }
        string FullName { get; set; }
        string UserId { get; set; }
        string ProjectId { get; set; }
        string UserAgent { get; set; }
    }

    public interface IProjectFollowRepository
    {
        Task SaveAsync(IProjectFollowData projectFollowData);
        Task<IProjectFollowData> GetAsync(string userId, string projectId);
        Task<IEnumerable<IProjectFollowData>> GetFollowAsync();
        Task<IProjectFollowData> DeleteAsync(string userId, string projectId);
    }
}
