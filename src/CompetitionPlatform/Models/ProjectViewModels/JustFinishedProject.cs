namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class JustFinishedProject
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public double Amount { get; set; }
        public int NumberOfWinners { get; set; }
    }
}