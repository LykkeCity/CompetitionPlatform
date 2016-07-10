using System;
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

        public HomeController(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _projectRepository.GetProjectsAsync();

            var compactModels = new List<ProjectCompactViewModel>();

            foreach (var project in projects)
            {
                var compactModel = new ProjectCompactViewModel()
                {
                    Name = project.Name.Length > 35? project.Name.Substring(0,32) + "..." : project.Name,
                    Description = project.Description.Length > 500 ? project.Description.Substring(0, 497) + "..." : project.Description,
                    Status = project.Status,
                    BudgetFirstPlace = project.BudgetFirstPlace,
                    VotesFor = project.VotesFor,
                    VotesAgainst = project.VotesAgainst
                };

                compactModels.Add(compactModel);
            }

            var viewModel = new ProjectListIndexViewModel()
            {
                Projects = compactModels
            };

            return View(viewModel);
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
