using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage.Queue;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.AzureRepositories.Vote;
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
        private readonly IAzureQueue<string> _emailsQueue;
        private readonly IProjectResultVoteRepository _resultVoteRepository;

        public ProjectController(IProjectRepository projectRepository, IProjectCommentsRepository commentsRepository,
            IProjectFileRepository fileRepository, IProjectFileInfoRepository fileInfoRepository,
            IProjectParticipantsRepository participantsRepository, IProjectCategoriesRepository categoriesRepository,
            IProjectResultRepository resultRepository, IProjectFollowRepository projectFollowRepository,
            IProjectWinnersRepository winnersRepository, IUserRolesRepository userRolesRepository,
            IProjectWinnersService winnersService, IAzureQueue<string> emailsQueue,
            IProjectResultVoteRepository resultVoteRepository)
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
            _emailsQueue = emailsQueue;
            _resultVoteRepository = resultVoteRepository;
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var user = GetAuthenticatedUser();
            var userRole = await _userRolesRepository.GetAsync(user.Email.ToLower());
            ViewBag.ProjectCategories = _categoriesRepository.GetCategories();

            if (userRole != null)
            {
                return View("CreateProject");
            }

            //if (user.Documents.Contains("Selfie") && user.Documents.Contains("IdCard"))
            //{
            //    return View("CreateProject");
            //}

            return View("CreateClosed");
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
        public async Task<IActionResult> SaveProject(ProjectViewModel projectViewModel, bool draft = false, bool enableVoting = false, bool enableRegistration = false)
        {
            projectViewModel.Tags = SerializeTags(projectViewModel.Tags);

            projectViewModel.ProjectStatus = projectViewModel.Status.ToString();

            projectViewModel.SkipVoting = !enableVoting;
            projectViewModel.SkipRegistration = !enableRegistration;
            if (projectViewModel.CompetitionRegistrationDeadline == DateTime.MinValue)
                projectViewModel.CompetitionRegistrationDeadline = DateTime.UtcNow.Date;

            if (projectViewModel.VotingDeadline == DateTime.MinValue)
                projectViewModel.VotingDeadline = DateTime.UtcNow.Date;

            string projectId;

            if (projectViewModel.Id == null)
            {
                //var actualResources = projectViewModel.ResourcesList.Where(resource => !string.IsNullOrEmpty(resource.Name) && !string.IsNullOrEmpty(resource.Link)).ToList();

                //projectViewModel.ProgrammingResources = JsonConvert.SerializeObject(actualResources);

                projectViewModel.Status = draft ? Status.Draft : Status.Initiative;

                var user = GetAuthenticatedUser();

                projectViewModel.AuthorId = user.Email;
                projectViewModel.AuthorFullName = user.GetFullName();
                projectViewModel.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                projectViewModel.Created = DateTime.UtcNow;
                projectViewModel.ParticipantsCount = 0;

                projectId = await _projectRepository.SaveAsync(projectViewModel);

                if (_emailsQueue != null)
                {
                    var message = NotificationMessageHelper.ProjectCreatedMessage(user.Email, user.GetFullName(),
                        projectViewModel.Name);
                    await _emailsQueue.PutMessageAsync(message);
                }

                await SaveProjectFile(projectViewModel.File, projectId);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                var project = await _projectRepository.GetAsync(projectViewModel.Id);

                project.Status = StatusHelper.GetProjectStatusFromString(project.ProjectStatus);

                projectViewModel.LastModified = DateTime.UtcNow;

                projectViewModel.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                projectViewModel.ParticipantsCount = project.ParticipantsCount;

                projectId = projectViewModel.Id;

                await _projectRepository.UpdateAsync(projectViewModel);

                if (project.Status != Status.Registration && projectViewModel.Status == Status.Registration)
                {
                    await AddCompetitionMailToQueue(project);
                }

                if (project.Status != Status.Submission && projectViewModel.Status == Status.Submission)
                {
                    await AddImplementationMailToQueue(project);
                }

                if (project.Status != Status.Voting && projectViewModel.Status == Status.Voting)
                {
                    await AddVotingMailToQueue(project);
                }

                if (project.Status != Status.Archive && projectViewModel.Status == Status.Archive)
                {
                    if (!project.SkipVoting)
                    {
                        await _winnersService.SaveWinners(projectViewModel.Id);
                    }

                    await AddArchiveMailToQueue(project);
                }

                await SaveProjectFile(projectViewModel.File, projectId);

                return RedirectToAction("ProjectDetails", "Project", new { id = projectViewModel.Id });
            }
        }

        private async Task AddCompetitionMailToQueue(IProjectData project)
        {
            var following = await GetProjectFollows(project.Id);

            foreach (var follower in following)
            {
                if (_emailsQueue != null)
                {
                    var message = NotificationMessageHelper.GenerateCompetitionMessage(project, follower);
                    await _emailsQueue.PutMessageAsync(message);
                }
            }
        }

        private async Task AddImplementationMailToQueue(IProjectData project)
        {
            var following = await GetProjectFollows(project.Id);

            var participants = await _participantsRepository.GetProjectParticipantsAsync(project.Id);
            var projectParticipateData = participants as IList<IProjectParticipateData> ?? participants.ToList();

            foreach (var follower in following)
            {
                if (_emailsQueue != null)
                {
                    var participant = projectParticipateData.FirstOrDefault(x => x.ProjectId == project.Id && x.UserId == follower.UserId);

                    var templateType = "";
                    templateType = participant != null ? "ImplementationParticipant" : "ImplementationFollower";

                    var message = NotificationMessageHelper.GenerateImplementationMessage(project, follower,
                            templateType);
                    await _emailsQueue.PutMessageAsync(message);
                }
            }
        }

        private async Task AddVotingMailToQueue(IProjectData project)
        {
            var following = await GetProjectFollows(project.Id);

            foreach (var follower in following)
            {
                if (_emailsQueue != null)
                {
                    var message = NotificationMessageHelper.GenerateVotingMessage(project, follower);
                    await _emailsQueue.PutMessageAsync(message);
                }
            }
        }

        private async Task AddArchiveMailToQueue(IProjectData project)
        {
            var participantsCount = await _participantsRepository.GetProjectParticipantsCountAsync(project.Id);
            var resultsCount = await _resultRepository.GetResultsCountAsync(project.Id);
            var winners = await _winnersRepository.GetWinnersAsync(project.Id);

            var following = await GetProjectFollows(project.Id);

            foreach (var follower in following)
            {
                if (_emailsQueue != null)
                {
                    var message = NotificationMessageHelper.GenerateArchiveMessage(project, follower, participantsCount, resultsCount, winners);
                    await _emailsQueue.PutMessageAsync(message);
                }
            }
        }

        private async Task<IEnumerable<IProjectFollowData>> GetProjectFollows(string projectId)
        {
            var follows = await _projectFollowRepository.GetFollowAsync();
            var projectFollows = follows.Where(f => f.ProjectId == projectId).ToList();
            return projectFollows;
        }

        public async Task<IActionResult> ProjectDetails(string id, bool participantAdded = false, bool votedForResult = false, bool votedTwice = false,
            bool commentsActive = false, bool participantsActive = false, bool resultsActive = false, bool winnersActive = false)
        {
            if (participantAdded)
            {
                ViewBag.ParticipantAdded = true;
            }

            if (votedForResult)
            {
                ViewBag.VotedForResult = true;
            }

            if (votedTwice)
            {
                ViewBag.VotedTwice = true;
            }

            if (commentsActive)
            {
                ViewBag.CommentsActive = true;
            }

            if (participantsActive)
            {
                ViewBag.ParticipantsActive = true;
            }

            if (winnersActive)
            {
                ViewBag.WinnersActive = true;
            }

            if (resultsActive)
            {
                ViewBag.ResultsActive = true;
            }

            var viewModel = await GetProjectViewModel(id);
            return View(viewModel);
        }

        private async Task<ProjectViewModel> GetProjectViewModel(string id)
        {
            var projectCategories = _categoriesRepository.GetCategories();

            var project = await _projectRepository.GetAsync(id);

            //var resources = JsonConvert.DeserializeObject<List<ProgrammingResource>>(project.ProgrammingResources);

            project.Status = StatusHelper.GetProjectStatusFromString(project.ProjectStatus);

            var comments = await _commentsRepository.GetProjectCommentsAsync(id);

            var participants = await _participantsRepository.GetProjectParticipantsAsync(id);

            var results = await _resultRepository.GetResultsAsync(id);

            var user = GetAuthenticatedUser();

            var participant = (user.Email == null) ? null : await _participantsRepository.GetAsync(id, user.Email);

            var userRole = (user.Email == null) ? null : await _userRolesRepository.GetAsync(user.Email.ToLower());

            var isAdmin = (userRole != null) && userRole.Role == "ADMIN";
            var isAuthor = (user.Email != null) && user.Email == project.AuthorId;

            var participantId = "";
            var isParticipant = false;
            var hasResult = false;

            if (participant != null)
            {
                participantId = user.Email;
                isParticipant = true;

                hasResult = results.Any(r => r.ParticipantId == user.Email);
            }

            var projectFollowing = (user.Email == null) ? null : await _projectFollowRepository.GetAsync(user.Email, id);
            var isFollowing = projectFollowing != null;

            comments = SortComments(comments);

            var commenterIsModerator = new Dictionary<string, bool>();

            foreach (var comment in comments)
            {
                var role = await _userRolesRepository.GetAsync(comment.UserId);
                var isModerator = role != null && role.Role == "ADMIN";
                commenterIsModerator.Add(comment.Id, isModerator);
            }

            var userVotedForResults = new Dictionary<string, bool>();
            var resultVotes = await _resultVoteRepository.GetProjectResultVotesAsync(project.Id);

            foreach (var result in results)
            {
                var match =
                    resultVotes.FirstOrDefault(x => x.ParticipantId == result.ParticipantId && x.VoterUserId == user.Email);

                userVotedForResults.Add(result.ParticipantId, match != null && user.Email != null);
            }

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
                Comments = comments,
                IsAdmin = isAdmin,
                IsAuthor = isAuthor,
                CommenterIsModerator = commenterIsModerator,
                ProjectAuthorId = project.AuthorId
            };

            var participantsPartial = new ProjectParticipantsPartialViewModel
            {
                CurrentUserId = user.Email,
                Participants = participants,
                Status = project.Status,
                HasResult = hasResult
            };

            var resultsPartial = new ResultsPartialViewModel
            {
                Status = project.Status,
                Results = results,
                IsAdmin = isAdmin,
                SkipVoting = project.SkipVoting,
                UserVotedForResults = userVotedForResults
            };

            var projectViewModel = new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Overview = project.Overview,
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
                OtherProjects = await GetOtherProjects(project.Id),
                ProgrammingResourceName = project.ProgrammingResourceName,
                ProgrammingResourceLink = project.ProgrammingResourceLink,
                SkipVoting = project.SkipVoting,
                SkipRegistration = project.SkipRegistration
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
                case Status.Registration:
                    completion = CalculateDateProgressPercent(projectData.Created,
                        projectData.CompetitionRegistrationDeadline);
                    break;
                case Status.Submission:
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

        private IEnumerable<ICommentData> SortComments(IEnumerable<ICommentData> comments)
        {
            var commentsData = comments as IList<ICommentData> ?? comments.ToList();

            var childComments = commentsData.Where(x => x.ParentId != null).ToList();
            comments = commentsData.Where(x => x.ParentId == null).ToList();
            comments = comments.OrderBy(c => c.Created).Reverse().ToList();

            var sortedComments = new List<ICommentData>();

            foreach (var comment in comments)
            {
                sortedComments.Add(comment);
                var children = childComments.Where(x => x.ParentId == comment.Id).ToList();
                children = children.OrderBy(x => x.Created).ToList();
                sortedComments.AddRange(children);
            }

            return sortedComments;
        }

        private CompetitionPlatformUser GetAuthenticatedUser()
        {
            return ClaimsHelper.GetUser(User.Identity);
        }
    }
}