using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Users;

namespace CompetitionPlatform.Models.ProjectViewModels
{
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
}
