using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Users;
using Microsoft.Extensions.DependencyInjection;
using Common.Log;
using CompetitionPlatform.Services;
using AzureStorage.Tables;
using AzureStorage.Blobs;
using AzureStorage.Queue;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Vote;
using CompetitionPlatform.Data.ProjectCategory;
using CompetitionPlatform.Helpers;
using CompetitionPlatform.Models;
using Microsoft.WindowsAzure.Storage.Queue;

namespace CompetitionPlatform
{
    public static class ServiceRegistrator
    {
        public static void RegisterRepositories(this IServiceCollection services, string connectionString, ILog log)
        {
            services.AddSingleton<IAzureTableStorage<ProjectEntity>>(
                   new AzureTableStorage<ProjectEntity>(connectionString, "Projects", log));

            services.AddSingleton<IAzureBlob>(
                new AzureBlobStorage(connectionString));

            services.AddSingleton<IAzureTableStorage<UserEntity>>(
                new AzureTableStorage<UserEntity>(connectionString, "Users", log));

            services.AddSingleton<IAzureTableStorage<CommentEntity>>(
                new AzureTableStorage<CommentEntity>(connectionString, "ProjectComments", log));

            services.AddSingleton<IAzureTableStorage<ProjectFileInfoEntity>>(
                new AzureTableStorage<ProjectFileInfoEntity>(connectionString, "ProjectFilesInfo", log));

            services.AddSingleton<IAzureTableStorage<ProjectVoteEntity>>(
                new AzureTableStorage<ProjectVoteEntity>(connectionString, "ProjectVotes", log));

            services.AddSingleton<IAzureTableStorage<ProjectParticipateEntity>>(
                new AzureTableStorage<ProjectParticipateEntity>(connectionString, "ProjectParticipants", log));

            services.AddSingleton<IAzureTableStorage<ProjectResultEntity>>(
                new AzureTableStorage<ProjectResultEntity>(connectionString, "ProjectResults", log));

            services.AddSingleton<IAzureTableStorage<ProjectResultVoteEntity>>(
                new AzureTableStorage<ProjectResultVoteEntity>(connectionString, "ProjectResultVotes", log));

            services.AddSingleton<IAzureTableStorage<ProjectFollowEntity>>(
                new AzureTableStorage<ProjectFollowEntity>(connectionString, "ProjectFollows", log));

            services.AddSingleton<IAzureTableStorage<WinnerEntity>>(
                new AzureTableStorage<WinnerEntity>(connectionString, "Winners", log));

            services.AddSingleton<IAzureTableStorage<UserRoleEntity>>(
                new AzureTableStorage<UserRoleEntity>(connectionString, "UserRoles", log));

            services.AddSingleton<IAzureTableStorage<FollowMailSentEntity>>(
                new AzureTableStorage<FollowMailSentEntity>(connectionString, "FollowMailSent", log));

            services.AddSingleton<IAzureTableStorage<UserFeedbackEntity>>(
                new AzureTableStorage<UserFeedbackEntity>(connectionString, "UserFeedback", log));

            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IProjectFileRepository, ProjectFileRepository>();
            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddTransient<IProjectCommentsRepository, ProjectCommentsRepository>();
            services.AddTransient<IProjectFileInfoRepository, ProjectFileInfoRepository>();
            services.AddTransient<IProjectVoteRepository, ProjectVoteRepository>();
            services.AddTransient<IProjectParticipantsRepository, ProjectParticipantsRepository>();
            services.AddTransient<IProjectCategoriesRepository, ProjectCategoriesRepository>();
            services.AddTransient<IProjectResultRepository, ProjectResultRepository>();
            services.AddTransient<IProjectResultVoteRepository, ProjectResultVoteRepository>();
            services.AddTransient<IProjectFollowRepository, ProjectFollowRepository>();
            services.AddTransient<IProjectWinnersRepository, ProjectWinnersRepository>();
            services.AddTransient<IUserRolesRepository, UserRolesRepository>();
            services.AddTransient<IProjectWinnersService, ProjectWinnersService>();
            services.AddTransient<IFollowMailSentRepository, FollowMailSentRepository>();
            services.AddTransient<IUserFeedbackRepository, UserFeedbackRepository>();
        }

        public static void RegisterLyykeServices(this IServiceCollection services)
        {
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        public static void RegisterEmailNotificationServices(this IServiceCollection services, string emailsQueueConnString)
        {
            services.AddSingleton<IAzureQueue<string>>(new AzureQueue<string>(emailsQueueConnString, "emailsqueue"));
        }

        public static void RegisterSlackNotificationServices(this IServiceCollection services, string slackQueueConnString)
        {
            services.AddSingleton<IAzureQueue<SlackMessage>>(new AzureQueue<SlackMessage>(slackQueueConnString, "slack-notifications"));
        }

        public static void RegisterInMemoryNotificationServices(this IServiceCollection services)
        {
            services.AddSingleton<IAzureQueue<string>>(new QueueInMemory<string>());
        }
    }
}
