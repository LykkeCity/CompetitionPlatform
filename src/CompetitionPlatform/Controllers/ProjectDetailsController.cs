using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Models.ProjectViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CompetitionPlatform.Controllers
{
    public class ProjectDetailsController : Controller
    {
        private readonly IProjectCommentsRepository _projectCommentsRepository;

        public ProjectDetailsController(IProjectCommentsRepository projectCommentsRepository)
        {
            _projectCommentsRepository = projectCommentsRepository;
        }

        public IActionResult AddComment(ProjectCommentPartialViewModel model)
        {
            model.User = "User1";
            model.Created = DateTime.UtcNow;
            model.LastModified = model.Created;

            _projectCommentsRepository.SaveAsync(model);
            return RedirectToAction("ProjectDetails", "Project", new { id = model.ProjectId });
        }
    }
}