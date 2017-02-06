using System;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.AzureRepositories.Vote;
using CompetitionPlatform.Helpers;
using CompetitionPlatform.Models;
using CompetitionPlatform.Models.ProjectViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompetitionPlatform.Controllers
{
    public class ProjectDetailsController : Controller
    {
        private readonly IProjectCommentsRepository _commentsRepository;
        private readonly IProjectFileRepository _fileRepository;
        private readonly IProjectFileInfoRepository _fileInfoRepository;
        private readonly IProjectVoteRepository _voteRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectParticipantsRepository _participantsRepository;
        private readonly IProjectResultRepository _resultRepository;
        private readonly IProjectResultVoteRepository _resultVoteRepository;
        private readonly IProjectFollowRepository _projectFollowRepository;
        private readonly IFollowMailSentRepository _mailSentRepository;
        private readonly IQueueExt _emailsQueue;
        private readonly IUserRolesRepository _userRolesRepository;
        private readonly IProjectWinnersRepository _winnersRepository;

        public ProjectDetailsController(IProjectCommentsRepository commentsRepository, IProjectFileRepository fileRepository,
            IProjectFileInfoRepository fileInfoRepository, IProjectVoteRepository voteRepository,
            IProjectRepository projectRepository, IProjectParticipantsRepository participantsRepository,
            IProjectResultRepository resultRepository, IProjectResultVoteRepository resultVoteRepository,
            IProjectFollowRepository projectFollowRepository, IFollowMailSentRepository mailSentRepository,
            IQueueExt emailsQueue, IUserRolesRepository userRolesRepository,
            IProjectWinnersRepository winnersRepository)
        {
            _commentsRepository = commentsRepository;
            _fileRepository = fileRepository;
            _fileInfoRepository = fileInfoRepository;
            _voteRepository = voteRepository;
            _projectRepository = projectRepository;
            _participantsRepository = participantsRepository;
            _resultRepository = resultRepository;
            _resultVoteRepository = resultVoteRepository;
            _projectFollowRepository = projectFollowRepository;
            _mailSentRepository = mailSentRepository;
            _emailsQueue = emailsQueue;
            _userRolesRepository = userRolesRepository;
            _winnersRepository = winnersRepository;
        }

        [Authorize]
        public async Task<IActionResult> AddComment(ProjectCommentPartialViewModel model)
        {
            var user = GetAuthenticatedUser();
            model.UserId = user.Email;
            model.FullName = user.GetFullName();
            model.Created = DateTime.UtcNow;
            model.LastModified = model.Created;
            model.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            await _commentsRepository.SaveAsync(model);
            return RedirectToAction("ProjectDetails", "Project", new { id = model.ProjectId, commentsActive = true });
        }

        public IActionResult GetCommentReplyForm(string commentId, string projectId)
        {
            var user = GetAuthenticatedUser();
            var created = DateTime.UtcNow;

            var model = new ProjectCommentPartialViewModel
            {
                UserId = user.Email,
                FullName = user.GetFullName(),
                ProjectId = projectId,
                ParentId = commentId,
                Created = created,
                LastModified = created
            };

            return PartialView("~/Views/Project/CommentReplyFormPartial.cshtml", model);
        }

        public async Task<IActionResult> DownloadProjectFile(string id)
        {
            var fileInfo = await _fileInfoRepository.GetAsync(id);

            var fileStream = await _fileRepository.GetProjectFile(id);
            return File(fileStream, fileInfo.ContentType, fileInfo.FileName);
        }

        private async Task DoVoteFor(string id)
        {
            var user = GetAuthenticatedUser();

            var result = new ProjectVoteEntity
            {
                ProjectId = id,
                VoterUserId = user.Email,
                ForAgainst = 1,
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
            };

            var vote = await _voteRepository.GetAsync(id, user.Email);

            if (vote == null)
            {
                await _voteRepository.SaveAsync(result);

                var project = await _projectRepository.GetAsync(id);

                project.VotesFor += 1;

                await _projectRepository.UpdateAsync(project);
            }
            else
            {
                await _voteRepository.UpdateAsync(result);
                if (vote.ForAgainst == -1)
                {
                    var project = await _projectRepository.GetAsync(id);

                    project.VotesFor += 1;
                    project.VotesAgainst -= 1;

                    await _projectRepository.UpdateAsync(project);
                }
            }
        }

        private async Task DoVoteAgainst(string id)
        {
            var user = GetAuthenticatedUser();

            var result = new ProjectVoteEntity
            {
                ProjectId = id,
                VoterUserId = user.Email,
                ForAgainst = -1,
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
            };

            var vote = await _voteRepository.GetAsync(id, user.Email);

            if (vote == null)
            {
                await _voteRepository.SaveAsync(result);

                var project = await _projectRepository.GetAsync(id);

                project.VotesAgainst += 1;

                await _projectRepository.UpdateAsync(project);
            }
            else
            {
                await _voteRepository.UpdateAsync(result);
                if (vote.ForAgainst == 1)
                {
                    var project = await _projectRepository.GetAsync(id);

                    project.VotesAgainst += 1;
                    project.VotesFor -= 1;

                    await _projectRepository.UpdateAsync(project);
                }
            }
        }

        public async Task<IActionResult> VoteFor(string projectId)
        {
            await DoVoteFor(projectId);

            return await ProjectVotingBarsPartial(projectId);
        }

        public async Task<IActionResult> VoteAgainst(string projectId)
        {
            await DoVoteAgainst(projectId);

            return await ProjectVotingBarsPartial(projectId);
        }

        private async Task<IActionResult> ProjectVotingBarsPartial(string projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            var viewModel = new ProjectVoteViewModel
            {
                VotesFor = project.VotesFor,
                VotesAgainst = project.VotesAgainst
            };

            return PartialView("~/Views/Project/VotingBarsPartial.cshtml", viewModel);
        }

        [Authorize]
        public async Task<IActionResult> AddParticipant(string id)
        {
            var user = GetAuthenticatedUser();

            var participant = await _participantsRepository.GetAsync(id, user.Email);

            if (participant == null)
            {
                var viewModel = new ProjectParticipateViewModel
                {
                    ProjectId = id,
                    UserId = user.Email,
                    FullName = user.GetFullName(),
                    Registered = DateTime.UtcNow,
                    Result = false,
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                };

                await _participantsRepository.SaveAsync(viewModel);

                var project = await _projectRepository.GetAsync(id);

                project.ParticipantsCount += 1;

                await _projectRepository.UpdateAsync(project);
            }

            return RedirectToAction("ProjectDetails", "Project", new { id = id, participantAdded = true });
        }

        [Authorize]
        public async Task<IActionResult> RemoveParticipant(string id)
        {
            var user = GetAuthenticatedUser();

            var participant = await _participantsRepository.GetAsync(id, user.Email);

            if (participant != null)
            {
                await _participantsRepository.DeleteAsync(id, user.Email);

                var project = await _projectRepository.GetAsync(id);

                project.ParticipantsCount -= 1;

                await _projectRepository.UpdateAsync(project);
            }

            return RedirectToAction("ProjectDetails", "Project", new { id });
        }

        [Authorize]
        public IActionResult AddResult(string id)
        {
            var viewModel = new AddResultViewModel
            {
                ProjectId = id,
                ParticipantId = GetAuthenticatedUser().Email
            };
            return View("~/Views/Project/AddResult.cshtml", viewModel);
        }

        [Authorize]
        public async Task<IActionResult> EditResult(EditResultViewModel model)
        {
            var result = await _resultRepository.GetAsync(model.ProjectId, model.UserId);

            var viewModel = new AddResultViewModel
            {
                ProjectId = model.ProjectId,
                ParticipantId = model.UserId,
                Link = result.Link
            };

            return View("~/Views/Project/AddResult.cshtml", viewModel);
        }

        [Authorize]
        public async Task<IActionResult> SaveResult(AddResultViewModel model)
        {
            var participant = await _participantsRepository.GetAsync(model.ProjectId, model.ParticipantId);

            if (participant == null) return View("AccessDenied");

            var result = await _resultRepository.GetAsync(model.ProjectId, model.ParticipantId);

            model.ParticipantFullName = participant.FullName;

            model.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            if (result == null)
            {
                model.Score = 0;
                model.Submitted = DateTime.UtcNow;

                await _resultRepository.SaveAsync(model);

                participant.Result = true;

                await _participantsRepository.UpdateAsync(participant);
            }
            else
            {
                model.Score = result.Score;
                model.Submitted = result.Submitted;
                model.Votes = result.Votes;

                await _resultRepository.UpdateAsync(model);
            }

            return RedirectToAction("ProjectDetails", "Project", new { id = model.ProjectId });
        }

        [Authorize]
        public async Task<IActionResult> VoteForResult(ResultVoteViewModel model)
        {
            if (model.ProjectId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var project = await _projectRepository.GetAsync(model.ProjectId);
            var voterId = GetAuthenticatedUser().Email;

            model.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            string voteType = null;
            var role = await _userRolesRepository.GetAsync(voterId);

            if (voterId == project.AuthorId)
            {
                voteType = "AUTHOR";
            }
            else if (role != null && role.Role == "ADMIN")
            {
                voteType = "ADMIN";
            }

            var vote = await _resultVoteRepository.GetAsync(model.ProjectId, model.ParticipantId, voterId);

            if (vote == null)
            {
                model.VoterUserId = voterId;
                model.Type = voteType;

                await _resultVoteRepository.SaveAsync(model);
                var votes = await _resultVoteRepository.GetProjectResultVotesAsync(model.ProjectId);

                var totalVotes = votes.Count();
                var result = await _resultRepository.GetAsync(model.ProjectId, model.ParticipantId);

                result.Votes += 1;

                await _resultRepository.UpdateAsync(result);
                await CalculateScores(totalVotes, model.ProjectId);

                return RedirectToAction("ProjectDetails", "Project", new { id = model.ProjectId, resultsActive = true, votedForResult = true });
            }
            else
            {
                await _resultVoteRepository.DeleteAsync(model.ProjectId, model.ParticipantId, voterId);
                var votes = await _resultVoteRepository.GetProjectResultVotesAsync(model.ProjectId);

                var totalVotes = votes.Count();
                var result = await _resultRepository.GetAsync(model.ProjectId, model.ParticipantId);

                result.Votes -= 1;

                await _resultRepository.UpdateAsync(result);
                await CalculateScores(totalVotes, model.ProjectId);

                return RedirectToAction("ProjectDetails", "Project", new { id = model.ProjectId, resultsActive = true, votedTwice = true });
            }
        }

        [Authorize]
        public async Task<IActionResult> FollowProject(string id)
        {
            var user = GetAuthenticatedUser();

            var project = await _projectRepository.GetAsync(id);

            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            project.Status = StatusHelper.GetProjectStatusFromString(project.ProjectStatus);

            var sentMails = await _mailSentRepository.GetFollowAsync();

            var match = sentMails.FirstOrDefault(x => x.ProjectId == id && x.UserId == user.Email);

            if (project.Status == Status.Initiative && match == null)
            {
                var initiativeMessage = NotificationMessageHelper.GenerateInitiativeMessage(project, user.GetFullName(),
                    user.Email);
                await _emailsQueue.PutMessageAsync(initiativeMessage);
                await _mailSentRepository.SaveFollowAsync(user.Email, id);
            }

            var follow = await _projectFollowRepository.GetAsync(user.Email, id);

            if (follow == null)
                await _projectFollowRepository.SaveAsync(
                    new ProjectFollowEntity { UserId = user.Email, FullName = user.GetFullName(), ProjectId = id, UserAgent = userAgent });

            return RedirectToAction("ProjectDetails", "Project", new { id });
        }

        [Authorize]
        public async Task<IActionResult> UnFollowProject(string id)
        {
            var user = GetAuthenticatedUser();

            await _projectFollowRepository.DeleteAsync(user.Email, id);

            return RedirectToAction("ProjectDetails", "Project", new { id });
        }

        private async Task CalculateScores(int totalVotes, string projectId)
        {
            var results = await _resultRepository.GetResultsAsync(projectId);

            foreach (var result in results)
            {
                if (totalVotes == 0)
                {
                    result.Score = 0;
                }
                else
                {
                    result.Score = result.Votes * 100 / totalVotes;
                }
                await _resultRepository.UpdateAsync(result);
            }
        }

        [Authorize]
        public async Task<IActionResult> RemoveComment(string projectId, string commentId)
        {
            var user = GetAuthenticatedUser();

            var userRole = (user.Email == null) ? null : await _userRolesRepository.GetAsync(user.Email.ToLower());

            var isAdmin = (userRole != null) && userRole.Role == "ADMIN";

            if (isAdmin)
            {
                var projectComments = await _commentsRepository.GetProjectCommentsAsync(projectId);

                await _commentsRepository.DeleteAsync(projectId, commentId);

                foreach (var comment in projectComments)
                {
                    if (comment.ParentId == commentId)
                        await _commentsRepository.DeleteAsync(projectId, comment.Id);
                }
            }

            return RedirectToAction("ProjectDetails", "Project", new { commentsActive = true, id = projectId });
        }

        [Authorize]
        public async Task<IActionResult> SaveWinner(string projectId, string winnerId, string fullName, int place, string result, double budget)
        {
            if (!await CurrentUserIsAdmin()) return View("AccessDenied");

            var existingWinner = await _winnersRepository.GetAsync(projectId, winnerId);

            var winner = new WinnerViewModel
            {
                ProjectId = projectId,
                WinnerId = winnerId,
                FullName = fullName,
                Place = place,
                Result = result,
                Budget = budget
            };

            if (existingWinner == null)
            {
                await _winnersRepository.SaveAsync(winner);
            }
            else
            {
                await _winnersRepository.UpdateAsync(winner);
            }

            return RedirectToAction("ProjectDetails", "Project", new { winnersActive = true, id = projectId });
        }

        [Authorize]
        public async Task<IActionResult> RemoveWinner(string projectId, string winnerId)
        {
            if (!await CurrentUserIsAdmin()) return View("AccessDenied");

            await _winnersRepository.DeleteAsync(projectId, winnerId);

            return RedirectToAction("ProjectDetails", "Project", new { winnersActive = true, id = projectId });
        }

        private async Task<bool> CurrentUserIsAdmin()
        {
            var user = GetAuthenticatedUser();

            var userRole = (user.Email == null) ? null : await _userRolesRepository.GetAsync(user.Email.ToLower());

            var isAdmin = (userRole != null) && userRole.Role == "ADMIN";

            return isAdmin;
        }

        private CompetitionPlatformUser GetAuthenticatedUser()
        {
            return ClaimsHelper.GetUser(User.Identity);
        }
    }
}