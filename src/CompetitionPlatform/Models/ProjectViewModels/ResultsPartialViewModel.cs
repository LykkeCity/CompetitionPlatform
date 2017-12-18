using System;
using System.Collections.Generic;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ResultsPartialViewModel
    {
        public Status Status { get; set; }
        public IEnumerable<IProjectResultData> Results { get; set; }
        public Dictionary<string, bool> UserVotedForResults { get; set; }
        public double BudgetFirstPlace { get; set; }
        public double? BudgetSecondPlace { get; set; }
        public int ParticipantCount { get; set; }
        public int DaysOfContest { get; set; }
        public IEnumerable<IWinnerData> Winners { get; set; }
        public bool IsAdmin { get; set; }
        public bool SkipVoting { get; set; }
        public DateTime SubmissionsDeadline { get; set; }
        public Dictionary<string, string> Avatars { get; set; }
    }
}