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
using CompetitionPlatform.Data.AzureRepositories.Vote;

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

            services.AddSingleton<IAzureTableStorage<ProjectFileInfoEntity>>(
                new AzureTableStorage<ProjectFileInfoEntity>(connstionString, "ProjectFilesInfo", log));

            services.AddSingleton<IAzureTableStorage<ProjectVoteEntity>>(
                new AzureTableStorage<ProjectVoteEntity>(connstionString, "ProjectVotes", log));

            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IProjectFileRepository, ProjectFileRepository>();
            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddTransient<IProjectCommentsRepository, ProjectCommentsRepository>();
            services.AddTransient<IProjectFileInfoRepository, ProjectFileInfoRepository>();
            services.AddTransient<IProjectVoteRepository, ProjectVoteRepository>();
        }

        public static void RegisterLyykeServices(this IServiceCollection services)
        {
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }
    }
}
