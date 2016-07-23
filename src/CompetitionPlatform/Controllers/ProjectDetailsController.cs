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
        private readonly IProjectFileRepository _projectFileRepository;
        private readonly IProjectFileInfoRepository _projectFileInfoRepository;

        public ProjectDetailsController(IProjectCommentsRepository projectCommentsRepository, IProjectFileRepository projectFileRepository,
            IProjectFileInfoRepository projectFileInfoRepository)
        {
            _projectCommentsRepository = projectCommentsRepository;
            _projectFileRepository = projectFileRepository;
            _projectFileInfoRepository = projectFileInfoRepository;
        }

        public IActionResult AddComment(ProjectCommentPartialViewModel model)
        {
            model.User = "User1";
            model.Created = DateTime.UtcNow;
            model.LastModified = model.Created;

            _projectCommentsRepository.SaveAsync(model);
            return RedirectToAction("ProjectDetails", "Project", new { id = model.ProjectId });
        }

        public async Task<IActionResult> DownloadProjectFile(string id)
        {
            var fileInfo = await _projectFileInfoRepository.GetAsync(id);

            var fileStream = await _projectFileRepository.GetProjectFile(id);
            return File(fileStream, fileInfo.ContentType, fileInfo.FileName);
        }
    }
}