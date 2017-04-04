using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public interface ICommentData
    {
        string Id { get; set; }
        string ProjectId { get; set; }
        string UserId { get; set; }
        string FullName { get; set; }
        string Comment { get; set; }
        string ParentId { get; set; }
        DateTime Created { get; set; }
        DateTime LastModified { get; set; }
        string UserAgent { get; set; }
        bool Deleted { get; set; }
    }

    public interface IProjectCommentsRepository
    {
        Task<IEnumerable<ICommentData>> GetProjectCommentsAsync(string projectId);
        Task<int> GetProjectCommentsCountAsync(string projectId);
        Task<ICommentData> GetCommentAsync(string projectId, string commentId);
        Task SaveAsync(ICommentData projectCommentData);
        Task UpdateAsync(ICommentData projectCommentData, string projectId);
        Task DeleteAsync(string projectId, string commentId);
    }
}
