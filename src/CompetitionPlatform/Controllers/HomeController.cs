using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.ProjectCategory;
using CompetitionPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using CompetitionPlatform.Models.ProjectViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CompetitionPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectCommentsRepository _projectCommentsRepository;
        private readonly IProjectCategoriesRepository _projectCategoriesRepository;

        public HomeController(IProjectRepository projectRepository, IProjectCommentsRepository projectCommentsRepository,
            IProjectCategoriesRepository projectCategoriesRepository)
        {
            _projectRepository = projectRepository;
            _projectCommentsRepository = projectCommentsRepository;
            _projectCategoriesRepository = projectCategoriesRepository;
        }

        [Authorize]
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

        private async Task<ProjectListIndexViewModel> GetProjectListViewModel(string projectStatusFilter = null, string projectCategoryFilter = null)
        {
            var projects = await _projectRepository.GetProjectsAsync();

            var projectCategories = _projectCategoriesRepository.GetCategories();

            if (!string.IsNullOrEmpty(projectStatusFilter))
                projects = projects.Where(x => x.ProjectStatus == projectStatusFilter);

            if (!string.IsNullOrEmpty(projectCategoryFilter))
                projects = projects.Where(x => x.Category == projectCategoryFilter);

            var compactModels = new List<ProjectCompactViewModel>();

            foreach (var project in projects)
            {
                var projectCommentsCount = await _projectCommentsRepository.GetProjectCommentsCountAsync(project.Id);

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
                    AuthorFullName = project.AuthorFullName
                };

                if (!string.IsNullOrEmpty(project.ProjectStatus))
                    compactModel.Status = (Status)Enum.Parse(typeof(Status), project.ProjectStatus, true);

                compactModels.Add(compactModel);
            }

            var viewModel = new ProjectListIndexViewModel
            {
                ProjectCategories = projectCategories,
                Projects = compactModels
            };

            return viewModel;
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

        public string Version()
        {
            return typeof(HomeController).GetTypeInfo().Assembly.GetName().Version.ToString();
        }
    }
}
