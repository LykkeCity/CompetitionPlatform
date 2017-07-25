using System.IO;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Blog
{
    public interface IBlogPictureRepository
    {
        Task<string> InsertBlogPicture(Stream stream, string blogId);
        Task<Stream> GetBlogPicture(string blogId);
    }
}
