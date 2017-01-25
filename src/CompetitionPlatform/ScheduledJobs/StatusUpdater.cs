using System;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Helpers;
using CompetitionPlatform.Models;
using CompetitionPlatform.Services;

namespace CompetitionPlatform.ScheduledJobs
{
    public class StatusUpdater
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectWinnersService _winnersService;

        public StatusUpdater(IProjectRepository projectRepository, IProjectWinnersService winnersService)
        {
            _projectRepository = projectRepository;
            _winnersService = winnersService;
        }

        public async Task UpdateProjectStatuses()
        {
            var projects = await _projectRepository.GetProjectsAsync();

            foreach (var project in projects)
            {
                project.Status = StatusHelper.GetProjectStatusFromString(project.ProjectStatus);

                switch (project.Status)
                {
                    case Status.Draft:
                        break;
                    case Status.Initiative:
                        break;
                    case Status.Registration:
                        if (project.CompetitionRegistrationDeadline < DateTime.UtcNow.Date)
                        {
                            project.ProjectStatus = Status.Submission.ToString();
                            await _projectRepository.UpdateAsync(project);
                        }
                        break;
                    case Status.Submission:
                        if (project.ImplementationDeadline < DateTime.UtcNow.Date)
                        {
                            project.ProjectStatus = project.SkipVoting ? Status.Archive.ToString() : Status.Voting.ToString();
                            await _projectRepository.UpdateAsync(project);
                        }
                        break;
                    case Status.Voting:
                        if (project.VotingDeadline < DateTime.UtcNow.Date)
                        {
                            project.ProjectStatus = Status.Archive.ToString();
                            await _winnersService.SaveWinners(project.Id);
                            await _projectRepository.UpdateAsync(project);
                        }
                        break;
                    case Status.Archive:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
