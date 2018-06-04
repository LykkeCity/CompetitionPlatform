using CompetitionPlatform.Data.AzureRepositories.Project;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class OtherProjectViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Members { get; set; }
        public double BudgetFirstPlace { get; set; }
        public double? BudgetSecondPlace { get; set; }
        
        public static OtherProjectViewModel Create(IProjectData project)
        {
            return new OtherProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                BudgetFirstPlace = project.BudgetFirstPlace,
                BudgetSecondPlace = project.BudgetSecondPlace,
                Members = project.ParticipantsCount
            };
        }
    }
}