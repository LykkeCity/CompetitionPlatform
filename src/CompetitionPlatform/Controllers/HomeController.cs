using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.ProjectCategory;
using CompetitionPlatform.Helpers;
using CompetitionPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using CompetitionPlatform.Models.ProjectViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        private readonly IProjectResultRepository _resultsRepository;
        private readonly IProjectWinnersRepository _winnersRepository;

        public HomeController(IProjectRepository projectRepository, IProjectCommentsRepository commentsRepository,
            IProjectCategoriesRepository categoriesRepository, IProjectParticipantsRepository participantsRepository,
            IProjectFollowRepository projectFollowRepository, IProjectResultRepository resultsRepository,
            IProjectWinnersRepository winnersRepository)
        {
            _projectRepository = projectRepository;
            _commentsRepository = commentsRepository;
            _categoriesRepository = categoriesRepository;
            _participantsRepository = participantsRepository;
            _projectFollowRepository = projectFollowRepository;
            _resultsRepository = resultsRepository;
            _winnersRepository = winnersRepository;
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

            if (!string.IsNullOrEmpty(projectStatusFilter) && projectStatusFilter != "All")
                projects = projects.Where(x => x.ProjectStatus == projectStatusFilter);

            if (!string.IsNullOrEmpty(projectCategoryFilter) && projectCategoryFilter != "All")
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
                var resultsCount = await _resultsRepository.GetResultsCountAsync(project.Id);
                var winnersCount = await _winnersRepository.GetWinnersCountAsync(project.Id);

                var compactModel = new ProjectCompactViewModel
                {
                    Id = project.Id,
                    Name = project.Name,
                    Overview = project.Overview,
                    Description = project.Description,
                    BudgetFirstPlace = project.BudgetFirstPlace,
                    VotesFor = project.VotesFor,
                    VotesAgainst = project.VotesAgainst,
                    CompetitionRegistrationDeadline = project.CompetitionRegistrationDeadline,
                    ImplementationDeadline = project.ImplementationDeadline,
                    VotingDeadline = project.VotingDeadline,
                    CommentsCount = projectCommentsCount,
                    ParticipantsCount = participantsCount,
                    ResultsCount = resultsCount,
                    WinnersCount = winnersCount,
                    AuthorFullName = project.AuthorFullName,
                    Category = project.Category
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

            var follows = await _projectFollowRepository.GetFollowAsync();

            var following = follows.Where(f => f.UserId == user.Email).ToList();

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

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.Authentication.SignOutAsync("OpenIdConnect");
            }
            return RedirectToAction("Index", "Home");
        }

        private CompetitionPlatformUser GetAuthenticatedUser()
        {
            return ClaimsHelper.GetUser(User.Identity);
        }

        public IActionResult AuthenticationFailed()
        {
            return View("AuthenticationFailed");
        }

        public string Version()
        {
            return typeof(HomeController).GetTypeInfo().Assembly.GetName().Version.ToString();
        }
    }
}
