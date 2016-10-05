using System;

namespace CompetitionPlatform.Models
{
    public class Initiative
    {
        public string FirstName { get; set; }
        public string ProjectId { get; set; }
        public DateTime ProjectCreatedDate { get; set; }
        public string ProjectAuthorName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectStatus { get; set; }
        public string ProjectDescription { get; set; }
        public double ProjectFirstPrize { get; set; }
        public double ProjectSecondPrize { get; set; }
    }

    public class Competition
    {
        public string FirstName { get; set; }
        public string ProjectId { get; set; }
        public DateTime ProjectCreatedDate { get; set; }
        public DateTime ProjectCompetitionDeadline { get; set; }
        public string ProjectAuthorName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectStatus { get; set; }
        public string ProjectDescription { get; set; }
        public double ProjectFirstPrize { get; set; }
        public double ProjectSecondPrize { get; set; }
    }

    public class Implementation
    {
        public string FirstName { get; set; }
        public string ProjectId { get; set; }
        public DateTime ProjectCreatedDate { get; set; }
        public DateTime ProjectImplementationDeadline { get; set; }
        public string ProjectAuthorName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectStatus { get; set; }
        public string ProjectDescription { get; set; }
        public double ProjectFirstPrize { get; set; }
        public double ProjectSecondPrize { get; set; }
    }

    public class Voting
    {
        public string FirstName { get; set; }
        public string ProjectId { get; set; }
        public DateTime ProjectCreatedDate { get; set; }
        public DateTime ProjectVotingDeadline { get; set; }
        public string ProjectAuthorName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectStatus { get; set; }
        public string ProjectDescription { get; set; }
        public double ProjectFirstPrize { get; set; }
        public double ProjectSecondPrize { get; set; }
    }
}
