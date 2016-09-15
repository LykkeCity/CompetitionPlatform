using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.ProjectCategory;
using CompetitionPlatform.Helpers;
using CompetitionPlatform.Models;
using CompetitionPlatform.Models.ProjectViewModels;
using CompetitionPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CompetitionPlatform.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectCommentsRepository _commentsRepository;
        private readonly IProjectFileRepository _fileRepository;
        private readonly IProjectFileInfoRepository _fileInfoRepository;
        private readonly IProjectParticipantsRepository _participantsRepository;
        private readonly IProjectCategoriesRepository _categoriesRepository;
        private readonly IProjectResultRepository _resultRepository;
        private readonly IProjectFollowRepository _projectFollowRepository;
        private readonly IProjectWinnersRepository _winnersRepository;
        private readonly IUserRolesRepository _userRolesRepository;
        private readonly IProjectWinnersService _winnersService;

        public ProjectController(IProjectRepository projectRepository, IProjectCommentsRepository commentsRepository,
            IProjectFileRepository fileRepository, IProjectFileInfoRepository fileInfoRepository,
            IProjectParticipantsRepository participantsRepository, IProjectCategoriesRepository categoriesRepository,
            IProjectResultRepository resultRepository, IProjectFollowRepository projectFollowRepository,
            IProjectWinnersRepository winnersRepository, IUserRolesRepository userRolesRepository,
            IProjectWinnersService winnersService)
        {
            _projectRepository = projectRepository;
            _commentsRepository = commentsRepository;
            _fileRepository = fileRepository;
            _fileInfoRepository = fileInfoRepository;
            _participantsRepository = participantsRepository;
            _categoriesRepository = categoriesRepository;
            _resultRepository = resultRepository;
            _projectFollowRepository = projectFollowRepository;
            _winnersRepository = winnersRepository;
            _userRolesRepository = userRolesRepository;
            _winnersService = winnersService;
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var user = GetAuthenticatedUser();
            var userRole = await _userRolesRepository.GetAsync(user.Email);
            ViewBag.ProjectCategories = _categoriesRepository.GetCategories();

            if (userRole != null)
            {
                return View("CreateProject");
            }

            if (user.Documents.Contains("Selfie") && user.Documents.Contains("IdCard"))
            {
                return View("CreateProject");
            }

            return View("AccessDenied");
        }

        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            var user = GetAuthenticatedUser();

            var viewModel = await GetProjectViewModel(id);

            if (viewModel.IsAdmin)
            {
                return View("EditProject", viewModel);
            }
            if (viewModel.Status == Status.Initiative && viewModel.AuthorId == user.Email)
            {
                return View("EditProject", viewModel);
            }

            return View("AccessDenied");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveProject(ProjectViewModel projectViewModel)
        {
            projectViewModel.Tags = SerializeTags(projectViewModel.Tags);

            projectViewModel.ProjectStatus = projectViewModel.Status.ToString();

            string projectId;

            if (projectViewModel.Id == null)
            {
                var user = GetAuthenticatedUser();

                projectViewModel.AuthorId = user.Email;
                projectViewModel.AuthorFullName = user.GetFullName();

                projectViewModel.Created = DateTime.UtcNow;
                projectViewModel.ParticipantsCount = 0;

                projectId = await _projectRepository.SaveAsync(projectViewModel);
            }
            else
            {
                projectViewModel.LastModified = DateTime.UtcNow;

                projectId = projectViewModel.Id;

                await _projectRepository.UpdateAsync(projectViewModel);

                if (projectViewModel.Status == Status.Archive)
                {
                    await _winnersService.SaveWinners(projectViewModel.Id);
                }
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
            var projectCategories = _categoriesRepository.GetCategories();

            var project = await _projectRepository.GetAsync(id);

            project.Status = (Status)Enum.Parse(typeof(Status), project.ProjectStatus, true);

            var comments = await _commentsRepository.GetProjectCommentsAsync(id);

            var participants = await _participantsRepository.GetProjectParticipantsAsync(id);

            var results = await _resultRepository.GetResultsAsync(id);

            var user = GetAuthenticatedUser();

            var participant = (user.Email == null) ? null : await _participantsRepository.GetAsync(id, user.Email);

            var userRole = (user.Email == null) ? null : await _userRolesRepository.GetAsync(user.Email);

            var isAdmin = userRole != null && userRole.Role == "ADMIN";

            var participantId = "";
            var isParticipant = false;

            if (participant != null)
            {
                participantId = user.Email;
                isParticipant = true;
            }

            var projectFollowing = (user.Email == null) ? null : await _projectFollowRepository.GetAsync(user.Email, id);
            var isFollowing = projectFollowing != null;

            comments = comments.OrderBy(c => c.Created).Reverse().ToList();

            var statusBarPartial = new ProjectDetailsStatusBarViewModel
            {
                Status = project.Status,
                ParticipantsCount = participants.Count(),
                CompetitionRegistrationDeadline = project.CompetitionRegistrationDeadline,
                ImplementationDeadline = project.ImplementationDeadline,
                VotingDeadline = project.VotingDeadline,
                StatusCompletionPercent = CalculateStatusCompletionPercent(project)
            };

            var commentsPartial = new ProjectCommentPartialViewModel
            {
                ProjectId = project.Id,
                UserId = project.AuthorId,
                Comments = comments
            };

            var participantsPartial = new ProjectParticipantsPartialViewModel
            {
                CurrentUserId = user.Email,
                Participants = participants
            };

            var resultsPartial = new ResultsPartialViewModel
            {
                Status = project.Status,
                Results = results
            };

            var projectViewModel = new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ProjectCategories = projectCategories,
                Category = project.Category,
                Status = project.Status,
                BudgetFirstPlace = project.BudgetFirstPlace,
                BudgetSecondPlace = project.BudgetSecondPlace,
                VotesFor = project.VotesFor,
                VotesAgainst = project.VotesAgainst,
                Created = project.Created,
                LastModified = project.LastModified,
                CompetitionRegistrationDeadline = project.CompetitionRegistrationDeadline,
                ImplementationDeadline = project.ImplementationDeadline,
                VotingDeadline = project.VotingDeadline,
                StatusBarPartial = statusBarPartial,
                CommentsPartial = commentsPartial,
                ParticipantsPartial = participantsPartial,
                ResultsPartial = resultsPartial,
                AuthorId = project.AuthorId,
                AuthorFullName = project.AuthorFullName,
                ParticipantId = participantId,
                IsParticipant = isParticipant,
                IsAdmin = isAdmin,
                IsFollowing = isFollowing,
                OtherProjects = await GetOtherProjects(project.Id)
            };

            if (!string.IsNullOrEmpty(project.Tags))
            {
                projectViewModel.TagsList = JsonConvert.DeserializeObject<List<string>>(project.Tags);

                var builder = new StringBuilder();
                foreach (var tag in projectViewModel.TagsList)
                {
                    builder.Append(tag).Append(", ");
                }
                projectViewModel.Tags = builder.ToString();
            }

            var fileInfo = await _fileInfoRepository.GetAsync(id);

            if (fileInfo != null)
            {
                var fileInfoViewModel = new ProjectFileInfoViewModel
                {
                    ContentType = fileInfo.ContentType,
                    FileName = fileInfo.FileName
                };

                projectViewModel.FileInfo = fileInfoViewModel;
            }

            if (projectViewModel.Status == Status.Archive)
                projectViewModel = await PopulateResultsViewModel(projectViewModel);

            return projectViewModel;
        }

        private async Task<List<OtherProjectViewModel>> GetOtherProjects(string id)
        {
            var projects = await _projectRepository.GetProjectsAsync();

            var filteredProjects = projects.Where(x => x.Id != id).OrderByDescending(p => p.ParticipantsCount).Take(5).ToList();

            var otherProjects = new List<OtherProjectViewModel>();

            foreach (var project in filteredProjects)
            {
                var otherProject = new OtherProjectViewModel
                {
                    Id = project.Id,
                    Name = project.Name,
                    BudgetFirstPlace = project.BudgetFirstPlace,
                    Members = project.ParticipantsCount
                };

                otherProjects.Add(otherProject);
            }

            return otherProjects;
        }

        private async Task<ProjectViewModel> PopulateResultsViewModel(ProjectViewModel model)
        {
            model.ResultsPartial.BudgetFirstPlace = model.BudgetFirstPlace;
            model.ResultsPartial.BudgetSecondPlace = model.BudgetSecondPlace;
            model.ResultsPartial.ParticipantCount = model.ParticipantsPartial.Participants.Count();
            model.ResultsPartial.DaysOfContest = (DateTime.UtcNow - model.Created).Days;

            var winnersList = await _winnersRepository.GetWinnersAsync(model.Id);

            winnersList = winnersList.OrderBy(x => x.Place).ThenByDescending(x => x.Votes).ThenByDescending(x => x.Score);

            model.ResultsPartial.Winners = winnersList;

            return model;
        }

        private string SerializeTags(string tagsString)
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
                await _fileRepository.InsertProjectFile(file.OpenReadStream(), projectId);

                var fileInfo = new ProjectFileInfoEntity
                {
                    RowKey = projectId,
                    ContentType = file.ContentType,
                    FileName = file.FileName
                };

                await _fileInfoRepository.SaveAsync(fileInfo);
            }
        }

        private int CalculateStatusCompletionPercent(IProjectData projectData)
        {
            var completion = 0;

            switch (projectData.Status)
            {
                case Status.Initiative:
                    completion = 100;
                    break;
                case Status.CompetitionRegistration:
                    completion = CalculateDateProgressPercent(projectData.Created,
                        projectData.CompetitionRegistrationDeadline);
                    break;
                case Status.Implementation:
                    completion = CalculateDateProgressPercent(projectData.CompetitionRegistrationDeadline,
                        projectData.ImplementationDeadline);
                    break;
                case Status.Voting:
                    completion = CalculateDateProgressPercent(projectData.ImplementationDeadline,
                        projectData.VotingDeadline);
                    break;
                case Status.Archive:
                    completion = 100;
                    break;
            }
            return (completion < 0) ? 0 : completion;
        }

        private int CalculateDateProgressPercent(DateTime start, DateTime end)
        {
            var totalDays = (end - start).Days;

            if (totalDays == 0) return 100;

            var daysPassed = (DateTime.UtcNow - start).Days;
            var percent = daysPassed * 100 / totalDays;

            return (percent > 100) ? 100 : percent;
        }

        private CompetitionPlatformUser GetAuthenticatedUser()
        {
            return ClaimsHelper.GetUser(User.Identity);
        }
    }
}