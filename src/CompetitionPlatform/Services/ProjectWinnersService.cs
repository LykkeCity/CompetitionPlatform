using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.AzureRepositories.Vote;
using CompetitionPlatform.Models.ProjectViewModels;

namespace CompetitionPlatform.Services
{
    public class ProjectWinnersService : IProjectWinnersService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectResultRepository _resultRepository;
        private readonly IProjectWinnersRepository _winnersRepository;
        private readonly IProjectResultVoteRepository _resultVoteRepository;

        private const double AuthorVoteValue = 0.1;
        private const double CitizensVoteValue = 0.2;
        private const double AdminsVoteValue = 0.3;

        public ProjectWinnersService(IProjectRepository projectRepository, IProjectResultRepository resultRepository,
            IProjectWinnersRepository winnersRepository, IProjectResultVoteRepository resultVoteRepository)
        {
            _projectRepository = projectRepository;
            _resultRepository = resultRepository;
            _winnersRepository = winnersRepository;
            _resultVoteRepository = resultVoteRepository;
        }

        public async Task SaveWinners(string projectId, IEnumerable<WinnerViewModel> winners = null)
        {
            var project = await _projectRepository.GetAsync(projectId);

            var results = await _resultRepository.GetResultsAsync(projectId);
            var projectResultDatas = results as IList<IProjectResultData> ?? results.ToList();

            var votes = await _resultVoteRepository.GetProjectResultVotesAsync(projectId);
            var projectResultVoteDatas = votes as IList<IProjectResultVoteData> ?? votes.ToList();

            var resultScores = CalculateScores(projectResultDatas, projectResultVoteDatas);

            if (winners != null && winners.Any(x => !string.IsNullOrEmpty(x.WinnerId)))
            {
                //await SaveCustomWinners(projectId, winners);
                foreach (var winner in winners.Where(x => !string.IsNullOrEmpty(x.WinnerId)))
                {
                    var winnerResult = projectResultDatas.FirstOrDefault(x => x.ParticipantId == winner.WinnerId);
                    var winnerScore = resultScores.First(x => x.Key == winner.WinnerId);
                    var budget = winner.Budget;
                    if (budget == null)
                    {
                        budget = winner.Place == 1 ? project.BudgetFirstPlace : project.BudgetSecondPlace;
                    }

                    var winnerModel = WinnerViewModel.Create(winnerResult, winner.Place, winnerScore.Value, budget);
                    await _winnersRepository.SaveAsync(winnerModel);
                }

                return;
            }

            var firstPlaceWinner = resultScores.OrderByDescending(pair => pair.Value).Take(1).FirstOrDefault();
            var firstPlaceResult = projectResultDatas.FirstOrDefault(x => x.ParticipantId == firstPlaceWinner.Key);

            if (firstPlaceResult != null)
            {
                var firstWinner = WinnerViewModel.Create(firstPlaceResult, 1, firstPlaceWinner.Value, project.BudgetFirstPlace);

                if (await WinnerIsEligible(firstWinner.ProjectId, firstWinner.WinnerId))
                    await _winnersRepository.SaveAsync(firstWinner);
            }

            var secondPlaceWinners = resultScores.OrderByDescending(pair => pair.Value).Skip(1).Take(3);
            if (project.BudgetSecondPlace != null)
            {
                foreach (var winner in secondPlaceWinners)
                {
                    var secondPlaceResult = projectResultDatas.FirstOrDefault(x => x.ParticipantId == winner.Key);
                    var secondWinner = WinnerViewModel.Create(secondPlaceResult, 2, winner.Value, project.BudgetSecondPlace);

                    if (await WinnerIsEligible(secondWinner.ProjectId, secondWinner.WinnerId))
                        await _winnersRepository.SaveAsync(secondWinner);
                }
            }
        }

        private static Dictionary<string, double> CalculateScores(IList<IProjectResultData> projectResultDatas, IList<IProjectResultVoteData> projectResultVoteDatas)
        {
            var resultScores = new Dictionary<string, double>();

            var adminVotes = projectResultVoteDatas.Where(x => x.Type == "ADMIN");
            var authorVotes = projectResultVoteDatas.Where(x => x.Type == "AUTHOR");

            var totalVotesCount = projectResultVoteDatas.Count;
            var resultVoteDatas = adminVotes as IList<IProjectResultVoteData> ?? adminVotes.ToList();

            var adminVotesCount = resultVoteDatas.Count;
            var voteDatas = authorVotes as IList<IProjectResultVoteData> ?? authorVotes.ToList();
            var authorVotesCount = voteDatas.Count;

            var userVotes = totalVotesCount - adminVotesCount - authorVotesCount;
            foreach (var result in projectResultDatas)
            {
                double resultScore = 0;
                double adminVotesScore = 0;
                double userVotesScore = 0;

                var resultAdminVotes = resultVoteDatas.Where(x => x.ParticipantId == result.ParticipantId);
                var resultVotes = projectResultVoteDatas.Where(x => x.ParticipantId == result.ParticipantId && x.Type != "ADMIN" && x.Type != "AUTHOR");
                var authorVote = voteDatas.FirstOrDefault(x => x.ParticipantId == result.ParticipantId);

                var resultAdminVotesCount = Convert.ToDouble(resultAdminVotes.Count());
                var resultVotesCount = Convert.ToDouble(resultVotes.Count());

                if (adminVotesCount != 0)
                    adminVotesScore = (resultAdminVotesCount / Convert.ToDouble(adminVotesCount)) * AdminsVoteValue;

                if (userVotes != 0)
                    userVotesScore = (resultVotesCount / Convert.ToDouble(userVotes)) * CitizensVoteValue;

                resultScore = adminVotesScore + userVotesScore;

                if (authorVote != null)
                    resultScore += AuthorVoteValue;

                resultScores.Add(result.ParticipantId, resultScore);
            }

            return resultScores;
        }

        private async Task<bool> WinnerIsEligible(string projectId, string participantId)
        {
            var votes = await _resultVoteRepository.GetProjectResultVotesAsync(projectId);
            var projectResultVoteDatas = votes as IList<IProjectResultVoteData> ?? votes.ToList();

            var adminVotes = projectResultVoteDatas.Where(x => x.Type == "ADMIN" && x.ParticipantId == participantId);
            var authorVotes = projectResultVoteDatas.Where(x => x.Type == "AUTHOR" && x.ParticipantId == participantId);

            return adminVotes.Any() && authorVotes.Any();
        }
        
        public async Task SaveCustomWinners(string projectId, IEnumerable<WinnerViewModel> winners)
        {
            var project = await _projectRepository.GetAsync(projectId);
            var results = await _resultRepository.GetResultsAsync(projectId);

            foreach (var winner in winners.Where(x => !string.IsNullOrEmpty(x.WinnerId)))
            {
                var winnerResult = results.FirstOrDefault(x => x.ParticipantId == winner.WinnerId);

                var budget = winner.Budget;
                if (budget == null)
                {
                    budget = winner.Place == 1 ? project.BudgetFirstPlace : project.BudgetSecondPlace;
                }

                var winnerModel = WinnerViewModel.Create(winnerResult, winner.Place, 0, budget);
                await _winnersRepository.SaveAsync(winnerModel);
            }
        }
    }
}
