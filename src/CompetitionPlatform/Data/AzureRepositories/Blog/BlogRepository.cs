using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Blog
{
    public class BlogEntity : TableEntity, IBlogData
    {
        public static string GeneratePartitionKey()
        {
            return "Blog";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public string Id => RowKey;
        public string Name { get; set; }
        public string Overview { get; set; }
        public string Text { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string FirstResult { get; set; }
        public string FirstResultAuthor { get; set; }
        public string FirstResultComment { get; set; }
        public string SecondResult { get; set; }
        public string SecondResultAuthor { get; set; }
        public string SecondResultComment { get; set; }
        public string ThirdResult { get; set; }
        public string ThirdResultAuthor { get; set; }
        public string ThirdResultComment { get; set; }
        public string FourthResult { get; set; }
        public string FourthResultAuthor { get; set; }
        public string FourthResultComment { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string Category { get; set; }
        public DateTime Posted { get; set; }
        public DateTime Published { get; set; }

        internal void Update(IBlogData src)
        {
            Name = src.Name;
            Overview = src.Overview;
            Text = src.Text;
            ProjectId = src.ProjectId;
            ProjectName = src.ProjectName;
            FirstResult = src.FirstResult;
            FirstResultComment = src.FirstResultComment;
            SecondResult = src.SecondResult;
            SecondResultAuthor = src.SecondResultAuthor;
            SecondResultComment = src.SecondResultComment;
            ThirdResult = src.ThirdResult;
            ThirdResultAuthor = src.ThirdResultAuthor;
            ThirdResultComment = src.ThirdResultComment;
            FourthResult = src.FourthResult;
            FourthResultAuthor = src.FourthResultAuthor;
            FourthResultComment = src.FourthResultComment;
            Category = src.Category;
            Published = src.Published;
        }

        public static BlogEntity Create(IBlogData src)
        {
            var result = new BlogEntity
            {
                RowKey = src.Id,
                PartitionKey = GeneratePartitionKey(),
                Name = src.Name,
                AuthorId = src.AuthorId,
                AuthorName = src.AuthorName,
                Category = src.Category,
                FirstResult = src.FirstResult,
                FirstResultAuthor = src.FirstResultAuthor,
                FirstResultComment = src.FirstResultComment,
                SecondResult = src.SecondResult,
                SecondResultAuthor = src.SecondResultAuthor,
                SecondResultComment = src.SecondResultComment,
                ThirdResult = src.ThirdResult,
                ThirdResultAuthor = src.ThirdResultAuthor,
                ThirdResultComment = src.ThirdResultComment,
                FourthResult = src.FourthResult,
                FourthResultAuthor = src.FourthResultAuthor,
                FourthResultComment = src.FourthResultComment,
                Overview = src.Overview,
                Posted = src.Posted,
                Published = src.Published,
                ProjectId = src.ProjectId,
                ProjectName = src.ProjectName,
                Text = src.Text
            };

            return result;
        }
    }

    public class BlogRepository : IBlogRepository
    {
        private readonly INoSQLTableStorage<BlogEntity> _blogsTableStorage;

        public BlogRepository(INoSQLTableStorage<BlogEntity> blogsTableStorage)
        {
            _blogsTableStorage = blogsTableStorage;
        }

        public async Task<string> SaveAsync(IBlogData blogData)
        {
            var newEntity = BlogEntity.Create(blogData);
            await _blogsTableStorage.InsertAsync(newEntity);
            return newEntity.Id;
        }

        public async Task<IBlogData> GetAsync(string id)
        {
            var partitionKey = BlogEntity.GeneratePartitionKey();
            var rowKey = BlogEntity.GenerateRowKey(id);

            return await _blogsTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public Task UpdateAsync(IBlogData blogData)
        {
            var partitionKey = BlogEntity.GeneratePartitionKey();
            var rowKey = BlogEntity.GenerateRowKey(blogData.Id);

            return _blogsTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(blogData);
                return itm;
            });
        }

        public async Task<IEnumerable<IBlogData>> GetBlogsAsync()
        {
            var partitionKey = BlogEntity.GeneratePartitionKey();
            return await _blogsTableStorage.GetDataAsync(partitionKey);
        }
    }
}
