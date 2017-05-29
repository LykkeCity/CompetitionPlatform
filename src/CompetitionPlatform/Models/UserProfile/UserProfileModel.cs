using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Models.ProjectViewModels;
using System;
using System.Collections.Generic;

namespace CompetitionPlatform.Models.UserProfile
{
    public class UserProfile
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Website { get; set; }
        public string Bio { get; set; }
        public string FacebookLink { get; set; }
        public string TwitterLink { get; set; }
        public string GithubLink { get; set; }
        public bool ReceiveLykkeNewsletter { get; set; }
    }

    public class UserProfileViewModel
    {
        public UserProfile Profile { get; set; }
        public double WinningsSum { get; set; }
        public List<ProjectCompactViewModel> ParticipatedProjects { get; set; }
        public List<ProjectCompactViewModel> WonProjects { get; set; }
        public List<ProjectCompactViewModel> CreatedProjects { get; set; }
        public List<UserProfileCommentData> Comments { get; set; }
    }

    public class UserProfileCommentData
    {
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public string FullName { get; set; }
        public string Comment { get; set; }
        public DateTime LastModified { get; set; }
    }
}
