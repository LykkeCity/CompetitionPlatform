using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureStorage.Queue
{
    public class QueueInMemory<T> : IAzureQueue<T> where T:class
    {
        private readonly Queue<T> _queue = new Queue<T>();

        public Task PutMessageAsync(T itm)
        {
            lock(_queue)
            _queue.Enqueue(itm);
            return Task.FromResult(0);
        }

        public Task ClearAsync()
        {
                        lock (_queue)
                _queue.Clear();
            return Task.FromResult(0);
        }

        public Task<AzureQueueMessage<T>> GetMessageAsync()
        {
            lock(_queue){
                
                if (_queue.Count == 0)
                return Task.FromResult(AzureQueueMessage<T>.Create(default(T), null));
                
                return Task.FromResult(AzureQueueMessage<T>.Create(_queue.Dequeue(), null));
            }
        }

        public Task ProcessMessageAsync(AzureQueueMessage<T> token)
        {
            return Task.FromResult(0);
        }
    }
}
