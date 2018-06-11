using CompetitionPlatform.Data.AzureRepositories.Admin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Models.ProjectViewModels
{
    public class TermsPageViewModel : ITermsPageData
    {
        [Required]
        public string ProjectId { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Add custom Terms and Conditions Page for current project")]
        public string Content { get; set; }

        public static TermsPageViewModel Create(ITermsPageData data)
        {
            return new TermsPageViewModel
            {
                ProjectId = data.ProjectId,
                Content = data.Content
            };
        }

        public static TermsPageViewModel Create(string projectId)
        {
            return new TermsPageViewModel
            {
                ProjectId = projectId,
                Content = ""
            };
        }
    }
}
