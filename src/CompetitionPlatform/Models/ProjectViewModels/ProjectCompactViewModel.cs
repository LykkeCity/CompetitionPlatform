using System;
using System.Collections.Generic;
using CompetitionPlatform.Data.AzureRepositories.Project;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class ProjectCompactViewModel
    {
        public int CommentsCount { get; set; }
        public int ResultsCount { get; set; }
        public int WinnersCount { get; set; }
        public int ParticipantsCount { get; set; }
        public List<string> Tags { get; set; }
        public bool IsFollowing { get; set; }
        public IProjectData BaseProjectData { get; set; }
        public string AuthorAvatarUrl { get; set; }
    }
}