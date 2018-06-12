using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Admin
{
    public class TermsPageEntity : TableEntity, ITermsPageData
    {
        public string ProjectId { get; set; }
        public string Content { get; set; }

        public static string GeneratePartitionKey()
        {
            return "TermsPage";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }
        
        public static TermsPageEntity Create(string projectId, string content)
        {
            var result = new TermsPageEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(projectId),
                ProjectId = projectId,
                Content = content
            };

            return result;
        }

    }

    public class TermsPageRepository : ITermsPageRepository
    {
        private readonly INoSQLTableStorage<TermsPageEntity> _termsPageTableStorage;

        public TermsPageRepository(INoSQLTableStorage<TermsPageEntity> termsPageTableStorage)
        {
            _termsPageTableStorage = termsPageTableStorage;
        }

        public async Task SaveAsync(string projectId, string content)
        {
            var newEntity = TermsPageEntity.Create(projectId, content);
            await _termsPageTableStorage.InsertOrMergeAsync(newEntity);
        }

        public async Task<ITermsPageData> GetAsync(string projectId)
        {
            var partitionKey = TermsPageEntity.GeneratePartitionKey();
            var rowKey = TermsPageEntity.GenerateRowKey(projectId);

            var data = await _termsPageTableStorage.GetDataAsync(partitionKey, rowKey);
            return data;
        }
    }
}
