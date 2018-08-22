using System;
using AzureStorage.Queue;
using Common.Log;
using CompetitionPlatform.Data.AzureRepositories.Settings;
using CompetitionPlatform.Models;
using Lykke.Common.Log;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompetitionPlatform.Exceptions
{
    public class GlobalExceptionFilter : IExceptionFilter, IDisposable
    {
        private readonly ILog _log;
        private readonly IQueueExt _slackMessageQueue;

        public GlobalExceptionFilter(ILog log, IReloadingManager<BaseSettings> settings)
        {
            _log = log;
            
            if (!string.IsNullOrEmpty(settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString))
            {
                _slackMessageQueue = AzureQueueExt.Create(settings.ConnectionString(x => x.SlackNotifications.AzureQueue.ConnectionString), "slack-notifications");
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

            _log.Error("LykkeStreams", context.Exception, context.Exception.Message, $"Controller: {controller}, action: {action}");

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
