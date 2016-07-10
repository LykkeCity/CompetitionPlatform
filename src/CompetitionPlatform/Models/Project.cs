using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;

namespace CompetitionPlatform.Models
{
    public class Project : IProjectData
    {
        public string Id { get; }

        [Required]
        [Display(Name = "Project name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Describe the project")]
        public string Description { get; set; }

        public Status Status { get; set; }

        public List<string> Categories { get; set; }

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

    }
}
