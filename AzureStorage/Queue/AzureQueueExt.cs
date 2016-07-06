using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorage.Queue
{
    
    public interface IQueueMessageSerializer{
        string Serialize(object src);
        object Deserialize(string src, Type t);
    }

    public class DefaultQueueSerializer : IQueueMessageSerializer
    {
        public object Deserialize(string src, Type t)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(src, t);
        }

        public string Serialize(object src)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(src);
        }
    }


    public class AzureQueueExt : IAzureQueueExt
    {
        private readonly CloudQueue _queue;

        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>(); 
        
        private IQueueMessageSerializer _serializer;

        public AzureQueueExt(string conectionString, string queueName, IQueueMessageSerializer serializer = null)
        {
            queueName = queueName.ToLower();
            var storageAccount = CloudStorageAccount.Parse(conectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();

            _queue = queueClient.GetQueueReference(queueName);
            _queue.CreateIfNotExistsAsync().Wait();
            _serializer = serializer ?? new DefaultQueueSerializer();
        }

        private string SerializeObject(object itm)
        {
            var myType = itm.GetType();
            return (from tp in _types where tp.Value == myType select tp.Key + ":" + _serializer.Serialize(itm)).FirstOrDefault();
        }

        private object DeserializeObject(string itm)
        {
            var i = itm.IndexOf(':');

            var typeStr = itm.Substring(0, i);

            if (!_types.ContainsKey(typeStr))
                return null;

            var data = itm.Substring(i + 1, itm.Count() - i-1);
            
            return _serializer.Deserialize(data, _types[typeStr]);
            
        }

        public async Task<QueueData> GetMessageAsync()
        {
            var msg = await _queue.GetMessageAsync();

            if (msg == null)
                return null;

            return new QueueData
            {
                Token = msg,
                Data = DeserializeObject(msg.AsString)
            };
        }


        public Task PutMessageAsync(object itm)
        {
            var msg = SerializeObject(itm);
            if (itm == null)
                Task.FromResult(0);

            return _queue.AddMessageAsync(new CloudQueueMessage(msg));
        }


        public async Task<object[]> GetMessagesAsync(int maxCount)
        {
            var messages = await _queue.GetMessagesAsync(maxCount);

            var cloudQueueMessages = messages as CloudQueueMessage[] ?? messages.ToArray();
            foreach (var cloudQueueMessage in cloudQueueMessages)
                await _queue.DeleteMessageAsync(cloudQueueMessage);

            return cloudQueueMessages
                .Select(message => DeserializeObject(message.AsString))
                .Where(itm => itm != null).ToArray();
        }
        
        
        public Task FinishMessageAsync(QueueData token)
        {
            var cloudQueueMessage = token.Token as CloudQueueMessage;
            if (cloudQueueMessage == null)
                return Task.FromResult(0);
             
            return _queue.DeleteMessageAsync(cloudQueueMessage);
        }

        public Task ClearAsync()
        {
            return _queue.ClearAsync();
        }

        public void RegisterTypes(params QueueType[] types)
        {
            foreach (var type in types)
                _types.Add(type.Id, type.Type);
        }

    }
}
