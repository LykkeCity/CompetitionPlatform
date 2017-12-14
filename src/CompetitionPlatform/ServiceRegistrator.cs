using AzureStorage;
using AzureStorage.Blob;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Users;
using Microsoft.Extensions.DependencyInjection;
using Common.Log;
using CompetitionPlatform.Services;
using AzureStorage.Tables;
using AzureStorage.Queue;
using CompetitionPlatform.Data.AzureRepositories.Blog;
using CompetitionPlatform.Data.AzureRepositories.Expert;
using CompetitionPlatform.Data.AzureRepositories.ProjectStream;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Vote;
using CompetitionPlatform.Data.BlogCategory;
using CompetitionPlatform.Data.ProjectCategory;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Settings;

namespace CompetitionPlatform
{
    public static class ServiceRegistrator
    {
        //public static void RegisterRepositories(this IServiceCollection services, string connectionString, ILog log, string personalDatAapiKey, string personalDataServiceUri)
        //{
        //    services.AddSingleton(log);

        //    services.AddSingleton<INoSQLTableStorage<ProjectEntity>>(
        //           new AzureTableStorage<ProjectEntity>(connectionString, "Projects", log));

        //    services.AddSingleton<IBlobStorage>(
        //        new AzureBlobStorage(connectionString));

        //    services.AddSingleton<INoSQLTableStorage<UserEntity>>(
        //        new AzureTableStorage<UserEntity>(connectionString, "Users", log));

        //    services.AddSingleton<INoSQLTableStorage<CommentEntity>>(
        //        new AzureTableStorage<CommentEntity>(connectionString, "ProjectComments", log));

        //    services.AddSingleton<INoSQLTableStorage<ProjectFileInfoEntity>>(
        //        new AzureTableStorage<ProjectFileInfoEntity>(connectionString, "ProjectFilesInfo", log));

        //    services.AddSingleton<INoSQLTableStorage<ProjectVoteEntity>>(
        //        new AzureTableStorage<ProjectVoteEntity>(connectionString, "ProjectVotes", log));

        //    services.AddSingleton<INoSQLTableStorage<ProjectParticipateEntity>>(
        //        new AzureTableStorage<ProjectParticipateEntity>(connectionString, "ProjectParticipants", log));

        //    services.AddSingleton<INoSQLTableStorage<ProjectResultEntity>>(
        //        new AzureTableStorage<ProjectResultEntity>(connectionString, "ProjectResults", log));

        //    services.AddSingleton<INoSQLTableStorage<ProjectResultVoteEntity>>(
        //        new AzureTableStorage<ProjectResultVoteEntity>(connectionString, "ProjectResultVotes", log));

        //    services.AddSingleton<INoSQLTableStorage<ProjectFollowEntity>>(
        //        new AzureTableStorage<ProjectFollowEntity>(connectionString, "ProjectFollows", log));

        //    services.AddSingleton<INoSQLTableStorage<WinnerEntity>>(
        //        new AzureTableStorage<WinnerEntity>(connectionString, "Winners", log));

        //    services.AddSingleton<INoSQLTableStorage<UserRoleEntity>>(
        //        new AzureTableStorage<UserRoleEntity>(connectionString, "UserRoles", log));

        //    services.AddSingleton<INoSQLTableStorage<FollowMailSentEntity>>(
        //        new AzureTableStorage<FollowMailSentEntity>(connectionString, "FollowMailSent", log));

        //    services.AddSingleton<INoSQLTableStorage<UserFeedbackEntity>>(
        //        new AzureTableStorage<UserFeedbackEntity>(connectionString, "UserFeedback", log));

        //    services.AddSingleton<INoSQLTableStorage<BlogEntity>>(
        //        new AzureTableStorage<BlogEntity>(connectionString, "Blogs", log));

        //    services.AddSingleton<INoSQLTableStorage<BlogCommentEntity>>(
        //        new AzureTableStorage<BlogCommentEntity>(connectionString, "BlogComments", log));

        //    services.AddSingleton<INoSQLTableStorage<BlogPictureInfoEntity>>(
        //        new AzureTableStorage<BlogPictureInfoEntity>(connectionString, "BlogPicturesInfo", log));

        //    services.AddSingleton<INoSQLTableStorage<ProjectExpertEntity>>(
        //        new AzureTableStorage<ProjectExpertEntity>(connectionString, "ProjectExperts", log));

        //    services.AddSingleton<INoSQLTableStorage<StreamEntity>>(
        //        new AzureTableStorage<StreamEntity>(connectionString, "ProjectStreams", log));

        //    services.AddSingleton<IPersonalDataService>(
        //        new PersonalDataService(new PersonalDataServiceSettings { ApiKey = personalDatAapiKey, ServiceUri = personalDataServiceUri }, log));

        //    services.AddTransient<IProjectRepository, ProjectRepository>();
        //    services.AddTransient<IProjectFileRepository, ProjectFileRepository>();
        //    services.AddTransient<IUsersRepository, UsersRepository>();
        //    services.AddTransient<IProjectCommentsRepository, ProjectCommentsRepository>();
        //    services.AddTransient<IProjectFileInfoRepository, ProjectFileInfoRepository>();
        //    services.AddTransient<IProjectVoteRepository, ProjectVoteRepository>();
        //    services.AddTransient<IProjectParticipantsRepository, ProjectParticipantsRepository>();
        //    services.AddTransient<IProjectCategoriesRepository, ProjectCategoriesRepository>();
        //    services.AddTransient<IProjectResultRepository, ProjectResultRepository>();
        //    services.AddTransient<IProjectResultVoteRepository, ProjectResultVoteRepository>();
        //    services.AddTransient<IProjectFollowRepository, ProjectFollowRepository>();
        //    services.AddTransient<IProjectWinnersRepository, ProjectWinnersRepository>();
        //    services.AddTransient<IUserRolesRepository, UserRolesRepository>();
        //    services.AddTransient<IProjectWinnersService, ProjectWinnersService>();
        //    services.AddTransient<IFollowMailSentRepository, FollowMailSentRepository>();
        //    services.AddTransient<IUserFeedbackRepository, UserFeedbackRepository>();
        //    services.AddTransient<IBlogRepository, BlogRepository>();
        //    services.AddTransient<IBlogCategoriesRepository, BlogCategoriesRepository>();
        //    services.AddTransient<IBlogCommentsRepository, BlogCommentsRepository>();
        //    services.AddTransient<IBlogPictureRepository, BlogPictureRepository>();
        //    services.AddTransient<IBlogPictureInfoRepository, BlogPictureInfoRepository>();
        //    services.AddTransient<IProjectExpertsRepository, ProjectExpertsRepository>();
        //    services.AddTransient<IStreamRepository, StreamRepository>();
        //}

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