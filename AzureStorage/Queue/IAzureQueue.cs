using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorage.Queue
{
    public class AzureQueueMessage<T>
    {
        public T Message { get; set; }
        internal CloudQueueMessage Token { get; set; }

        public static AzureQueueMessage<T> Create(T message, CloudQueueMessage token)
        {
            return new AzureQueueMessage<T>
            {
                Message = message,
                Token = token
            };
        }
    }

    public interface IAzureQueue<T>
    {
        Task PutMessageAsync(T itm);
        Task<AzureQueueMessage<T>> GetMessageAsync();

        Task ProcessMessageAsync(AzureQueueMessage<T> token);

        Task ClearAsync();

    }

}
