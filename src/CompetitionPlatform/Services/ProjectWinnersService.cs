using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Models.ProjectViewModels;

namespace CompetitionPlatform.Services
{
    public class ProjectWinnersService : IProjectWinnersService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectResultRepository _resultRepository;
        private readonly IProjectWinnersRepository _winnersRepository;

        public ProjectWinnersService(IProjectRepository projectRepository, IProjectResultRepository resultRepository,
            IProjectWinnersRepository winnersRepository)
        {
            _projectRepository = projectRepository;
            _resultRepository = resultRepository;
            _winnersRepository = winnersRepository;
        }

        public async Task SaveWinners(string projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);

            var results = await _resultRepository.GetResultsAsync(projectId);

            results = results.OrderByDescending(x => x.Votes).ThenByDescending(x => x.Score);

            var resultDatas = results as IList<IProjectResultData> ?? results.ToList();

            var firstPlaceResult = resultDatas.FirstOrDefault();

            if (firstPlaceResult != null)
            {
                var firstPlaceWinner = new WinnerViewModel
                {
                    ProjectId = firstPlaceResult.ProjectId,
                    WinnerId = firstPlaceResult.ParticipantId,
                    FullName = firstPlaceResult.ParticipantFullName,
                    Result = firstPlaceResult.Link,
                    Votes = firstPlaceResult.Votes,
                    Score = firstPlaceResult.Score,
                    Place = 1,
                    Budget = project.BudgetFirstPlace
                };

                await _winnersRepository.SaveAsync(firstPlaceWinner);
            }

            if (project.BudgetSecondPlace != null)
            {
                var secondPlaceResults = resultDatas.Skip(1).Take(3);

                foreach (var result in secondPlaceResults)
                {
                    var winner = new WinnerViewModel
                    {
                        ProjectId = result.ProjectId,
                        WinnerId = result.ParticipantId,
                        FullName = result.ParticipantFullName,
                        Result = result.Link,
                        Votes = result.Votes,
                        Score = result.Score,
                        Place = 2,
                        Budget = project.BudgetSecondPlace
                    };

                    await _winnersRepository.SaveAsync(winner);
                }
            }
        }
    }
}
