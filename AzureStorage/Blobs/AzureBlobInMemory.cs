using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace AzureStorage.Blobs
{

    internal static class BlobInMemoryHelper
    {
        public static void AddOrReplace(this Dictionary<string, byte[]> blob, string key, byte[] data)
        {
            if (blob.ContainsKey(key))
            {
                blob[key] = data;
                return;
            }


            blob.Add(key, data);
        }

        public static byte[] GetOrNull(this Dictionary<string, byte[]> blob, string key)
        {
            if (blob.ContainsKey(key))
                return blob[key];


            return null;
        }
    }


    public class AzureBlobInMemory : IAzureBlob
    {


        private Dictionary<string, Dictionary<string, byte[]>> _blobs = new Dictionary<string, Dictionary<string, byte[]>>();


        private object _lockObject = new object();


        private Dictionary<string, byte[]> GetBlob(string container)
        {
            if (!_blobs.ContainsKey(container))
                _blobs.Add(container, new Dictionary<string, byte[]>());


            return _blobs[container];

        } 

        public void SaveBlob(string container, string key, Stream bloblStream)
        {
            lock (_lockObject)
                GetBlob(container).AddOrReplace(key, bloblStream.ToBytes());
        }

        public Task SaveBlobAsync(string container, string key, Stream bloblStream)
        {
            SaveBlob(container, key, bloblStream);
            return Task.FromResult(0);
        }

        public Task SaveBlobAsync(string container, string key, byte[] blob)
        {
            lock (_lockObject)
                GetBlob(container).AddOrReplace(key, blob);
            return Task.FromResult(0);
        }

        public Task<Stream> GetAsync(string container, string key)
        {
            return Task.FromResult(GetBlob(container).GetOrNull(key).ToStream());
        }

        public Task<List<string>> FindNamesByPrefixAsync(string container, string prefix)
        {
            lock (_lockObject)
                return Task.FromResult(GetBlob(container).Where(itm => itm.Key.StartsWith(prefix)).Select(itm => itm.Key).ToList());
        }

        public Task<List<string>> GetListOfBlobsAsync(string container)
        {
            lock (_lockObject){
                return Task.FromResult(GetBlob(container).Select(itm => itm.Key).ToList());
            }
                
        }

        public Task DelBlobAsync(string blobContainer, string key)
        {
            lock (_lockObject)
                GetBlob(blobContainer).Remove(key);

            return Task.FromResult(0);
        }
    }
}
