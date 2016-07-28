using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using Microsoft.AspNetCore.Mvc;
using CompetitionPlatform.Models.ProjectViewModels;

namespace CompetitionPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectCommentsRepository _projectCommentsRepository;

        public HomeController(IProjectRepository projectRepository, IProjectCommentsRepository projectCommentsRepository)
        {
            _projectRepository = projectRepository;
            _projectCommentsRepository = projectCommentsRepository;
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

        private async Task<ProjectListIndexViewModel> GetProjectListViewModel(string projectStatusFilter = null, string projectCategoryFilter = null)
        {
            var projects = await _projectRepository.GetProjectsAsync();

            if(!string.IsNullOrEmpty(projectStatusFilter))
                projects = projects.Where(x => x.Status.ToString() == projectStatusFilter);

            if (!string.IsNullOrEmpty(projectCategoryFilter))
                projects = projects.Where(x => x.Category == projectCategoryFilter);

            var compactModels = new List<ProjectCompactViewModel>();

            foreach (var project in projects)
            {
                var projectCommentsCount = await _projectCommentsRepository.GetProjectCommentsCountAsync(project.Id);

                var compactModel = new ProjectCompactViewModel
                {
                    Id = project.Id,
                    Name = project.Name.Length > 43 ? project.Name.Substring(0, 40) + "..." : project.Name,
                    Description = project.Description.Length > 500 ? project.Description.Substring(0, 497) + "..." : project.Description,
                    Status = project.Status,
                    BudgetFirstPlace = project.BudgetFirstPlace,
                    VotesFor = project.VotesFor,
                    VotesAgainst = project.VotesAgainst,
                    CommentsCount = projectCommentsCount
                };

                compactModels.Add(compactModel);
            }

            var viewModel = new ProjectListIndexViewModel
            {
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
    }
}
