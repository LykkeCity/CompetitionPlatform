using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using Microsoft.AspNetCore.Http;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectViewModel : IProjectData
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Project name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Describe the project")]
        public string Description { get; set; }

        public Status Status { get; set; }

        public List<string> Categories { get; set; }

        public string Category { get; set; }

        public string Tags { get; set; }

        [Required]
        [Display(Name = "Competition Registration")]
        public DateTime CompetitionRegistrationDeadline { get; set; }

        [Required]
        [Display(Name = "Implementation")]
        public DateTime ImplementationDeadline { get; set; }

        [Required]
        [Display(Name = "Voting")]
        public DateTime VotingDeadline { get; set; }

        [Required]
        [Display(Name = "1ST place")]
        public double BudgetFirstPlace { get; set; }


        [Display(Name = "2ND place")]
        public double? BudgetSecondPlace { get; set; }


        [Display(Name = "3D place")]
        public double? BudgetThirdPlace { get; set; }

        public int VotesFor { get; set; }

        public int VotesAgainst { get; set; }

        public IFormFile File { set; get; }

        public DateTime Created { get; set; }

        public ProjectCommentPartialViewModel CommentsPartial { get; set; }
    }

    public class ProjectCompactViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public double BudgetFirstPlace { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
    }

    public class ProjectListIndexViewModel
    {
        public IEnumerable<ProjectCompactViewModel> Projects { get; set; }
    }
}
