using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Blog
{
    public class BlogPictureInfoEntity : TableEntity, IBlogPictureInfoData
    {
        public static string GeneratePartitionKey()
        {
            return "BlogPictureInfo";
        }

        public static string GenerateRowKey(string blogId)
        {
            return blogId;
        }

        public string BlogId => RowKey;
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string ImageUrl { get; set; }

        public static BlogPictureInfoEntity Create(IBlogPictureInfoData src)
        {
            var result = new BlogPictureInfoEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.BlogId),
                FileName = src.FileName,
                ContentType = src.ContentType,
                ImageUrl = src.ImageUrl
            };

            return result;
        }
    }

    public class BlogPictureInfoRepository : IBlogPictureInfoRepository
    {
        private readonly INoSQLTableStorage<BlogPictureInfoEntity> _blogPictureInfoTableStorage;

        public BlogPictureInfoRepository(INoSQLTableStorage<BlogPictureInfoEntity> blogPictureInfoTableStorage)
        {
            _blogPictureInfoTableStorage = blogPictureInfoTableStorage;
        }

        public async Task<IBlogPictureInfoData> GetAsync(string blogId)
        {
            var partitionKey = BlogPictureInfoEntity.GeneratePartitionKey();
            var rowKey = BlogPictureInfoEntity.GenerateRowKey(blogId);

            return await _blogPictureInfoTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task SaveAsync(IBlogPictureInfoData fileInfoData)
        {
            var newEntity = BlogPictureInfoEntity.Create(fileInfoData);
            await _blogPictureInfoTableStorage.InsertOrReplaceAsync(newEntity);
        }
    }
}
