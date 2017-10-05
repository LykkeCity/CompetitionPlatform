﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
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
        public string UserIdentifier { get; set; }
        public string FullName { get; set; }
        public string ProjectId { get; set; }
        public string Comment { get; set; }
        public string ParentId { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public string UserAgent { get; set; }
        public bool Deleted { get; set; }

        public static CommentEntity Create(ICommentData src)
        {
            var id = Guid.NewGuid().ToString("N");
            var result = new CommentEntity
            {
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                RowKey = GenerateRowKey(id),
                ProjectId = GeneratePartitionKey(src.ProjectId),
                Id = id,
                Comment = src.Comment,
                UserId = src.UserId,
                UserIdentifier = src.UserIdentifier,
                FullName = src.FullName,
                Created = src.Created,
                LastModified = src.LastModified,
                ParentId = src.ParentId,
                UserAgent = src.UserAgent
            };

            return result;
        }

        internal void Update(ICommentData src)
        {
            Comment = src.Comment;
            LastModified = DateTime.UtcNow;
            Deleted = src.Deleted;
            UserIdentifier = src.UserIdentifier;
        }
    }

    public class ProjectCommentsRepository : IProjectCommentsRepository
    {
        private readonly INoSQLTableStorage<CommentEntity> _projectCommentsTableStorage;

        public ProjectCommentsRepository(INoSQLTableStorage<CommentEntity> projectCommentsTableStorage)
        {
            _projectCommentsTableStorage = projectCommentsTableStorage;
        }

        public async Task<IEnumerable<ICommentData>> GetProjectCommentsAsync(string projectId)
        {
            var partitionKey = CommentEntity.GeneratePartitionKey(projectId);
            return await _projectCommentsTableStorage.GetDataAsync(partitionKey);
        }

        public async Task<ICommentData> GetCommentAsync(string projectId, string commentId)
        {
            var partitionKey = CommentEntity.GeneratePartitionKey(projectId);
            var rowKey = CommentEntity.GenerateRowKey(commentId);
            return await _projectCommentsTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task SaveAsync(ICommentData projectCommentData)
        {
            var newEntity = CommentEntity.Create(projectCommentData);
            await _projectCommentsTableStorage.InsertAsync(newEntity);
        }

        public Task UpdateAsync(ICommentData projectCommentData, string projectId = null)
        {
            var partitionKey = projectId == null ? CommentEntity.GeneratePartitionKey(projectCommentData.ProjectId) : CommentEntity.GeneratePartitionKey(projectId);
            
            var rowKey = CommentEntity.GenerateRowKey(projectCommentData.Id);

            return _projectCommentsTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(projectCommentData);
                return itm;
            });
        }

        public async Task DeleteAsync(string projectId, string commentId)
        {
            var partitionKey = CommentEntity.GeneratePartitionKey(projectId);
            var rowKey = CommentEntity.GenerateRowKey(commentId);

            await _projectCommentsTableStorage.DeleteAsync(partitionKey, rowKey);
        }

        public async Task<int> GetProjectCommentsCountAsync(string projectId)
        {
            var comments = await GetProjectCommentsAsync(projectId);
            return comments.ToList().Count;
        }
    }
}

