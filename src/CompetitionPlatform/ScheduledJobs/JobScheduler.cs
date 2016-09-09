using Common.Log;
using Quartz;
using Quartz.Impl;

namespace CompetitionPlatform.ScheduledJobs
{
    public static class JobScheduler
    {
        public static async void Start(string connectionString, ILog log)
        {
            var schedFact = new StdSchedulerFactory();

            var sched = await schedFact.GetScheduler();
            await sched.Start();

            var job = JobBuilder.Create<ProjectStatusIpdaterJob>()
                .WithIdentity("myJob", "group1")
                .Build();

            var trigger = TriggerBuilder.Create()
              .WithIdentity("myTrigger", "group1")
              .WithSimpleSchedule(x => x
                  .WithIntervalInHours(1)
                  .RepeatForever())
              .Build();

            job.JobDataMap["connectionString"] = connectionString;
            job.JobDataMap["log"] = log;

            await sched.ScheduleJob(job, trigger);
        }
    }
}
