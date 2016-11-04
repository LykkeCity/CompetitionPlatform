using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IFollowMailSentData
    {
        string UserId { get; set; }
        string ProjectId { get; set; }
    }

    public interface IFollowMailSentRepository
    {
        Task SaveFollowAsync(string userId, string projectId);
        Task<IEnumerable<IFollowMailSentData>> GetFollowAsync();
    }
}
