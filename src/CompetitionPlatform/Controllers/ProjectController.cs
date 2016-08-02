using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Models;
using CompetitionPlatform.Models.ProjectViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CompetitionPlatform.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectCommentsRepository _projectCommentsRepository;
        private readonly IProjectFileRepository _projectFileRepository;
        private readonly IProjectFileInfoRepository _projectFileInfoRepository;

        public ProjectController(IProjectRepository projectRepository, IProjectCommentsRepository projectCommentsRepository,
            IProjectFileRepository projectFileRepository, IProjectFileInfoRepository projectFileInfoRepository)
        {
            _projectRepository = projectRepository;
            _projectCommentsRepository = projectCommentsRepository;
            _projectFileRepository = projectFileRepository;
            _projectFileInfoRepository = projectFileInfoRepository;
        }

        public IActionResult Create()
        {
            return View("CreateProject");
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(ProjectViewModel projectViewModel)
        {
            projectViewModel.Tags = TrimAndSerializeTags(projectViewModel.Tags);

            projectViewModel.Created = DateTime.UtcNow;

            projectViewModel.ProjectStatus = projectViewModel.Status.ToString();

            var newProjectId = await _projectRepository.SaveAsync(projectViewModel);

            await SaveProjectFile(projectViewModel.File, newProjectId);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ProjectDetails(string id)
        {
            var viewModel = await GetProjectViewModel(id);
            return View(viewModel);
        }

        private async Task<ProjectViewModel> GetProjectViewModel(string id)
        {
            var project = await _projectRepository.GetAsync(id);

            var comments = await _projectCommentsRepository.GetProjectCommentsAsync(id);

            comments = comments.OrderBy(c => c.Created).Reverse().ToList();

            var commentsPartial = new ProjectCommentPartialViewModel
            {
                ProjectId = project.Id,
                Comments = comments
            };

            var projectViewModel = new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Category = project.Category,
                Status = (Status)Enum.Parse(typeof(Status), project.ProjectStatus, true),
                BudgetFirstPlace = project.BudgetFirstPlace,
                BudgetSecondPlace = project.BudgetSecondPlace,
                VotesFor = project.VotesFor,
                VotesAgainst = project.VotesAgainst,
                Created = project.Created,
                CompetitionRegistrationDeadline = project.CompetitionRegistrationDeadline,
                ImplementationDeadline = project.ImplementationDeadline,
                VotingDeadline = project.VotingDeadline,
                CommentsPartial = commentsPartial
            };

            if (!string.IsNullOrEmpty(project.Tags))
                projectViewModel.Categories = JsonConvert.DeserializeObject<List<string>>(project.Tags);

            var fileInfo = await _projectFileInfoRepository.GetAsync(id);

            if (fileInfo != null)
            {
                var fileInfoViewModel = new ProjectFileInfoViewModel
                {
                    ContentType = fileInfo.ContentType,
                    FileName = fileInfo.FileName
                };

                projectViewModel.FileInfo = fileInfoViewModel;
            }

            return projectViewModel;
        }

        private string TrimAndSerializeTags(string tagsString)
        {
            if (string.IsNullOrEmpty(tagsString))
                return null;

            var tags = tagsString.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var tagsList = new List<string>(tags);

            tagsList = tagsList.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            tagsList = tagsList.Select(s => s.Trim()).ToList();

            return JsonConvert.SerializeObject(tagsList);
        }

        private async Task SaveProjectFile(IFormFile file, string projectId)
        {
            if (file != null)
            {
                await _projectFileRepository.InsertProjectFile(file.OpenReadStream(), projectId);

                var fileInfo = new ProjectFileInfoEntity
                {
                    RowKey = projectId,
                    ContentType = file.ContentType,
                    FileName = file.FileName
                };

                await _projectFileInfoRepository.SaveAsync(fileInfo);
            }
        }
    }
}