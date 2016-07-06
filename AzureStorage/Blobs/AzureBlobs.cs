using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorage.Blobs
{
    public class AzureBlobStorage : IAzureBlob
    {
        private readonly CloudBlobClient _blobClient;

        public AzureBlobStorage(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            _blobClient = storageAccount.CreateCloudBlobClient();

        }

        public async Task SaveBlobAsync(string container, string key, Stream bloblStream)
        {
            var containerRef = _blobClient.GetContainerReference(container);
            await containerRef.CreateIfNotExistsAsync();

            var blockBlob = containerRef.GetBlockBlobReference(key);

            bloblStream.Position = 0;
            await blockBlob.UploadFromStreamAsync(bloblStream);
        }

        public async Task SaveBlobAsync(string container, string key, byte[] blob)
        {
            var containerRef = _blobClient.GetContainerReference(container);
            await containerRef.CreateIfNotExistsAsync();

            var blockBlob = containerRef.GetBlockBlobReference(key);
            await blockBlob.UploadFromByteArrayAsync(blob, 0, blob.Length);
        }

        public async Task<Stream> GetAsync(string blobContainer, string key)
        {
            var containerRef = _blobClient.GetContainerReference(blobContainer);

            var blockBlob = containerRef.GetBlockBlobReference(key);
            var ms = new MemoryStream();
            await blockBlob.DownloadToStreamAsync(ms);
            ms.Position = 0;
            return ms;
        }

        public async Task<List<string>> FindNamesByPrefixAsync(string container, string prefix)
        {
            
          BlobContinuationToken continuationToken = null;
          List<string> results = new List<string>();
          var containerRef = _blobClient.GetContainerReference(container);
          
          do
          {
             var response = await containerRef.ListBlobsSegmentedAsync(continuationToken);
             continuationToken = response.ContinuationToken;
             results.AddRange(response.Results.Select(itm => itm.Uri.ToString()).Where(itm => itm.StartsWith(prefix)));
           }
            while (continuationToken != null);
            
            return results;
        }

        public async Task<List<string>> GetListOfBlobsAsync(string container)
        {
          BlobContinuationToken continuationToken = null;
          List<string> results = new List<string>();
          
          var containerRef = _blobClient.GetContainerReference(container);
          
          do
          {
             var response = await containerRef.ListBlobsSegmentedAsync(continuationToken);
             continuationToken = response.ContinuationToken;
             results.AddRange(response.Results.Select(itm => itm.Uri.ToString()));
           }
            while (continuationToken != null);
            
            return results;
        }


        public Task DelBlobAsync(string container, string key)
        {
            var containerRef = _blobClient.GetContainerReference(container);

            var blockBlob = containerRef.GetBlockBlobReference(key);
            return blockBlob.DeleteAsync();
        }

    }
}
