using System.Threading.Tasks;
using AzureStorage.Tables;
using Common.Log;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.AzureRepositories.Vote;
using CompetitionPlatform.Services;
using Lykke.SettingsReader;
using Quartz;

namespace CompetitionPlatform.ScheduledJobs
{
    public class ProjectStatusUpdaterJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;

            var connectionString = (IReloadingManager<string>)dataMap["connectionString"];
            var log = (ILog)dataMap["log"];

            var projectRepository =
                new ProjectRepository(AzureTableStorage<ProjectEntity>.Create(connectionString, "Projects", log));

            var resultRepository =
                new ProjectResultRepository(AzureTableStorage<ProjectResultEntity>.Create(connectionString, "ProjectResults", log));

            var winnersRepository =
                new ProjectWinnersRepository(AzureTableStorage<WinnerEntity>.Create(connectionString, "Winners", log));

            var resultVotesRepository =
                new ProjectResultVoteRepository(AzureTableStorage<ProjectResultVoteEntity>.Create(connectionString, "ProjectResultVotes", log));

            var winnersService = new ProjectWinnersService(projectRepository, resultRepository, winnersRepository, resultVotesRepository);

            var statusUpdater = new StatusUpdater(projectRepository, winnersService);

            await statusUpdater.UpdateProjectStatuses();
        }
    }
}
