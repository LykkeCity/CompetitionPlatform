using System.Collections.Generic;
using CompetitionPlatform.Data.AzureRepositories.Users;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectParticipantsPartialViewModel
    {
        public string CurrentUserId { get; set; }
        public IEnumerable<IProjectParticipateData> Participants { get; set; }
        public Status Status { get; set; }
        public bool HasResult { get; set; }
    }
}