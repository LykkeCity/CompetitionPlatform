﻿using System.Threading.Tasks;
using AzureStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public class ProjectFileInfoEntity : TableEntity, IProjectFileInfoData
    {
        public static string GeneratePartitionKey()
        {
            return "ProjectFileInfo";
        }

        public static string GenerateRowKey(string projectId)
        {
            return projectId;
        }

        public string ProjectId => RowKey;
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public static ProjectFileInfoEntity Create(IProjectFileInfoData src)
        {
            var result = new ProjectFileInfoEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.ProjectId),
                FileName = src.FileName,
                ContentType = src.ContentType
            };

            return result;
        }

        public static ProjectFileInfoEntity Create(IFormFile file, string projectId)
        {
            var result = new ProjectFileInfoEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(projectId),
                FileName = file.FileName,
                ContentType = file.ContentType
            };

            return result;
        }
    }

    public class ProjectFileInfoRepository : IProjectFileInfoRepository
    {
        private readonly INoSQLTableStorage<ProjectFileInfoEntity> _projectFileInfoTableStorage;

        public ProjectFileInfoRepository(INoSQLTableStorage<ProjectFileInfoEntity> projectFileInfoTableStorage)
        {
            _projectFileInfoTableStorage = projectFileInfoTableStorage;
        }

        public async Task<IProjectFileInfoData> GetAsync(string projectId)
        {
            var partitionKey = ProjectFileInfoEntity.GeneratePartitionKey();
            var rowKey = ProjectFileInfoEntity.GenerateRowKey(projectId);

            return await _projectFileInfoTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task SaveAsync(IProjectFileInfoData fileInfoData)
        {
            var newEntity = ProjectFileInfoEntity.Create(fileInfoData);
            await _projectFileInfoTableStorage.InsertOrReplaceAsync(newEntity);
        }
    }
}
