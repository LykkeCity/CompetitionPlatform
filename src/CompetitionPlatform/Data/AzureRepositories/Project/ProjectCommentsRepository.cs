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

        public string User { get; set; }
        public string ProjectId { get; set; }
        public string Comment { get; set; }

        public static CommentEntity Create(ICommentData src)
        {
            var result = new CommentEntity()
            {
                RowKey = GenerateRowKey(),
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                Comment = src.Comment,
                User = src.User
            };

            return result;
        }
    }

    public class ProjectCommentsRepository : IProjectCommentsRepository
    {
        private readonly IAzureTableStorage<CommentEntity> _projectCommentsTableStorage;

        public ProjectCommentsRepository(IAzureTableStorage<CommentEntity> projectCommentsTableStorage)
        {
            _projectCommentsTableStorage = projectCommentsTableStorage;
        }

        public Task<ICommentData> GetAsync(string projectId, string username)
        {
            throw new NotImplementedException();
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

        public Task UpdateAsync(ICommentData projectData)
        {
            throw new NotImplementedException();
        }
    }
}

