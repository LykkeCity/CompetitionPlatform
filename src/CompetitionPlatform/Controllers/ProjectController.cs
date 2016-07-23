using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Models.ProjectViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CompetitionPlatform.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectFileRepository _projectFileRepository;
        private readonly IProjectCommentsRepository _projectCommentsRepository;
        private readonly IProjectFileInfoRepository _projectFileInfoRepository;

        public ProjectController(IProjectRepository projectRepository, IProjectFileRepository projectFileRepository,
            IProjectCommentsRepository projectCommentsRepository, IProjectFileInfoRepository projectFileInfoRepository)
        {
            _projectRepository = projectRepository;
            _projectFileRepository = projectFileRepository;
            _projectCommentsRepository = projectCommentsRepository;
            _projectFileInfoRepository = projectFileInfoRepository;
        }

        public IActionResult Create()
        {
            return View("CreateProject");
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(ProjectViewModel projectViewModel)
        {
            var tags = projectViewModel.Tags.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var tagsList = new List<string>(tags);

            tagsList = tagsList.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            projectViewModel.Tags = JsonConvert.SerializeObject(tagsList);

            projectViewModel.Created = DateTime.UtcNow;

            string newProjectId = await _projectRepository.SaveAsync(projectViewModel);

            if (projectViewModel.File != null)
            {
                await _projectFileRepository.InsertProjectFile(projectViewModel.File.OpenReadStream(), newProjectId);

                var fileInfo = new ProjectFileInfoEntity
                {
                    RowKey = newProjectId,
                    ContentType = projectViewModel.File.ContentType,
                    FileName = projectViewModel.File.FileName
                };

                await _projectFileInfoRepository.SaveAsync(fileInfo);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ProjectDetails(string id)
        {
            var project = await _projectRepository.GetAsync(id);

            var comments = await _projectCommentsRepository.GetProjectCommentsAsync(id);

            var commentsPartial = new ProjectCommentPartialViewModel()
            {
                ProjectId = project.Id,
                Comments = comments
            };

            var fileInfo = await _projectFileInfoRepository.GetAsync(id);

            var fileInfoViewModel = new ProjectFileInfoViewModel
            {
                ContentType = fileInfo.ContentType,
                FileName = fileInfo.FileName
            };

            var projectViewModel = new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Categories = JsonConvert.DeserializeObject<List<string>>(project.Tags),
                Category = project.Category,
                Status = project.Status,
                BudgetFirstPlace = project.BudgetFirstPlace,
                BudgetSecondPlace = project.BudgetSecondPlace,
                BudgetThirdPlace = project.BudgetThirdPlace,
                VotesFor = project.VotesFor,
                VotesAgainst = project.VotesAgainst,
                Created = project.Created,
                CommentsPartial = commentsPartial,
                FileInfo = fileInfoViewModel
            };

            return View(projectViewModel);
        }
    }
}