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

        public async Task SaveWinners(string projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);

            var results = await _resultRepository.GetResultsAsync(projectId);

            var votes = await _resultVoteRepository.GetProjectResultVotesAsync(projectId);
            var projectResultVoteDatas = votes as IList<IProjectResultVoteData> ?? votes.ToList();

            var adminVotes = projectResultVoteDatas.Where(x => x.Type == "ADMIN");
            var authorVotes = projectResultVoteDatas.Where(x => x.Type == "AUTHOR");

            var totalVotesCount = projectResultVoteDatas.Count;
            var resultVoteDatas = adminVotes as IList<IProjectResultVoteData> ?? adminVotes.ToList();

            var adminVotesCount = resultVoteDatas.Count;
            var voteDatas = authorVotes as IList<IProjectResultVoteData> ?? authorVotes.ToList();

            var authorVotesCount = voteDatas.Count;

            var userVotes = totalVotesCount - adminVotesCount - authorVotesCount;

            var resultScores = new Dictionary<string, double>();

            var projectResultDatas = results as IList<IProjectResultData> ?? results.ToList();

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

            var firstPlaceWinner = resultScores.OrderByDescending(pair => pair.Value).Take(1).FirstOrDefault();

            var secondPlaceWinners = resultScores.OrderByDescending(pair => pair.Value).Skip(1).Take(3);

            var firstPlaceResult = projectResultDatas.FirstOrDefault(x => x.ParticipantId == firstPlaceWinner.Key);

            if (firstPlaceResult != null)
            {
                var firstWinner = new WinnerViewModel
                {
                    ProjectId = firstPlaceResult.ProjectId,
                    WinnerId = firstPlaceResult.ParticipantId,
                    FullName = firstPlaceResult.ParticipantFullName,
                    Result = firstPlaceResult.Link,
                    Votes = firstPlaceResult.Votes,
                    Score = firstPlaceResult.Score,
                    Place = 1,
                    Budget = project.BudgetFirstPlace,
                    WinningScore = firstPlaceWinner.Value
                };

                if (await WinnerIsEligible(firstWinner.ProjectId, firstWinner.WinnerId))
                    await _winnersRepository.SaveAsync(firstWinner);
            }

            if (project.BudgetSecondPlace != null)
            {
                foreach (var winner in secondPlaceWinners)
                {
                    var secondPlaceResult = projectResultDatas.FirstOrDefault(x => x.ParticipantId == winner.Key);

                    var secondWinner = new WinnerViewModel
                    {
                        ProjectId = secondPlaceResult.ProjectId,
                        WinnerId = secondPlaceResult.ParticipantId,
                        FullName = secondPlaceResult.ParticipantFullName,
                        Result = secondPlaceResult.Link,
                        Votes = secondPlaceResult.Votes,
                        Score = secondPlaceResult.Score,
                        Place = 2,
                        Budget = project.BudgetSecondPlace,
                        WinningScore = winner.Value
                    };

                    if (await WinnerIsEligible(secondWinner.ProjectId, secondWinner.WinnerId))
                        await _winnersRepository.SaveAsync(secondWinner);
                }
            }
        }

        private async Task<bool> WinnerIsEligible(string projectId, string participantId)
        {
            var votes = await _resultVoteRepository.GetProjectResultVotesAsync(projectId);
            var projectResultVoteDatas = votes as IList<IProjectResultVoteData> ?? votes.ToList();

            var adminVotes = projectResultVoteDatas.Where(x => x.Type == "ADMIN" && x.ParticipantId == participantId);
            var authorVotes = projectResultVoteDatas.Where(x => x.Type == "AUTHOR" && x.ParticipantId == participantId);

            return adminVotes.Any() && authorVotes.Any();
        }
    }
}
