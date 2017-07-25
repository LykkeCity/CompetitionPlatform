using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Blog
{
    public interface IBlogPictureInfoData
    {
        string BlogId { get; }
        string FileName { get; set; }
        string ContentType { get; set; }
        string ImageUrl { get; set; }
    }
    public interface IBlogPictureInfoRepository
    {
        Task<IBlogPictureInfoData> GetAsync(string blogId);
        Task SaveAsync(IBlogPictureInfoData fileInfoData);
    }
}
