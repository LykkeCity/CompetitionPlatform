using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Users;
using Microsoft.Extensions.DependencyInjection;
using Common.Log;
using CompetitionPlatform.Services;
using AzureStorage.Tables;
using AzureStorage.Blobs;

namespace CompetitionPlatform
{
    public static class ServiceRegistrator
    {
        public static void RegisterRepositories(this IServiceCollection services, string connstionString, ILog log)
        {
            services.AddSingleton<IAzureTableStorage<ProjectEntity>>(
                   new AzureTableStorage<ProjectEntity>(connstionString, "Projects", log));

            services.AddSingleton<IAzureBlob>(
                new AzureBlobStorage(connstionString));

            services.AddSingleton<IAzureTableStorage<UserEntity>>(
                new AzureTableStorage<UserEntity>(connstionString, "Users", log));

            services.AddSingleton<IAzureTableStorage<CommentEntity>>(
                new AzureTableStorage<CommentEntity>(connstionString, "ProjectComments", log));

            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IProjectFileRepository, ProjectFileRepository>();
            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddTransient<IProjectCommentsRepository, ProjectCommentsRepository>();

        }


        public static void RegisterLyykeServices(this IServiceCollection services)
        {
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }
    }
}
