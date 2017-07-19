using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Blog;

namespace CompetitionPlatform.Models.BlogViewModels
{
    public class BlogViewModel : IBlogData
    {
        [Required]
        [Display(Name = "Blog Entry Url")]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Overview { get; set; }
        [Required]
        public string Text { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public double ProjectBudget { get; set; }
        public string ProjectCategory { get; set; }
        public string ProjectAuthor { get; set; }
        public int ProjectParticipants { get; set; }
        public int ProjectResults { get; set; }
        public int ProjectWinners { get; set; }
        public string FirstResult { get; set; }
        public string FirstResultAuthor { get; set; }
        public string FirstResultComment { get; set; }
        public string SecondResult { get; set; }
        public string SecondResultAuthor { get; set; }
        public string SecondResultComment { get; set; }
        public string ThirdResult { get; set; }
        public string ThirdResultAuthor { get; set; }
        public string ThirdResultComment { get; set; }
        public string FourthResult { get; set; }
        public string FourthResultAuthor { get; set; }
        public string FourthResultComment { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        [Required]
        public string Category { get; set; }
        public DateTime Posted { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Publish Date")]
        public DateTime Published { get; set; }
        public BlogCommentPartialViewModel CommentsPartial { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class BlogCompactViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public string Category { get; set; }
        public DateTime Published { get; set; }
        public int CommentsCount { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
    }

    public class BlogListIndexViewModel
    {
        public List<string> BlogCategories { get; set; }
        public IEnumerable<BlogCompactViewModel> BlogEntries { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class BlogCommentPartialViewModel : IBlogCommentData
    {
        public string Id { get; set; }
        public string BlogId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        [Required]
        public string Comment { get; set; }
        public string ParentId { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public IEnumerable<IBlogCommentData> Comments { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsAuthor { get; set; }
        public string BlogAuthorId { get; set; }
        public Dictionary<string, bool> CommenterIsModerator { get; set; }
        public string UserAgent { get; set; }
        public bool Deleted { get; set; }
    }
}
