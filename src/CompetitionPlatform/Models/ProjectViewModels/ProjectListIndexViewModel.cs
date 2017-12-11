using System.Collections.Generic;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectListIndexViewModel
    {
        public List<string> ProjectCategories { get; set; }
        public IEnumerable<ProjectCompactViewModel> Projects { get; set; }
        public List<LatestWinner> LatestWinners { get; set; }
        public List<JustFinishedProject> JustFinishedProjects { get; set; }
    }
}