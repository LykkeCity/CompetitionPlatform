using System;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectDetailsStatusBarViewModel
    {
        public Status Status { get; set; }
        public int StatusCompletionPercent { get; set; }
        public DateTime CompetitionRegistrationDeadline { get; set; }
        public DateTime ImplementationDeadline { get; set; }
        public DateTime VotingDeadline { get; set; }
        public int ParticipantsCount { get; set; }
    }
}