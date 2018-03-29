using System;
using System.ComponentModel.DataAnnotations;
using CompetitionPlatform.Data.AzureRepositories.Result;

namespace CompetitionPlatform.Models.ProjectViewModels
{
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
        public string StreamsId { get; set; }
    }
}