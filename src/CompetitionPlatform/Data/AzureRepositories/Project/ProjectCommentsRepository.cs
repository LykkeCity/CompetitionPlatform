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

        public static string GenerateRowKey()
        {
            return (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19");
        }

        public string UserId { get; set; }
        public string FullName { get; set; }
        public string ProjectId { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        public static CommentEntity Create(ICommentData src)
        {
            var result = new CommentEntity()
            {
                RowKey = GenerateRowKey(),
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                Comment = src.Comment,
                UserId = src.UserId,
                FullName = src.FullName,
                Created = src.Created,
                LastModified = src.LastModified
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
            await _projectCommentsTableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, DateTime.Now);
        }

        public Task UpdateAsync(ICommentData projectData)
        {
            var partitionKey = CommentEntity.GeneratePartitionKey(projectData.ProjectId);
            var rowKey = CommentEntity.GenerateRowKey();

            return _projectCommentsTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(projectData);
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

