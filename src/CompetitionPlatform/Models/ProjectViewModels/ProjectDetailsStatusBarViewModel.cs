using CompetitionPlatform.Data.AzureRepositories.Project;
using System;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectDetailsStatusBarViewModel
    {
        public Status Status { get; set; }
        public int StatusCompletionPercent { get; set; }
        public DateTime CompetitionRegistrationDeadline { get; set; }
        public DateTime ImplementationDeadline { get; set; }
        public DateTime VotingDeadline { get; set; }
        public int ParticipantsCount { get; set; }

        public static ProjectDetailsStatusBarViewModel Create(IProjectData project, int participantCount)
        {
            return new ProjectDetailsStatusBarViewModel
            {
                Status = project.Status,
                ParticipantsCount = participantCount,
                CompetitionRegistrationDeadline = project.CompetitionRegistrationDeadline,
                ImplementationDeadline = project.ImplementationDeadline,
                VotingDeadline = project.VotingDeadline,
                StatusCompletionPercent = CalculateStatusCompletionPercent(project)
            };
        }


        private static int CalculateStatusCompletionPercent(IProjectData projectData)
        {
            var completion = 0;

            switch (projectData.Status)
            {
                case Status.Initiative:
                    completion = 100;
                    break;
                case Status.Registration:
                    completion = CalculateDateProgressPercent(projectData.Created,
                        projectData.CompetitionRegistrationDeadline);
                    break;
                case Status.Submission:
                    completion = CalculateDateProgressPercent(projectData.CompetitionRegistrationDeadline,
                        projectData.ImplementationDeadline);
                    break;
                case Status.Voting:
                    completion = CalculateDateProgressPercent(projectData.ImplementationDeadline,
                        projectData.VotingDeadline);
                    break;
                case Status.Archive:
                    completion = 100;
                    break;
            }
            return (completion < 0) ? 0 : completion;
        }

        private static int CalculateDateProgressPercent(DateTime start, DateTime end)
        {
            var totalDays = (end - start).Days;

            if (totalDays == 0) return 100;

            var daysPassed = (DateTime.UtcNow - start).Days;
            var percent = daysPassed * 100 / totalDays;

            return (percent > 100) ? 100 : percent;
        }

    }
}