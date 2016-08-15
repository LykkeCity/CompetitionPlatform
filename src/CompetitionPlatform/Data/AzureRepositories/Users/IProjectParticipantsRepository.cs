using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IProjectParticipateData
    {
        string ProjectId { get; set; }
        string UserId { get; set; }
        string FullName { get; set; }
        DateTime Registered { get; set; }
        bool Result { get; set; }
    }

    public interface IProjectParticipantsRepository
    {
        Task SaveAsync(IProjectParticipateData projectParticipateData);
        Task<IProjectParticipateData> GetAsync(string projectId, string userId);
        Task<IEnumerable<IProjectParticipateData>> GetProjectParticipantsAsync(string projectId);
        Task<IProjectParticipateData> DeleteAsync(string projectId, string userId);
        Task<int> GetProjectParticipantsCountAsync(string projectId);
        Task UpdateAsync(IProjectParticipateData projectParticipantData);
    }
}
