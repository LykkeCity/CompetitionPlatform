using AzureStorage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories
{
    public class ProjectFileRepository : IProjectFileRepository
    {
        private readonly IAzureBlob _blobStorage;

        private const string ContainerName = "projectfiles";

        public ProjectFileRepository(IAzureBlob blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task<string> InsertAttachment(Stream stream)
        {
            var key = Guid.NewGuid().ToString("N");
            await _blobStorage.SaveBlobAsync(ContainerName, key, stream);
            return key;
        }

        public async Task<Stream> GetAttachment(string fileId)
        {
            return await _blobStorage.GetAsync(ContainerName, fileId);
        }
    }
}
