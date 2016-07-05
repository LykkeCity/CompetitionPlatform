using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Models
{
    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public List<string> Categories { get; set; }
        public DateTime CompetitionRegistrationDeadline { get; set; }
        public DateTime ImplementationDeadline { get; set; }
        public DateTime VotingDeadline { get; set; }
        public List<double> Budget { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
    }
}
