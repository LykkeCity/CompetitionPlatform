using Autofac;
using AzureStorage;
using AzureStorage.Blob;
using AzureStorage.Tables;
using Common.Log;
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
using Lykke.Messages.Email;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Settings;
using Lykke.SettingsReader;

namespace CompetitionPlatform.Modules
{
    public class DbModule : Module
    {
        private readonly IReloadingManager<BaseSettings> _settings;
        private readonly ILog _log;

        public DbModule(IReloadingManager<BaseSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
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
                new ProjectRepository(AzureTableStorage<ProjectEntity>.Create(connectionString, "Projects", _log))
            ).As<IProjectRepository>().SingleInstance();


            builder.RegisterInstance(
                new UsersRepository(AzureTableStorage<UserEntity>.Create(connectionString, "Users", _log))
            ).As<IUsersRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectCommentsRepository(AzureTableStorage<CommentEntity>.Create(connectionString, "ProjectComments", _log))
            ).As<IProjectCommentsRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectFileInfoRepository(AzureTableStorage<ProjectFileInfoEntity>.Create(connectionString, "ProjectFilesInfo", _log))
            ).As<IProjectFileInfoRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectVoteRepository(AzureTableStorage<ProjectVoteEntity>.Create(connectionString, "ProjectVotes", _log))
            ).As<IProjectVoteRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectParticipantsRepository(AzureTableStorage<ProjectParticipateEntity>.Create(connectionString, "ProjectParticipants", _log))
            ).As<IProjectParticipantsRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectResultRepository(AzureTableStorage<ProjectResultEntity>.Create(connectionString, "ProjectResults", _log))
            ).As<IProjectResultRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectResultVoteRepository(AzureTableStorage<ProjectResultVoteEntity>.Create(connectionString, "ProjectResultVotes", _log))
            ).As<IProjectResultVoteRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectFollowRepository(AzureTableStorage<ProjectFollowEntity>.Create(connectionString, "ProjectFollows", _log))
            ).As<IProjectFollowRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectWinnersRepository(AzureTableStorage<WinnerEntity>.Create(connectionString, "Winners", _log))
            ).As<IProjectWinnersRepository>().SingleInstance();

            builder.RegisterInstance(
                new UserRolesRepository(AzureTableStorage<UserRoleEntity>.Create(connectionString, "UserRoles", _log))
            ).As<IUserRolesRepository>().SingleInstance();

            builder.RegisterInstance(
                new FollowMailSentRepository(AzureTableStorage<FollowMailSentEntity>.Create(connectionString, "FollowMailSent", _log))
            ).As<IFollowMailSentRepository>().SingleInstance();

            builder.RegisterInstance(
                new UserFeedbackRepository(AzureTableStorage<UserFeedbackEntity>.Create(connectionString, "UserFeedback", _log))
            ).As<IUserFeedbackRepository>().SingleInstance();

            builder.RegisterInstance(
                new BlogRepository(AzureTableStorage<BlogEntity>.Create(connectionString, "Blogs", _log))
            ).As<IBlogRepository>().SingleInstance();

            builder.RegisterInstance(
                new BlogCommentsRepository(AzureTableStorage<BlogCommentEntity>.Create(connectionString, "BlogComments", _log))
            ).As<IBlogCommentsRepository>().SingleInstance();

            builder.RegisterInstance(
                new BlogPictureInfoRepository(AzureTableStorage<BlogPictureInfoEntity>.Create(connectionString, "BlogPicturesInfo", _log))
            ).As<IBlogPictureInfoRepository>().SingleInstance();

            builder.RegisterInstance(
                new ProjectExpertsRepository(AzureTableStorage<ProjectExpertEntity>.Create(connectionString, "ProjectExperts", _log))
            ).As<IProjectExpertsRepository>().SingleInstance();

            builder.RegisterInstance(
                new StreamRepository(AzureTableStorage<StreamEntity>.Create(connectionString, "ProjectStreams", _log))
            ).As<IStreamRepository>().SingleInstance();

            builder.RegisterInstance(
                new StreamsIdRepository(AzureTableStorage<StreamsIdEntity>.Create(connectionString, "StreamsId", _log))
            ).As<IStreamsIdRepository>().SingleInstance();

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
            ).As<IKycProfileServiceV2>().SingleInstance();;

            builder.RegisterEmailSenderViaAzureQueueMessageProducer(_settings.ConnectionString(x => x.LykkeStreams.Azure.MessageQueueConnString));

            builder.RegisterType<ProjectFileRepository>().As<IProjectFileRepository>();
            builder.RegisterType<BlogPictureRepository>().As<IBlogPictureRepository>();
            builder.RegisterType<ProjectWinnersService>().As<IProjectWinnersService>();
            builder.RegisterType<ExpertsService>().As<IExpertsService>();
            builder.RegisterType<BlogCategoriesRepository>().As<IBlogCategoriesRepository>();
        }
    }
}

