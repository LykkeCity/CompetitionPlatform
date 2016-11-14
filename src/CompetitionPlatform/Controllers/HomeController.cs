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
using Newtonsoft.Json;

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
        private readonly IUserFeedbackRepository _feedbackRepository;
        private readonly IUserRolesRepository _userRolesRepository;

        public HomeController(IProjectRepository projectRepository, IProjectCommentsRepository commentsRepository,
            IProjectCategoriesRepository categoriesRepository, IProjectParticipantsRepository participantsRepository,
            IProjectFollowRepository projectFollowRepository, IProjectResultRepository resultsRepository,
            IProjectWinnersRepository winnersRepository, IUserFeedbackRepository feedbackRepository,
            IUserRolesRepository userRolesRepository)
        {
            _projectRepository = projectRepository;
            _commentsRepository = commentsRepository;
            _categoriesRepository = categoriesRepository;
            _participantsRepository = participantsRepository;
            _projectFollowRepository = projectFollowRepository;
            _resultsRepository = resultsRepository;
            _winnersRepository = winnersRepository;
            _feedbackRepository = feedbackRepository;
            _userRolesRepository = userRolesRepository;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await GetProjectListViewModel(currentProjects: true);
            return View(viewModel);
        }

        public async Task<IActionResult> Allprojects()
        {
            var viewModel = await GetProjectListViewModel();
            return View(viewModel);
        }

        [Authorize]
        public async Task<IActionResult> Myprojects()
        {
            var user = GetAuthenticatedUser();
            var viewModel = await GetMyProjectListViewModel(userId: user.Email, createdProjects: true, followingProjects: true, participatingProjects: true);

            return View(viewModel);
        }

        public async Task<IActionResult> GetProjectList(string projectStatusFilter, string projectCategoryFilter, string projectPrizeFilter)
        {
            var viewModel = await GetProjectListViewModel(projectStatusFilter, projectCategoryFilter, projectPrizeFilter);
            return PartialView("ProjectListPartial", viewModel);
        }

        public async Task<IActionResult> GetMyProjectList(string myProjectStatusFilter, string myProjectCategoryFilter, string myProjectPrizeFilter)
        {
            var user = GetAuthenticatedUser();
            var viewModel = await GetMyProjectListViewModel(userId: user.Email, createdProjects: true, followingProjects: true, participatingProjects: true,
                myProjectCategoryFilter: myProjectCategoryFilter, myProjectPrizeFilter: myProjectPrizeFilter, myProjectStatusFilter: myProjectStatusFilter);
            return PartialView("ProjectListPartial", viewModel);
        }

        private async Task<ProjectListIndexViewModel> GetProjectListViewModel(string projectStatusFilter = null, string projectCategoryFilter = null,
            string projectPrizeFilter = null, string projectAuthorId = null,
            bool currentProjects = false)
        {
            var projects = await _projectRepository.GetProjectsAsync();

            var projectCategories = _categoriesRepository.GetCategories();

            if (!string.IsNullOrEmpty(projectStatusFilter) && projectStatusFilter != "All")
                projects = projects.Where(x => x.ProjectStatus == projectStatusFilter);

            if (!string.IsNullOrEmpty(projectCategoryFilter) && projectCategoryFilter != "All")
                projects = projects.Where(x => x.Category.Replace(" ", "") == projectCategoryFilter);

            if (!string.IsNullOrEmpty(projectAuthorId))
                projects = projects.Where(x => x.AuthorId == projectAuthorId);

            if (currentProjects)
                projects = projects.Where(x => x.ProjectStatus != Status.Initiative.ToString() && x.ProjectStatus != Status.Archive.ToString());

            if (!string.IsNullOrEmpty(projectPrizeFilter))
            {
                if (projectPrizeFilter == "Ascending")
                    projects = projects.OrderBy(x => x.BudgetFirstPlace);

                if (projectPrizeFilter == "Descending")
                    projects = projects.OrderByDescending(x => x.BudgetFirstPlace);
            }
            else
            {
                projects = projects.OrderBy(x => x.BudgetFirstPlace);
            }

            var compactModels = await GetCompactProjectsList(projects);

            var viewModel = new ProjectListIndexViewModel
            {
                ProjectCategories = projectCategories,
                Projects = compactModels
            };

            return viewModel;
        }

        private async Task<ProjectListIndexViewModel> GetMyProjectListViewModel(string userId = null, bool createdProjects = false,
            bool followingProjects = false, bool participatingProjects = false,
            string myProjectStatusFilter = null, string myProjectCategoryFilter = null,
            string myProjectPrizeFilter = null)
        {
            var authoredProjects = await _projectRepository.GetProjectsAsync();

            var followedProjects = authoredProjects as IList<IProjectData> ?? authoredProjects.ToList();
            var participatedProjects = authoredProjects as IList<IProjectData> ?? authoredProjects.ToList();

            var projectCategories = _categoriesRepository.GetCategories();

            if (createdProjects && !string.IsNullOrEmpty(userId))
                authoredProjects = authoredProjects.Where(x => x.AuthorId == userId);

            if (followingProjects)
            {
                foreach (var project in followedProjects.ToList())
                {
                    var match = await _projectFollowRepository.GetAsync(userId, project.Id);
                    if (match == null)
                        followedProjects.Remove(project);
                }
            }

            if (participatingProjects)
            {
                foreach (var project in participatedProjects.ToList())
                {
                    var match = await _participantsRepository.GetAsync(project.Id, userId);
                    if (match == null)
                        participatedProjects.Remove(project);
                }
            }

            var allProjects = authoredProjects.Union(followedProjects).Union(participatedProjects);

            if (!string.IsNullOrEmpty(myProjectStatusFilter) && myProjectStatusFilter != "All")
                allProjects = allProjects.Where(x => x.ProjectStatus == myProjectStatusFilter);

            if (!string.IsNullOrEmpty(myProjectCategoryFilter) && myProjectCategoryFilter != "All")
                allProjects = allProjects.Where(x => x.Category.Replace(" ", "") == myProjectCategoryFilter);

            if (!string.IsNullOrEmpty(myProjectPrizeFilter))
            {
                if (myProjectPrizeFilter == "Ascending")
                    allProjects = allProjects.OrderBy(x => x.BudgetFirstPlace);

                if (myProjectPrizeFilter == "Descending")
                    allProjects = allProjects.OrderByDescending(x => x.BudgetFirstPlace);
            }
            else
            {
                allProjects = allProjects.OrderBy(x => x.BudgetFirstPlace);
            }

            var compactModels = await GetCompactProjectsList(allProjects);

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
            var user = GetAuthenticatedUser();

            foreach (var project in projects)
            {
                var projectCommentsCount = await _commentsRepository.GetProjectCommentsCountAsync(project.Id);
                var participantsCount = await _participantsRepository.GetProjectParticipantsCountAsync(project.Id);
                var resultsCount = await _resultsRepository.GetResultsCountAsync(project.Id);
                var winnersCount = await _winnersRepository.GetWinnersCountAsync(project.Id);

                var tagsList = new List<string>();
                if (!string.IsNullOrEmpty(project.Tags))
                {
                    tagsList = JsonConvert.DeserializeObject<List<string>>(project.Tags);
                }

                var following = false;
                if (user.Email != null)
                {
                    var follow = await _projectFollowRepository.GetAsync(user.Email, project.Id);
                    if (follow != null)
                        following = true;
                }

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
                    Category = project.Category,
                    Tags = tagsList,
                    Following = following
                };

                if (!string.IsNullOrEmpty(project.ProjectStatus))
                {
                    compactModel.Status = StatusHelper.GetProjectStatusFromString(project.ProjectStatus);
                }

                compactModels.Add(compactModel);
            }

            return compactModels;
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
        public IActionResult LeaveFeedback()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> ViewFeedBack()
        {
            var user = GetAuthenticatedUser();
            var userRole = await _userRolesRepository.GetAsync(user.Email);

            if (userRole == null) return View("AccessDenied");

            var feedback = await _feedbackRepository.GetFeedbacksAsync();
            feedback = feedback.OrderByDescending(x => x.Created);

            var viewModel = new FeedbackListViewModel
            {
                FeedbackList = feedback
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveFeeback(FeedbackViewModel feedbackViewModel)
        {
            var user = GetAuthenticatedUser();
            feedbackViewModel.Email = user.Email;
            feedbackViewModel.Created = DateTime.UtcNow;

            await _feedbackRepository.SaveAsync(feedbackViewModel);
            return RedirectToAction("Index", "Home");
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
