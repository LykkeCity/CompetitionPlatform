using CompetitionPlatform.Data.AzureRepositories.Vote;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ResultVoteViewModel : IProjectResultVoteData
    {
        public string ProjectId { get; set; }
        public string ParticipantId { get; set; }
        public string VoterUserId { get; set; }
        public string UserAgent { get; set; }
        public string Type { get; set; }
    }
}