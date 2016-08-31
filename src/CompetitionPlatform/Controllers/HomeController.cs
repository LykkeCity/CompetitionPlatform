using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.ProjectCategory;
using CompetitionPlatform.Helpers;
using CompetitionPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using CompetitionPlatform.Models.ProjectViewModels;
using Microsoft.AspNetCore.Authorization;

namespace CompetitionPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectCommentsRepository _commentsRepository;
        private readonly IProjectCategoriesRepository _categoriesRepository;
        private readonly IProjectParticipantsRepository _participantsRepository;
        private readonly IProjectFollowRepository _projectFollowRepository;

        public HomeController(IProjectRepository projectRepository, IProjectCommentsRepository commentsRepository,
            IProjectCategoriesRepository categoriesRepository, IProjectParticipantsRepository participantsRepository,
            IProjectFollowRepository projectFollowRepository)
        {
            _projectRepository = projectRepository;
            _commentsRepository = commentsRepository;
            _categoriesRepository = categoriesRepository;
            _participantsRepository = participantsRepository;
            _projectFollowRepository = projectFollowRepository;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await GetProjectListViewModel();
            return View(viewModel);
        }

        public async Task<IActionResult> GetProjectList(string projectStatusFilter, string projectCategoryFilter)
        {
            var viewModel = await GetProjectListViewModel(projectStatusFilter, projectCategoryFilter);
            return PartialView("ProjectListPartial", viewModel);
        }

        private async Task<ProjectListIndexViewModel> GetProjectListViewModel(string projectStatusFilter = null, string projectCategoryFilter = null, string projectAuthorId = null)
        {
            var projects = await _projectRepository.GetProjectsAsync();

            var projectCategories = _categoriesRepository.GetCategories();

            if (!string.IsNullOrEmpty(projectStatusFilter))
                projects = projects.Where(x => x.ProjectStatus == projectStatusFilter);

            if (!string.IsNullOrEmpty(projectCategoryFilter))
                projects = projects.Where(x => x.Category == projectCategoryFilter);

            if (!string.IsNullOrEmpty(projectAuthorId))
                projects = projects.Where(x => x.AuthorId == projectAuthorId);

            var compactModels = await GetCompactProjectsList(projects);

            var viewModel = new ProjectListIndexViewModel
            {
                ProjectCategories = projectCategories,
                Projects = compactModels
            };

            return viewModel;
        }

        private async Task<List<ProjectCompactViewModel>> GetCompactProjectsList(IEnumerable<IProjectData> projects)
        {
            var compactModels = new List<ProjectCompactViewModel>();

            foreach (var project in projects)
            {
                var projectCommentsCount = await _commentsRepository.GetProjectCommentsCountAsync(project.Id);
                var participantsCount = await _participantsRepository.GetProjectParticipantsCountAsync(project.Id);

                var compactModel = new ProjectCompactViewModel
                {
                    Id = project.Id,
                    Name = project.Name.Length > 36 ? project.Name.Substring(0, 33) + "..." : project.Name,
                    Description = project.Description.Length > 500 ? project.Description.Substring(0, 497) + "..." : project.Description,
                    BudgetFirstPlace = project.BudgetFirstPlace,
                    VotesFor = project.VotesFor,
                    VotesAgainst = project.VotesAgainst,
                    CompetitionRegistrationDeadline = project.CompetitionRegistrationDeadline,
                    ImplementationDeadline = project.ImplementationDeadline,
                    VotingDeadline = project.VotingDeadline,
                    CommentsCount = projectCommentsCount,
                    ParticipantsCount = participantsCount,
                    AuthorFullName = project.AuthorFullName
                };

                if (!string.IsNullOrEmpty(project.ProjectStatus))
                    compactModel.Status = (Status)Enum.Parse(typeof(Status), project.ProjectStatus, true);

                compactModels.Add(compactModel);
            }

            return compactModels;
        }

        [Authorize]
        public async Task<IActionResult> FilterMyProjects()
        {
            var user = GetAuthenticatedUser();
            var viewModel = await GetProjectListViewModel(projectAuthorId: user.Email);

            return View("~/Views/Home/Index.cshtml", viewModel);
        }

        [Authorize]
        public async Task<IActionResult> FilterFollowingProjects()
        {
            var projectCategories = _categoriesRepository.GetCategories();

            var user = GetAuthenticatedUser();

            var projects = await _projectRepository.GetProjectsAsync();

            var following = await _projectFollowRepository.GetProjectsFollowAsync(user.Email);

            var filtered = projects
                   .Where(x => following.Any(y => y.ProjectId == x.Id));

            var compactModels = await GetCompactProjectsList(filtered);

            var viewModel = new ProjectListIndexViewModel
            {
                ProjectCategories = projectCategories,
                Projects = compactModels
            };

            return View("~/Views/Home/Index.cshtml", viewModel);
        }

        [Authorize]
        public IActionResult SignIn()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        private CompetitionPlatformUser GetAuthenticatedUser()
        {
            return ClaimsHelper.GetUser(User.Identity);
        }

        public string Version()
        {
            return typeof(HomeController).GetTypeInfo().Assembly.GetName().Version.ToString();
        }
    }
}
