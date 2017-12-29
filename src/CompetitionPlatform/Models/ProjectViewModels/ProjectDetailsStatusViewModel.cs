using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectDetailsStatusViewModel
    {
        public string ProjectId { get; set; }
        public Status Status { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
        public DateTime CompetitionRegistrationDeadline { get; set; }
        public DateTime ImplementationDeadline { get; set; }
        public DateTime VotingDeadline { get; set; }
        public bool IsParticipant { get; set; }
        public bool HasResult { get; set; }
    }
}
