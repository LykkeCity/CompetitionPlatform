using System;

namespace CompetitionPlatform.Models
{
    public class RegistrationEmailModel
    {
        public string FirstName { get; set; }
    }

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

    public class Archive
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
        public int ParticipantCount { get; set; }
        public int ResultCount { get; set; }
        public int WinnerCount { get; set; }
        public int Duration { get; set; }
        public string FirstPlaceWinnerName { get; set; }
        public string FirstPlaceWinnerId { get; set; }
        public int FirstPlaceWinnerScore { get; set; }
        public int FirstPlaceWinnerVotes { get; set; }
        public string SecondPlaceWinnerName { get; set; }
        public string SecondPlaceWinnerId { get; set; }
        public int SecondPlaceWinnerScore { get; set; }
        public int SecondPlaceWinnerVotes { get; set; }
        public string ThirdPlaceWinnerName { get; set; }
        public string ThirdPlaceWinnerId { get; set; }
        public int ThirdPlaceWinnerScore { get; set; }
        public int ThirdPlaceWinnerVotes { get; set; }
        public string FourthPlaceWinnerName { get; set; }
        public string FourthPlaceWinnerId { get; set; }
        public int FourthPlaceWinnerScore { get; set; }
        public int FourthPlaceWinnerVotes { get; set; }
    }

    public class SlackMessage
    {
        public string Type { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
    }
}
