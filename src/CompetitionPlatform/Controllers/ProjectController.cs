using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Models;
using Microsoft.AspNetCore.Mvc;

namespace CompetitionPlatform.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectController(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
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
        public IActionResult CreateProject(Project project)
        {
            _projectRepository.SaveAsync(project);
            return View("~/Views/Home/Index.cshtml");
        }
    }
}