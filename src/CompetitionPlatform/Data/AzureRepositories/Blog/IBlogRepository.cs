using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Blog
{
    public interface IBlogData
    {
        string Id { get; }
        string Name { get; set; }
        string Overview { get; set; }
        string Text { get; set; }
        string ProjectId { get; set; }
        string ProjectName { get; set; }
        string FirstResult { get; set; }
        string FirstResultAuthor { get; set; }
        string FirstResultComment { get; set; }
        string SecondResult { get; set; }
        string SecondResultAuthor { get; set; }
        string SecondResultComment { get; set; }
        string ThirdResult { get; set; }
        string ThirdResultAuthor { get; set; }
        string ThirdResultComment { get; set; }
        string FourthResult { get; set; }
        string FourthResultAuthor { get; set; }
        string FourthResultComment { get; set; }
        string AuthorId { get; set; }
        string AuthorName { get; set; }
        string Category { get; set; }
        DateTime Posted { get; set; }
        DateTime Published { get; set; }
    }

    public interface IBlogRepository
    {
        Task<string> SaveAsync(IBlogData blogData);
        Task<IEnumerable<IBlogData>> GetBlogsAsync();
        Task<IBlogData> GetAsync(string id);
        Task UpdateAsync(IBlogData blogData);
    }
}
