using Autofac;
using AzureStorage;
using AzureStorage.Blob;
using AzureStorage.Tables;
using Common.Log;
using CompetitionPlatform.Data.AzureRepositories.Admin;
using CompetitionPlatform.Data.AzureRepositories.Blog;
using CompetitionPlatform.Data.AzureRepositories.Expert;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.ProjectStream;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Settings;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.AzureRepositories.Vote;
using CompetitionPlatform.Data.BlogCategory;
using CompetitionPlatform.Data.ProjectCategory;
using CompetitionPlatform.Services;
using Lykke.Common.Log;
using Lykke.Messages.Email;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Settings;
using Lykke.SettingsReader;
using System;

namespace CompetitionPlatform.Modules
{
    public class DbModule : Module
    {
        private readonly IReloadingManager<BaseSettings> _settings;
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;


        public DbModule(IReloadingManager<BaseSettings> settings, ILogFactory logFactory)
        {
            _settings = settings;

            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        protected override void Load(ContainerBuilder builder)
        {
            var connectionString = _settings.ConnectionString(x => x.LykkeStreams.Azure.StorageConnString);
            var personalDatAapiKey = _settings.CurrentValue.LykkeStreams.PersonalDataService.ApiKey;
            var personalDataServiceUri = _settings.CurrentValue.LykkeStreams.PersonalDataService.ServiceUri;
            var kycSettings = _settings.CurrentValue.KycServiceClient;

            builder.RegisterInstance(
              AzureBlobStorage.Create(connectionString)).As<IBlobStorage>().SingleInstance();

            builder.RegisterInstance(
                new ProjectRepository(AzureTableStorage<ProjectEntity>.Create(connectionString, "Projects", _logFactory))
            ).As<IProjectRepository>().SingleInstance();


            builder.RegisterInstance(
                new UsersRepository(AzureTableStorage<UserEntity>.Create(connectionString, "Users", _logFactory))
            ).As<IUsersRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectCommentsRepository(AzureTableStorage<CommentEntity>.Create(connectionString, "ProjectComments", _logFactory))
            ).As<IProjectCommentsRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectFileInfoRepository(AzureTableStorage<ProjectFileInfoEntity>.Create(connectionString, "ProjectFilesInfo", _logFactory))
            ).As<IProjectFileInfoRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectVoteRepository(AzureTableStorage<ProjectVoteEntity>.Create(connectionString, "ProjectVotes", _logFactory))
            ).As<IProjectVoteRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectParticipantsRepository(AzureTableStorage<ProjectParticipateEntity>.Create(connectionString, "ProjectParticipants", _logFactory))
            ).As<IProjectParticipantsRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectResultRepository(AzureTableStorage<ProjectResultEntity>.Create(connectionString, "ProjectResults", _logFactory))
            ).As<IProjectResultRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectResultVoteRepository(AzureTableStorage<ProjectResultVoteEntity>.Create(connectionString, "ProjectResultVotes", _logFactory))
            ).As<IProjectResultVoteRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectFollowRepository(AzureTableStorage<ProjectFollowEntity>.Create(connectionString, "ProjectFollows", _logFactory))
            ).As<IProjectFollowRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectWinnersRepository(AzureTableStorage<WinnerEntity>.Create(connectionString, "Winners", _logFactory))
            ).As<IProjectWinnersRepository>().SingleInstance();

            builder.RegisterInstance(
                new UserRolesRepository(AzureTableStorage<UserRoleEntity>.Create(connectionString, "UserRoles", _logFactory))
            ).As<IUserRolesRepository>().SingleInstance();

            builder.RegisterInstance(
                new FollowMailSentRepository(AzureTableStorage<FollowMailSentEntity>.Create(connectionString, "FollowMailSent", _logFactory))
            ).As<IFollowMailSentRepository>().SingleInstance();

            builder.RegisterInstance(
                new UserFeedbackRepository(AzureTableStorage<UserFeedbackEntity>.Create(connectionString, "UserFeedback", _logFactory))
            ).As<IUserFeedbackRepository>().SingleInstance();
            
            builder.RegisterInstance(
                new PublicFeedbackRepository(AzureTableStorage<PublicFeedbackEntity>.Create(connectionString, "PublicFeedback", _logFactory))
            ).As<IPublicFeedbackRepository>().SingleInstance();

            builder.RegisterInstance(
                new BlogRepository(AzureTableStorage<BlogEntity>.Create(connectionString, "Blogs", _logFactory))
            ).As<IBlogRepository>().SingleInstance();

            builder.RegisterInstance(
                new BlogCommentsRepository(AzureTableStorage<BlogCommentEntity>.Create(connectionString, "BlogComments", _logFactory))
            ).As<IBlogCommentsRepository>().SingleInstance();

            builder.RegisterInstance(
                new BlogPictureInfoRepository(AzureTableStorage<BlogPictureInfoEntity>.Create(connectionString, "BlogPicturesInfo", _logFactory))
            ).As<IBlogPictureInfoRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectExpertsRepository(AzureTableStorage<ProjectExpertEntity>.Create(connectionString, "ProjectExperts", _logFactory))
            ).As<IProjectExpertsRepository>().SingleInstance();

            builder.RegisterInstance(
                new StreamRepository(AzureTableStorage<StreamEntity>.Create(connectionString, "ProjectStreams", _logFactory))
            ).As<IStreamRepository>().SingleInstance();

            builder.RegisterInstance(
                new StreamsIdRepository(AzureTableStorage<StreamsIdEntity>.Create(connectionString, "StreamsId", _logFactory))
            ).As<IStreamsIdRepository>().SingleInstance();

            builder.RegisterInstance(
                new TermsPageRepository(AzureTableStorage<TermsPageEntity>.Create(connectionString, "TermsPage", _logFactory))
            ).As<ITermsPageRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectCategoriesRepository()
            ).As<IProjectCategoriesRepository>().SingleInstance();

            builder.RegisterInstance(
                _settings.CurrentValue
            ).As<BaseSettings>().SingleInstance();

            builder.RegisterInstance(
                new PersonalDataService(new PersonalDataServiceClientSettings { ApiKey = personalDatAapiKey, ServiceUri = personalDataServiceUri }, _log)
            ).As<IPersonalDataService>().SingleInstance();

            builder.RegisterInstance(
                new KycProfileServiceV2Client(new KycServiceClientSettings{ApiKey = kycSettings.ApiKey, ServiceUri = kycSettings.ServiceUri }, _log)
            ).As<IKycProfileServiceV2>().SingleInstance();

            builder.RegisterEmailSenderViaAzureQueueMessageProducer(_settings.ConnectionString(x => x.LykkeStreams.Azure.MessageQueueConnString));

            builder.RegisterType<ProjectFileRepository>().As<IProjectFileRepository>();
            builder.RegisterType<BlogPictureRepository>().As<IBlogPictureRepository>();
            builder.RegisterType<ProjectWinnersService>().As<IProjectWinnersService>();
            builder.RegisterType<ExpertsService>().As<IExpertsService>();
            builder.RegisterType<BlogCategoriesRepository>().As<IBlogCategoriesRepository>();
        }
    }
}

