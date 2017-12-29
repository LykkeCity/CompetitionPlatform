﻿using Microsoft.Extensions.DependencyInjection;
using CompetitionPlatform.Services;
using AzureStorage.Queue;

namespace CompetitionPlatform
{
    public static class ServiceRegistrator
    {
        public static void RegisterLyykeServices(this IServiceCollection services)
        {
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        public static void RegisterSlackNotificationServices(this IServiceCollection services, string slackQueueConnString)
        {
            services.AddSingleton<IQueueExt>(new AzureQueueExt(slackQueueConnString, "slack-notifications"));
        }

        public static void RegisterInMemoryNotificationServices(this IServiceCollection services)
        {
            services.AddSingleton<IQueueExt>(new QueueExtInMemory());
        }
    }
}