using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Blog
{
    public class BlogCommentEntity : TableEntity, IBlogCommentData
    {
        public static string GeneratePartitionKey(string blogId)
        {
            return blogId;
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public string Id { get; set; }
        public string BlogId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Comment { get; set; }
        public string ParentId { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public string UserAgent { get; set; }
        public bool Deleted { get; set; }

        public static BlogCommentEntity Create(IBlogCommentData src)
        {
            var id = Guid.NewGuid().ToString("N");
            var result = new BlogCommentEntity
            {
                PartitionKey = GeneratePartitionKey(src.BlogId),
                RowKey = GenerateRowKey(id),
                BlogId = GeneratePartitionKey(src.BlogId),
                Id = id,
                Comment = src.Comment,
                UserId = src.UserId,
                FullName = src.FullName,
                Created = src.Created,
                LastModified = src.LastModified,
                ParentId = src.ParentId,
                UserAgent = src.UserAgent
            };

            return result;
        }

        internal void Update(IBlogCommentData src)
        {
            Comment = src.Comment;
            LastModified = DateTime.UtcNow;
            Deleted = src.Deleted;
        }
    }


    public class BlogCommentsRepository : IBlogCommentsRepository
    {
        private readonly INoSQLTableStorage<BlogCommentEntity> _blogCommentsTableStorage;

        public BlogCommentsRepository(INoSQLTableStorage<BlogCommentEntity> blogCommentsTableStorage)
        {
            _blogCommentsTableStorage = blogCommentsTableStorage;
        }

        public async Task<IEnumerable<IBlogCommentData>> GetBlogCommentsAsync(string blogId)
        {
            var partitionKey = BlogCommentEntity.GeneratePartitionKey(blogId);
            return await _blogCommentsTableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IBlogCommentData> GetBlogCommentAsync(string blogId, string commentId)
        {
            var partitionKey = BlogCommentEntity.GeneratePartitionKey(blogId);
            var rowKey = BlogCommentEntity.GenerateRowKey(commentId);

            return await _blogCommentsTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task SaveAsync(IBlogCommentData blogCommentData)
        {
            var newEntity = BlogCommentEntity.Create(blogCommentData);
            await _blogCommentsTableStorage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(IBlogCommentData blogCommentData, string blogId = null)
        {
            var partitionKey = blogId == null ? BlogCommentEntity.GeneratePartitionKey(blogCommentData.BlogId) : BlogCommentEntity.GeneratePartitionKey(blogId);

            var rowKey = BlogCommentEntity.GenerateRowKey(blogCommentData.Id);

            return _blogCommentsTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(blogCommentData);
                return itm;
            });
        }

        public async Task DeleteAsync(string blogId, string commentId)
        {
            var partitionKey = BlogCommentEntity.GeneratePartitionKey(blogId);
            var rowKey = BlogCommentEntity.GenerateRowKey(commentId);

            await _blogCommentsTableStorage.DeleteAsync(partitionKey, rowKey);
        }

        public async Task<int> GetBlogCommentsCountAsync(string blogId)
        {
            var comments = await GetBlogCommentsAsync(blogId);
            return comments.ToList().Count;
        }
    }
}
