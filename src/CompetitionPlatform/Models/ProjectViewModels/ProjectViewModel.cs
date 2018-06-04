using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CompetitionPlatform.Data.AzureRepositories.Expert;
using CompetitionPlatform.Data.AzureRepositories.Project;
using Microsoft.AspNetCore.Http;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectViewModel : IProjectData
    {
        [Required]
        [Display(Name = "Project Url")]
        public string Id { get; set; }

        [Display(Name = "Project Url")]
        public string ProjectUrl { get; set; }

        [Required]
        [Display(Name = "Project name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Summary")]
        public string Overview { get; set; }

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

        [Display(Name = "Repository Name")]
        public string ProgrammingResourceName { get; set; }

        [Display(Name = "Link")]
        [Url]
        public string ProgrammingResourceLink { get; set; }

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

        [Required]
        [Display(Name = "Author Email")]
        public string AuthorId { get; set; }
        public string AuthorIdentifier { get; set; }

        [Required]
        [Display(Name = "Author Full Name")]
        public string AuthorFullName { get; set; }
        public ProjectCommentPartialViewModel CommentsPartial { get; set; }
        public ProjectParticipantsPartialViewModel ParticipantsPartial { get; set; }
        public int ParticipantsCount { get; set; }
        public ResultsPartialViewModel ResultsPartial { get; set; }
        public ProjectDetailsStatusBarViewModel StatusBarPartial { get; set; }
        public ProjectFileInfoViewModel FileInfo { get; set; }
        public bool IsParticipant { get; set; }
        public string ParticipantId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsAuthor { get; set; }
        public bool IsFollowing { get; set; }
        public List<OtherProjectViewModel> OtherProjects { get; set; }
        public string UserAgent { get; set; }
        public bool SkipVoting { get; set; }
        public bool SkipRegistration { get; set; }
        public IEnumerable<IProjectExpertData> ProjectExperts { get; set; }

        [Display(Name = "Prize Description")]
        public string PrizeDescription { get; set; }

        public string StreamId { get; set; }

        public string StreamType { get; set; }

        public List<StreamProject> StreamProjects { get; set; }
        public List<StreamProject> AllStreamProjects { get; set; }
        public List<CompactStream> CompactStreams { get; set; }
        public EditStreamProjects EditStreamProjects { get; set; }
        public string NewStreamName { get; set; }
        public string ExistingStreamId { get; set; }
        public string SerializedStream { get; set; }

        [Display(Name = "Title Note")]
        [StringLength(30)]
        public string NameTag { get; set; }

        public string AuthorAvatarUrl { get; set; }
        public string AuthorStreamsId { get; set; }

        public List<WinnerViewModel> Winners { get; set; }
        
        public List<ExpertViewModel> Experts { get; set; }

        [StringLength(5000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "This text will be visible only for users with KYC status OK")]
        public string InfoForKycUsers { get; set; }

        [StringLength(5000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description footer")]
        public string DescriptionFooter { get; set; }
    }
}