using System;
using System.Collections.Generic;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectCompactViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public double BudgetFirstPlace { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
        public int CommentsCount { get; set; }
        public int ParticipantsCount { get; set; }
        public int ResultsCount { get; set; }
        public int WinnersCount { get; set; }
        public DateTime CompetitionRegistrationDeadline { get; set; }
        public DateTime ImplementationDeadline { get; set; }
        public DateTime VotingDeadline { get; set; }
        public string AuthorFullName { get; set; }
        public string AuthorId { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; }
        public bool Following { get; set; }
        public string NameTag { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime Created { get; set; }
    }
}