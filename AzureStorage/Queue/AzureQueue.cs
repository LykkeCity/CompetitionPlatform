using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorage.Queue
{

    public class AzureQueue<T> : IAzureQueue<T> where T : class
    {
        private readonly CloudQueue _queue;

        public AzureQueue(string conectionString, string queueName)
        {
            queueName = queueName.ToLower();
            var storageAccount = CloudStorageAccount.Parse(conectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();

            _queue = queueClient.GetQueueReference(queueName);
            _queue.CreateIfNotExistsAsync().Wait();
        }

        public Task PutMessageAsync(T itm)
        {
            var msg = Newtonsoft.Json.JsonConvert.SerializeObject(itm);
            return _queue.AddMessageAsync(new CloudQueueMessage(msg));
        }


        public async Task<AzureQueueMessage<T>> GetMessageAsync()
        {
            var msg = await _queue.GetMessageAsync();
            if (msg == null)
                return null;

            await _queue.DeleteMessageAsync(msg);
            return AzureQueueMessage<T>.Create(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(msg.AsString), msg);

        }
        
        public Task ProcessMessageAsync(AzureQueueMessage<T> token)
        {
            return _queue.DeleteMessageAsync(token.Token);
        }

        public Task ClearAsync()
        {
            return _queue.ClearAsync();
        }

    }

}
