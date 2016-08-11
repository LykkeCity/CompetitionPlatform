using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.ProjectCategory;
using CompetitionPlatform.Helpers;
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
        private readonly IProjectParticipantsRepository _projectParticipantsRepository;
        private readonly IProjectCategoriesRepository _projectCategoriesRepository;

        public ProjectController(IProjectRepository projectRepository, IProjectCommentsRepository projectCommentsRepository,
            IProjectFileRepository projectFileRepository, IProjectFileInfoRepository projectFileInfoRepository,
            IProjectParticipantsRepository projectParticipantsRepository, IProjectCategoriesRepository projectCategoriesRepository)
        {
            _projectRepository = projectRepository;
            _projectCommentsRepository = projectCommentsRepository;
            _projectFileRepository = projectFileRepository;
            _projectFileInfoRepository = projectFileInfoRepository;
            _projectParticipantsRepository = projectParticipantsRepository;
            _projectCategoriesRepository = projectCategoriesRepository;
        }

        public IActionResult Create()
        {
            var viewModel = new ProjectViewModel
            {
                ProjectCategories = _projectCategoriesRepository.GetCategories()
            };

            return View("CreateProject", viewModel);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var viewModel = await GetProjectViewModel(id);
            return View("EditProject", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProject(ProjectViewModel projectViewModel)
        {
            projectViewModel.Tags = TrimAndSerializeTags(projectViewModel.Tags);

            projectViewModel.ProjectStatus = projectViewModel.Status.ToString();

            string projectId;

            if (projectViewModel.Id == null)
            {
                var user = GetAuthenticatedUser();

                projectViewModel.AuthorId = user.Email;
                projectViewModel.AuthorFullName = user.GetFullName();

                projectViewModel.Created = DateTime.UtcNow;

                projectId = await _projectRepository.SaveAsync(projectViewModel);
            }
            else
            {
                projectViewModel.LastModified = DateTime.UtcNow;

                projectId = projectViewModel.Id;

                await _projectRepository.UpdateAsync(projectViewModel);
            }

            await SaveProjectFile(projectViewModel.File, projectId);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ProjectDetails(string id)
        {
            var viewModel = await GetProjectViewModel(id);
            return View(viewModel);
        }

        private async Task<ProjectViewModel> GetProjectViewModel(string id)
        {
            var projectCategories = _projectCategoriesRepository.GetCategories();

            var project = await _projectRepository.GetAsync(id);

            var comments = await _projectCommentsRepository.GetProjectCommentsAsync(id);

            var participants = await _projectParticipantsRepository.GetProjectParticipants(id);

            comments = comments.OrderBy(c => c.Created).Reverse().ToList();

            var commentsPartial = new ProjectCommentPartialViewModel
            {
                ProjectId = project.Id,
                Comments = comments
            };

            var participantsPartial = new ProjectParticipantsPartialViewModel
            {
                Participants = participants
            };

            var projectViewModel = new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ProjectCategories = projectCategories,
                Category = project.Category,
                Status = (Status)Enum.Parse(typeof(Status), project.ProjectStatus, true),
                BudgetFirstPlace = project.BudgetFirstPlace,
                BudgetSecondPlace = project.BudgetSecondPlace,
                VotesFor = project.VotesFor,
                VotesAgainst = project.VotesAgainst,
                Created = project.Created,
                LastModified = project.LastModified,
                CompetitionRegistrationDeadline = project.CompetitionRegistrationDeadline,
                ImplementationDeadline = project.ImplementationDeadline,
                VotingDeadline = project.VotingDeadline,
                CommentsPartial = commentsPartial,
                ParticipantsPartial = participantsPartial,
                AuthorId = project.AuthorId,
                AuthorFullName = project.AuthorFullName
            };

            if (!string.IsNullOrEmpty(project.Tags))
            {
                projectViewModel.TagsList = JsonConvert.DeserializeObject<List<string>>(project.Tags);

                StringBuilder builder = new StringBuilder();
                foreach (string tag in projectViewModel.TagsList)
                {
                    builder.Append(tag).Append(", ");
                }
                projectViewModel.Tags = builder.ToString();
            }


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

        private CompetitionPlatformUser GetAuthenticatedUser()
        {
            return ClaimsHelper.GetUser(User.Identity);
        }
    }
}