using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectCommentPartialViewModel : ICommentData
    {
        public string ProjectId { get; set; }
        public string User { get; set; }
        public string Comment { get; set; }

        public IEnumerable<ICommentData> Comments { get; set; }
    }
}
