using System;
using AzureStorage.Queue;
using Common.Log;
using CompetitionPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompetitionPlatform.Exceptions
{
    public class GlobalExceptionFilter : IExceptionFilter, IDisposable
    {
        private readonly ILog _log;
        private readonly IAzureQueue<SlackMessage> _slackMessageQueue;

        public GlobalExceptionFilter(ILog log, string slackNotificationsConnString)
        {
            _log = log;

            if (!string.IsNullOrEmpty(slackNotificationsConnString))
            {
                _slackMessageQueue = new AzureQueue<SlackMessage>(slackNotificationsConnString, "slack-notifications");
            }
            _slackMessageQueue = new QueueInMemory<SlackMessage>();
        }

        public void OnException(ExceptionContext context)
        {
            var controller = context.RouteData.Values["controller"];
            var action = context.RouteData.Values["action"];

            _log.WriteError("Exception", "LykkeStreams", $"Controller: {controller}, action: {action}", context.Exception).Wait();

            var message = new SlackMessage
            {
                Type = "Errors", //Errors, Info
                Sender = "Lykke Streams",
                Message = "Message occured in: " + controller + ", " + action + " - " + context.Exception
            };

            _slackMessageQueue.PutMessageAsync(message).Wait();
        }

        public void Dispose()
        {

        }
    }
}
