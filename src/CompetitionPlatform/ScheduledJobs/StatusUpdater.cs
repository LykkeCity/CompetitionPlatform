using System;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
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
                project.Status = (Status)Enum.Parse(typeof(Status), project.ProjectStatus, true);

                switch (project.Status)
                {
                    case Status.Initiative:
                        break;
                    case Status.Registration:
                        if (project.CompetitionRegistrationDeadline < DateTime.Today)
                        {
                            project.ProjectStatus = Status.Implementation.ToString();
                            await _projectRepository.UpdateAsync(project);
                        }
                        break;
                    case Status.Implementation:
                        if (project.ImplementationDeadline < DateTime.Today)
                        {
                            project.ProjectStatus = Status.Voting.ToString();
                            await _projectRepository.UpdateAsync(project);
                        }
                        break;
                    case Status.Voting:
                        if (project.VotingDeadline < DateTime.Today)
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
