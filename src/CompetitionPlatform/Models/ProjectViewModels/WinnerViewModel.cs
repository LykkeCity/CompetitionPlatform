using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class WinnerViewModel : IWinnerData
    {
        public string ProjectId { get; set; }
        public string WinnerId { get; set; }
        public string WinnerIdentifier { get; set; }
        public string FullName { get; set; }
        public int Place { get; set; }
        public string Result { get; set; }
        public int Votes { get; set; }
        public int Score { get; set; }
        public double? Budget { get; set; }
        public double WinningScore { get; set; }
        public string StreamsId { get; set; }

        public static WinnerViewModel Create(IProjectResultData result,
            int winningPlace, double score, double? budget)
        {
            return new WinnerViewModel
            {
                ProjectId = result.ProjectId,
                WinnerId = result.ParticipantId,
                WinnerIdentifier = result.ParticipantIdentifier,
                FullName = result.ParticipantFullName,
                Result = result.Link,
                Votes = result.Votes,
                Score = result.Score,
                Place = winningPlace,
                Budget = budget,
                WinningScore = score
            };
        }

        public static WinnerViewModel Create(IWinnerData winner)
        {
            return new WinnerViewModel
            {
                ProjectId = winner.ProjectId,
                WinnerId = winner.WinnerId,
                WinnerIdentifier = winner.WinnerIdentifier,
                FullName = winner.FullName,
                Result = winner.Result,
                Votes = winner.Votes,
                Score = winner.Score,
                Place = winner.Place,
                Budget = winner.Budget,
                WinningScore = winner.WinningScore
            };
        }
    }
}