using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IProjectParticipateData
    {
        string UserId { get; set; }
        string ProjectId { get; set; }
    }

    public interface IProjectParticipateRepository
    {
        Task SaveAsync(IProjectParticipateData projectParticipateData);
        Task<IProjectParticipateData> GetAsync(string projectId, string userId);
        Task<IEnumerable<IProjectParticipateData>> GetProjectParticipants(string projectId);
        Task<IProjectParticipateData> DeleteAsync(string userId, string projectId);
    }
}
