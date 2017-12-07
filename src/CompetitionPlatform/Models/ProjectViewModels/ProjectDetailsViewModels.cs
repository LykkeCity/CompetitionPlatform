﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.AzureRepositories.Vote;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectCommentPartialViewModel : ICommentData
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string UserIdentifier { get; set; }
        public string FullName { get; set; }
        [Required]
        public string Comment { get; set; }
        public string ParentId { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public IEnumerable<ICommentData> Comments { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsAuthor { get; set; }
        public string ProjectAuthorId { get; set; }
        public Dictionary<string, bool> CommenterIsModerator { get; set; }
        public string UserAgent { get; set; }
        public bool Deleted { get; set; }
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
        public bool IsParticipant { get; set; }
        public bool HasResult { get; set; }
    }

    public class ProjectParticipateViewModel : IProjectParticipateData
    {
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string UserIdentifier { get; set; }
        public string FullName { get; set; }
        public DateTime Registered { get; set; }
        public bool Result { get; set; }
        public string UserAgent { get; set; }
    }

    public class ProjectParticipantsPartialViewModel
    {
        public string CurrentUserId { get; set; }
        public IEnumerable<IProjectParticipateData> Participants { get; set; }
        public Status Status { get; set; }
        public bool HasResult { get; set; }
    }

    public class AddResultViewModel : IProjectResultData
    {
        public string ProjectId { get; set; }
        public string ParticipantId { get; set; }
        public string ParticipantIdentifier { get; set; }

        [Required]
        [Url]
        public string Link { get; set; }

        public string ParticipantFullName { get; set; }
        public DateTime Submitted { get; set; }
        public int Score { get; set; }
        public int Votes { get; set; }
        public string UserAgent { get; set; }
    }

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
    }

    public class EditResultViewModel
    {
        public string ProjectId { get; set; }
        public string UserId { get; set; }
    }

    public class ResultVoteViewModel : IProjectResultVoteData
    {
        public string ProjectId { get; set; }
        public string ParticipantId { get; set; }
        public string VoterUserId { get; set; }
        public string UserAgent { get; set; }
        public string Type { get; set; }
    }

    public class ProjectDetailsStatusBarViewModel
    {
        public Status Status { get; set; }
        public int StatusCompletionPercent { get; set; }
        public DateTime CompetitionRegistrationDeadline { get; set; }
        public DateTime ImplementationDeadline { get; set; }
        public DateTime VotingDeadline { get; set; }
        public int ParticipantsCount { get; set; }
    }

    public class OtherProjectViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Members { get; set; }
        public double BudgetFirstPlace { get; set; }
        public double? BudgetSecondPlace { get; set; }
    }

    public class WinnerViewModel : IWinnerData
    {
        public string ProjectId { get; set; }
        public string WinnerId { get; set; }
        public string WinnerIdentifier { get; set; }
        public string FullName { get; set; }
        public int Place { get; set; }
        public string Result { get; set; }
        public int Votes { get; set; }
        public int Score { get; set; }
        public double? Budget { get; set; }
        public double WinningScore { get; set; }
    }
}
