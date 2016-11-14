using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CompetitionPlatform.Data.AzureRepositories.Users;

namespace CompetitionPlatform.Models
{
    public class FeedbackViewModel : IUserFeedbackData
    {
        public string Email { get; set; }

        [Required]
        [Display(Name = "Your name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Your feedback")]
        public string Feedback { get; set; }

        public DateTime Created { get; set; }
    }

    public class FeedbackListViewModel
    {
        public IEnumerable<IUserFeedbackData> FeedbackList { get; set; }
    }
}
