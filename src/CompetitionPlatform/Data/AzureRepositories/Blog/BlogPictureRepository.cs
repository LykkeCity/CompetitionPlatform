using System.IO;
using System.Threading.Tasks;
using AzureStorage;

namespace CompetitionPlatform.Data.AzureRepositories.Blog
{
    public class BlogPictureRepository : IBlogPictureRepository
    {
        private readonly IBlobStorage _blobStorage;

        private const string ContainerName = "blogpictures";

        public BlogPictureRepository(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task<string> InsertBlogPicture(Stream stream, string blogId)
        {
            await _blobStorage.SaveBlobAsync(ContainerName, blogId, stream);
            return _blobStorage.GetBlobUrl(ContainerName, blogId);
        }

        public async Task<Stream> GetBlogPicture(string blogId)
        {
            return await _blobStorage.GetAsync(ContainerName, blogId);
        }
    }
}
