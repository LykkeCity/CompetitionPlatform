using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [StringLength(5000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Describe the project")]
        public string Description { get; set; }

        public Status Status { get; set; }

        public string ProjectStatus { get; set; }

        public string Category { get; set; }

        public List<string> ProjectCategories { get; set; }

        public string Tags { get; set; }

        public List<string> TagsList { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Competition Registration")]
        public DateTime CompetitionRegistrationDeadline { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Implementation")]
        public DateTime ImplementationDeadline { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Voting")]
        public DateTime VotingDeadline { get; set; }

        [Required]
        [Display(Name = "1ST place")]
        public double BudgetFirstPlace { get; set; }

        [Display(Name = "3x2ND place")]
        public double? BudgetSecondPlace { get; set; }

        public int VotesFor { get; set; }

        public int VotesAgainst { get; set; }

        public IFormFile File { set; get; }

        public DateTime Created { get; set; }

        public DateTime LastModified { get; set; }

        public ProjectCommentPartialViewModel CommentsPartial { get; set; }

        public ProjectParticipantsPartialViewModel ParticipantsPartial { get; set; }

        public ProjectFileInfoViewModel FileInfo { get; set; }

        public string AuthorId { get; set; }

        public string AuthorFullName { get; set; }
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
        public int CommentsCount { get; set; }
        public DateTime CompetitionRegistrationDeadline { get; set; }
        public DateTime ImplementationDeadline { get; set; }
        public DateTime VotingDeadline { get; set; }
        public string AuthorFullName { get; set; }
    }

    public class ProjectFileInfoViewModel
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }

    public class ProjectListIndexViewModel
    {
        public List<string> ProjectCategories { get; set; }
        public IEnumerable<ProjectCompactViewModel> Projects { get; set; }
    }

    public class ProjectVoteViewModel
    {
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
    }

    public class ProjectEditStatusDisplayViewModel
    {
        public Status Status { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
    }
}
