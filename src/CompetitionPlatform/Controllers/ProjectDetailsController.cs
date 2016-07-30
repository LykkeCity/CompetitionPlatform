using System;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Vote;
using CompetitionPlatform.Models.ProjectViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CompetitionPlatform.Controllers
{
    public class ProjectDetailsController : Controller
    {
        private readonly IProjectCommentsRepository _projectCommentsRepository;
        private readonly IProjectFileRepository _projectFileRepository;
        private readonly IProjectFileInfoRepository _projectFileInfoRepository;
        private readonly IProjectVoteRepository _projectVoteRepository;
        private readonly IProjectRepository _projectRepository;

        public ProjectDetailsController(IProjectCommentsRepository projectCommentsRepository, IProjectFileRepository projectFileRepository,
            IProjectFileInfoRepository projectFileInfoRepository, IProjectVoteRepository projectVoteRepository,
            IProjectRepository projectRepository)
        {
            _projectCommentsRepository = projectCommentsRepository;
            _projectFileRepository = projectFileRepository;
            _projectFileInfoRepository = projectFileInfoRepository;
            _projectVoteRepository = projectVoteRepository;
            _projectRepository = projectRepository;
        }

        public IActionResult AddComment(ProjectCommentPartialViewModel model)
        {
            model.User = "User1";
            model.Created = DateTime.UtcNow;
            model.LastModified = model.Created;

            _projectCommentsRepository.SaveAsync(model);
            return RedirectToAction("ProjectDetails", "Project", new { id = model.ProjectId });
        }

        public async Task<IActionResult> DownloadProjectFile(string id)
        {
            var fileInfo = await _projectFileInfoRepository.GetAsync(id);

            var fileStream = await _projectFileRepository.GetProjectFile(id);
            return File(fileStream, fileInfo.ContentType, fileInfo.FileName);
        }

        public async Task<IActionResult> VoteFor(string id)
        {
            var user = "User1";

            var result = new ProjectVoteEntity
            {
                ProjectId = id,
                VoterUserId = user,
                ForAgainst = 1,
            };

            await _projectVoteRepository.SaveAsync(result);

            var project = await _projectRepository.GetAsync(id);

            project.VotesFor += 1;

            await _projectRepository.UpdateAsync(project);

            return RedirectToAction("ProjectDetails", "Project", new { id = id });
        }

        public async Task<IActionResult> VoteAgainst(string id)
        {
            var user = "User1";

            var result = new ProjectVoteEntity
            {
                ProjectId = id,
                VoterUserId = user,
                ForAgainst = -1
            };

            await _projectVoteRepository.SaveAsync(result);

            var project = await _projectRepository.GetAsync(id);

            project.VotesAgainst += 1;

            await _projectRepository.UpdateAsync(project);

            return RedirectToAction("ProjectDetails", "Project", new { id = id });
        }
    }
}