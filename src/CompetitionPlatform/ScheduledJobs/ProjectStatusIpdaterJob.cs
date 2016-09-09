using System.Threading.Tasks;
using AzureStorage.Tables;
using Common.Log;
using CompetitionPlatform.Data.AzureRepositories.Project;
using Quartz;

namespace CompetitionPlatform.ScheduledJobs
{
    public class ProjectStatusIpdaterJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;

            var connectionString = dataMap.GetString("connectionString");
            var log = (ILog)dataMap["log"];

            var projectRepository = new ProjectRepository(new AzureTableStorage<ProjectEntity>(connectionString, "Projects", log));

            var statusUpdater = new StatusUpdater(projectRepository);

            await statusUpdater.UpdateProjectStatuses();
        }
    }
}
