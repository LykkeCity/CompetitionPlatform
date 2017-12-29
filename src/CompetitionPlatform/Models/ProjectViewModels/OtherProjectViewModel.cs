namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class OtherProjectViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Members { get; set; }
        public double BudgetFirstPlace { get; set; }
        public double? BudgetSecondPlace { get; set; }
    }
}