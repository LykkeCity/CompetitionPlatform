using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IMailSentData
    {
        string UserId { get; set; }
        string ProjectId { get; set; }
    }

    public interface IMailSentRepository
    {
        Task SaveRegisterAsync(string userId);
        Task SaveFollowAsync(string userId, string projectId);
        Task<IMailSentData> GetRegisterAsync(string userId);
        Task<IEnumerable<IMailSentData>> GetFollowAsync();
    }
}
