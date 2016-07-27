using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public async Task InsertProjectResult(Stream stream, string projectId, string userName)
        {
            await _blobStorage.SaveBlobAsync(ContainerName, projectId + userName, stream);
        }

        public async Task<Stream> GetProjectResult(string projectId, string userName)
        {
            return await _blobStorage.GetAsync(ContainerName, projectId + userName);
        }
    }
}
