using System.IO;
using System.Threading.Tasks;
using AzureStorage;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public class ProjectFileRepository : IProjectFileRepository
    {
        private readonly IBlobStorage _blobStorage;

        private const string ContainerName = "projectfiles";

        public ProjectFileRepository(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task InsertProjectFile(Stream stream, string projectId)
        {
            await _blobStorage.SaveBlobAsync(ContainerName, projectId, stream);
        }

        public async Task<Stream> GetProjectFile(string projectId)
        {
            return await _blobStorage.GetAsync(ContainerName, projectId);
        }
    }
}
