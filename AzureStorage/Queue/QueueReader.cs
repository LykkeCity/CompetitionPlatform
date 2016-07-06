using System;
using System.Threading.Tasks;
using Common;
using Common.Log;

namespace AzureStorage.Queue
{
    public class QueueReader : TimerPeriod
    {
        private readonly IAzureQueueExt _queueExt;
        private readonly Func<object, Task<bool>> _messageHandler;

        public QueueReader(IAzureQueueExt queueExt, string componentName, int periodMs, ILog log, Func<object, Task<bool>> messageHandler) : base(componentName, periodMs, log)
        {
            _queueExt = queueExt;
            _messageHandler = messageHandler;
        }

        protected override async Task Execute()
        {
            var queueData = await _queueExt.GetMessageAsync();

            while (queueData != null)
            {
                var result = await _messageHandler(queueData.Data);
                if (result)
                    await _queueExt.FinishMessageAsync(queueData);

                queueData = await _queueExt.GetMessageAsync();
            }
        }
    }
}
