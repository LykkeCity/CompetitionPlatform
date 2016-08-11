using System;
using System.Collections.Generic;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Users;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectCommentPartialViewModel : ICommentData
    {
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public IEnumerable<ICommentData> Comments { get; set; }
    }

    public class ProjectDetailsStatusViewModel
    {
        public string ProjectId { get; set; }
        public Status Status { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
        public DateTime CompetitionRegistrationDeadline { get; set; }
        public DateTime ImplementationDeadline { get; set; }
        public DateTime VotingDeadline { get; set; }
    }

    public class ProjectParticipateViewModel : IProjectParticipateData
    {
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public DateTime Registered { get; set; }
        public bool Result { get; set; }
    }

    public class ProjectParticipantsPartialViewModel
    {
        public IEnumerable<IProjectParticipateData> Participants { get; set; }
    }
}
