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
    }
}