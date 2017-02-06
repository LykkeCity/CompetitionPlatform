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
        private readonly IQueueExt _slackMessageQueue;

        public GlobalExceptionFilter(ILog log, string slackNotificationsConnString)
        {
            _log = log;

            if (!string.IsNullOrEmpty(slackNotificationsConnString))
            {
                _slackMessageQueue = new AzureQueueExt(slackNotificationsConnString, "slack-notifications");
            }
            else
            {
                _slackMessageQueue = new QueueExtInMemory();
            }
        }

        public void OnException(ExceptionContext context)
        {
            var controller = context.RouteData.Values["controller"];
            var action = context.RouteData.Values["action"];

            _log.WriteErrorAsync("Exception", "LykkeStreams", $"Controller: {controller}, action: {action}", context.Exception).Wait();

            var message = new SlackMessage
            {
                Type = "Errors", //Errors, Info
                Sender = "Lykke Streams",
                Message = "Occured in: " + controller + "Controller, " + action + " - " + context.Exception.GetType()
            };

            _slackMessageQueue.PutMessageAsync(message).Wait();
        }

        public void Dispose()
        {

        }
    }
}
