using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Models
{
    public class Project
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public Status Status { get; set; }
        public List<string> Categories { get; set; }
        [Display(Name = "Registration Deadline")]
        public DateTime CompetitionRegistrationDeadline { get; set; }
        [Display(Name = "Implementation Deadline")]
        public DateTime ImplementationDeadline { get; set; }
        [Display(Name = "Voting Deadline")]
        public DateTime VotingDeadline { get; set; }
        public List<double> Budget { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
    }
}
