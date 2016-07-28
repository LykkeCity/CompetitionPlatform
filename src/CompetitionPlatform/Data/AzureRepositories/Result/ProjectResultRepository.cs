using System.IO;
using System.Threading.Tasks;
using AzureStorage.Blobs;

namespace CompetitionPlatform.Data.AzureRepositories.Result
{
    public class ProjectResultRepository : IProjectResultRepository
    {
        private readonly IAzureBlob _blobStorage;

        private const string ContainerName = "projectresults";

        public ProjectResultRepository(IAzureBlob blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task InsertProjectResult(Stream stream, string resultId)
        {
            await _blobStorage.SaveBlobAsync(ContainerName, resultId, stream);
        }

        public async Task<Stream> GetProjectResult(string resultId)
        {
            return await _blobStorage.GetAsync(ContainerName, resultId);
        }
    }
}
