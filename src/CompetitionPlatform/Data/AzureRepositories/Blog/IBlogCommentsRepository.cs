using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Blog
{
    public interface IBlogCommentData
    {
        string Id { get; set; }
        string BlogId { get; set; }
        string UserId { get; set; }
        string FullName { get; set; }
        string Comment { get; set; }
        string ParentId { get; set; }
        DateTime Created { get; set; }
        DateTime LastModified { get; set; }
        string UserAgent { get; set; }
        bool Deleted { get; set; }
    }
    
    public interface IBlogCommentsRepository
    {
        Task<IEnumerable<IBlogCommentData>> GetBlogCommentsAsync(string blogId);
        Task<int> GetBlogCommentsCountAsync(string blogId);
        Task<IBlogCommentData> GetBlogCommentAsync(string blogId, string commentId);
        Task SaveAsync(IBlogCommentData blogCommentData);
        Task UpdateAsync(IBlogCommentData blogCommentData, string blogId);
        Task DeleteAsync(string blogId, string commentId);
    }
}
