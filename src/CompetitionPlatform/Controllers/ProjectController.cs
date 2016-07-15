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

        public ProjectController(IProjectRepository projectRepository, IProjectFileRepository projectFileRepository)
        {
            _projectRepository = projectRepository;
            _projectFileRepository = projectFileRepository;
        }

        public IActionResult Index()
        {
            return View();
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
            projectViewModel.Tags = JsonConvert.SerializeObject(tagsList);

            string newProjectId = await _projectRepository.SaveAsync(projectViewModel);

            if (projectViewModel.File != null)
            {
                await _projectFileRepository.InsertProjectFile(projectViewModel.File.OpenReadStream(), newProjectId);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ProjectDetails(string id)
        {
            var project = await _projectRepository.GetAsync(id);

            var projectViewModel = new ProjectViewModel()
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Categories = JsonConvert.DeserializeObject<List<string>>(project.Tags),
                Status = project.Status,
                BudgetFirstPlace = project.BudgetFirstPlace,
                BudgetSecondPlace = project.BudgetSecondPlace,
                BudgetThirdPlace = project.BudgetThirdPlace,
                VotesFor = project.VotesFor,
                VotesAgainst = project.VotesAgainst
            };

            return View(projectViewModel);
        }
    }
}