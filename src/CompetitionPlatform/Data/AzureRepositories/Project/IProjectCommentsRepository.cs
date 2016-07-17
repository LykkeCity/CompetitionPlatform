using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public interface ICommentData
    {
        string ProjectId { get; set; }
        string User { get; }
        string Comment { get; set; }
    }

    public interface IProjectCommentsRepository
    {
        Task<ICommentData> GetAsync(string projectId, string username);
        Task<IEnumerable<ICommentData>> GetProjectCommentsAsync(string projectId);
        Task SaveAsync(ICommentData projectCommentData);
        Task UpdateAsync(ICommentData projectData);
    }
}
