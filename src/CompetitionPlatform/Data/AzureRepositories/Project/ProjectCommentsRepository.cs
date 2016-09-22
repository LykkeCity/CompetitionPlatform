using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public class CommentEntity : TableEntity, ICommentData
    {
        public static string GeneratePartitionKey(string projectId)
        {
            return projectId;
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public string Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string ProjectId { get; set; }
        public string Comment { get; set; }
        public string ParentId { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        public static CommentEntity Create(ICommentData src)
        {
            var id = Guid.NewGuid().ToString("N");
            var result = new CommentEntity
            {
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                RowKey = GenerateRowKey(id),
                Id = id,
                Comment = src.Comment,
                UserId = src.UserId,
                FullName = src.FullName,
                Created = src.Created,
                LastModified = src.LastModified,
                ParentId = src.ParentId
            };

            return result;
        }

        internal void Update(ICommentData src)
        {
            Comment = src.Comment;
            LastModified = DateTime.UtcNow;
        }
    }

    public class ProjectCommentsRepository : IProjectCommentsRepository
    {
        private readonly IAzureTableStorage<CommentEntity> _projectCommentsTableStorage;

        public ProjectCommentsRepository(IAzureTableStorage<CommentEntity> projectCommentsTableStorage)
        {
            _projectCommentsTableStorage = projectCommentsTableStorage;
        }

        public async Task<IEnumerable<ICommentData>> GetProjectCommentsAsync(string projectId)
        {
            var partitionKey = CommentEntity.GeneratePartitionKey(projectId);
            return await _projectCommentsTableStorage.GetDataAsync(partitionKey);
        }

        public async Task SaveAsync(ICommentData projectCommentData)
        {
            var newEntity = CommentEntity.Create(projectCommentData);
            await _projectCommentsTableStorage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(ICommentData projectCommentData)
        {
            var partitionKey = CommentEntity.GeneratePartitionKey(projectCommentData.ProjectId);
            var rowKey = CommentEntity.GenerateRowKey(projectCommentData.Id);

            return _projectCommentsTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(projectCommentData);
                return itm;
            });
        }

        public async Task<int> GetProjectCommentsCountAsync(string projectId)
        {
            var comments = await GetProjectCommentsAsync(projectId);
            return comments.ToList().Count;
        }
    }
}

