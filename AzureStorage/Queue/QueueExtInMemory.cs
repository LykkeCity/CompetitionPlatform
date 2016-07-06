using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureStorage.Queue
{
    public class QueueExtInMemory : IAzureQueueExt
    {
        private Queue<object> _queue = new Queue<object>(); 

        public Task PutMessageAsync(object itm)
        {
          lock(_queue)
                _queue.Enqueue(itm);
              return Task.FromResult(0);
        }

        public object GetMessage()
        {
            lock (_queue)
                return _queue.Dequeue();
        }

        public Task<QueueData> GetMessageAsync()
        {
            var item = GetMessage();

            var queueData = new QueueData
            {
                Data = item,
                Token = item
            };
            return Task.FromResult(queueData);
        }

        public Task FinishMessageAsync(QueueData token)
        {
            return Task.FromResult(0);
        }


        public Task<object[]> GetMessagesAsync(int maxCount)
        {
            lock(_queue){
                var result = new List<object>();
                
                while(maxCount>0){
                    if (_queue.Count == 0)
                      break;
                      
                    result.Add(_queue.Dequeue());
                    
                }
                
                return Task.FromResult(result.ToArray());
            }
            
        }

        public Task ClearAsync()
        {
            lock (_queue)
                _queue.Clear();
                
                return Task.FromResult(0);
        }

        public void RegisterTypes(params QueueType[] type)
        {

        }
    }
}
