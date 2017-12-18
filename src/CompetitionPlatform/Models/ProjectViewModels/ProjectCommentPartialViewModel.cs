using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CompetitionPlatform.Data.AzureRepositories.Project;

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
        public Dictionary<string, string> Avatars { get; set; }
    }
}