using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;
using CompetitionPlatform.Data.AzureRepositories.Expert;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.ProjectStream;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Settings;
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
using Ganss.XSS;
using Lykke.Service.PersonalData.Contract;

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
        private readonly IQueueExt _emailsQueue;
        private readonly IProjectResultVoteRepository _resultVoteRepository;
        private readonly BaseSettings _settings;
        private readonly ILog _log;
        private readonly IProjectExpertsRepository _projectExpertsRepository;
        private readonly IStreamRepository _streamRepository;
        private readonly IPersonalDataService _personalDataService;
        private readonly Lykke.Messages.Email.IEmailSender _emailSender;
        private readonly IStreamsIdRepository _streamsIdRepository;

        public ProjectController(IProjectRepository projectRepository, IProjectCommentsRepository commentsRepository,
            IProjectFileRepository fileRepository, IProjectFileInfoRepository fileInfoRepository,
            IProjectParticipantsRepository participantsRepository, IProjectCategoriesRepository categoriesRepository,
            IProjectResultRepository resultRepository, IProjectFollowRepository projectFollowRepository,
            IProjectWinnersRepository winnersRepository, IUserRolesRepository userRolesRepository,
            IProjectWinnersService winnersService, IQueueExt emailsQueue,
            IProjectResultVoteRepository resultVoteRepository, BaseSettings settings,
            ILog log, IProjectExpertsRepository projectExpertsRepository,
            IStreamRepository streamRepository, IPersonalDataService personalDataService,
            Lykke.Messages.Email.IEmailSender emailSender,
            IStreamsIdRepository streamsIdRepository)
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
            _settings = settings;
            _log = log;
            _projectExpertsRepository = projectExpertsRepository;
            _streamRepository = streamRepository;
            _personalDataService = personalDataService;
            _emailSender = emailSender;
            _streamsIdRepository = streamsIdRepository;
        }


        // Create a new project
        [Authorize]
        public async Task<IActionResult> Create()
        {
            // Fetch user and role
            // TODO: These should be methods on the CompetitionPlatformUser model class
            // which should probably be broken out from IdentityModels
            var user = UserModel.GetAuthenticatedUser(User.Identity);
            var userRole = await _userRolesRepository.GetAsync(user.Email.ToLower());


            ViewBag.ProjectCategories = _categoriesRepository.GetCategories();

            // TODO: Move these checks into a bool CanCreateProject() or IsVerified() on the CompetitionPlatformUser
            // avoiding these if trees makes it much easier to test and modify in the future
            // plus it's used everywhere so we can DRY the code up
            if (userRole != null)
            {
                return View("CreateProject");
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                var kycStatus = await GetUserKycStatus(user.Email);

                if (kycStatus == "\"Ok\"")
                {
                    return View("CreateProject");
                }
            }

            return View("CreateClosed");
        }

        // Edit a project
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            var user = UserModel.GetAuthenticatedUser(User.Identity);

            var viewModel = await GetProjectViewModel(id);
            viewModel.IsAuthor = viewModel.AuthorId == user.Email;

            // TODO: move to a bool CanUserEditProject(
            if (viewModel.IsAdmin)
            {
                return View("EditProject", viewModel);
            }
            if ((viewModel.Status == Status.Initiative || viewModel.Status == Status.Draft) && viewModel.AuthorId == user.Email)
            {
                return View("EditProject", viewModel);
            }

            return View("AccessDenied");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveProject(ProjectViewModel projectViewModel, bool draft = false, bool enableVoting = false, bool enableRegistration = false)
        {

            // TODO: We should re-check if we can do enums, or some workaround - the serialization/deserialization everywhere
            // is definitely a surface for bugs to hide in.

            projectViewModel.Tags = SerializeTags(projectViewModel.Tags);
            projectViewModel.ProjectStatus = projectViewModel.Status.ToString();

            // TODO: Thinking about a Sanitize() method on the ProjectViewModel right before we write to DB,
            // that way we get every possible user-generated field and don't need to worry about missing one
            // (e.g. what if you forgot to do the PrizeDescription here?)
            var sanitizer = new HtmlSanitizer();
            projectViewModel.PrizeDescription = sanitizer.Sanitize(projectViewModel.PrizeDescription);
            projectViewModel.Description = sanitizer.Sanitize(projectViewModel.Description);

            // TODO: Switch the name of the fields in the projectViewModel so it
            // is consistent, using the ! operator is difficult to read and reason about here
            projectViewModel.SkipVoting = !enableVoting;
            projectViewModel.SkipRegistration = !enableRegistration;

            // TODO: What's going on in these two if statements? Probably needs a comment, at least, if not a refactor.
            if (projectViewModel.CompetitionRegistrationDeadline == DateTime.MinValue)
                projectViewModel.CompetitionRegistrationDeadline = DateTime.UtcNow.Date;

            if (projectViewModel.VotingDeadline == DateTime.MinValue)
                projectViewModel.VotingDeadline = DateTime.UtcNow.Date;

            // TODO: Move to a separate validation function for testing, regexes are notorious.
            // Also, is the ID field the 'project URL' on the frontend? Should probably be changed, I found
            // it quite confusing.
            var idValid = Regex.IsMatch(projectViewModel.Id, @"^[a-z0-9-]+$") && !string.IsNullOrEmpty(projectViewModel.Id);

            if (!idValid)
            {
                ViewBag.ProjectCategories = _categoriesRepository.GetCategories();
                ModelState.AddModelError("Id", "Project Url can only contain lowercase letters, numbers and the dash symbol and cannot be empty!");
                return View("CreateProject", projectViewModel);
            }

            var project = await _projectRepository.GetAsync(projectViewModel.Id);

            if (project == null)
            {
                // TODO: Put this entire if-block into a constructor or a factory method on ProjectViewModel?
                projectViewModel.Status = draft ? Status.Draft : Status.Initiative;

                var user = UserModel.GetAuthenticatedUser(User.Identity);
                var userRole = await _userRolesRepository.GetAsync(user.Email.ToLower());

                projectViewModel.AuthorId = user.Email;
                projectViewModel.AuthorFullName = user.GetFullName();
                projectViewModel.AuthorIdentifier = user.Id;

                //Don't let non-admin users create Initiative projects
                if (userRole == null || userRole.Role != "ADMIN")
                {
                    var kycStatus = await GetUserKycStatus(user.Email);
                    if (kycStatus == "\"Ok\"" && projectViewModel.Status != Status.Draft)
                    {
                        return View("CreateInitiativeClosed");
                    }
                }

                // TODO: Where is the user-agent used?
                projectViewModel.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                projectViewModel.Created = DateTime.UtcNow;
                projectViewModel.ParticipantsCount = 0;

                var projectId = await _projectRepository.SaveAsync(projectViewModel);

                // TODO: Move these into some kind of post-creation notification?
                // Especially because we don't want to notify in case of some problem
                if (projectViewModel.Status == Status.Initiative)
                {
                    await SendProjectCreateNotification(projectViewModel);
                }

                if (_emailsQueue != null)
                {
                    var message = NotificationMessageHelper.ProjectCreatedMessage(user.Email, user.GetFullName(),
                        projectViewModel.Name);
                    await _emailsQueue.PutMessageAsync(message);
                }

                // TODO: How is this different than the _projectRepository.SaveAsync?
                await SaveProjectFile(projectViewModel.File, projectId);
                UserModel.GenerateStreamsId(_streamsIdRepository, user.Id);

                return RedirectToAction("ProjectDetails", "Project", new { id = projectId });
            }

            // TODO: Move this to the validation function, then we can get rid of the if-statement 
            // and the async fetch above
            ViewBag.ProjectCategories = _categoriesRepository.GetCategories();
            ModelState.AddModelError("Id", "Project with that Project Url already exists!");
            return View("CreateProject", projectViewModel);
        }


        // TODO: This has a lot of duplicated code to SaveProject, rewrite to handle any
        // edit-specific logic then call SaveProject
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveEditedProject(ProjectViewModel projectViewModel, bool draft = false,
            bool enableVoting = false, bool enableRegistration = false)
        {
            if (projectViewModel.ProjectUrl == null)
                projectViewModel.ProjectUrl = projectViewModel.Id;

            projectViewModel.Tags = SerializeTags(projectViewModel.Tags);

            projectViewModel.ProjectStatus = projectViewModel.Status.ToString();

            var sanitizer = new HtmlSanitizer();
            projectViewModel.PrizeDescription = sanitizer.Sanitize(projectViewModel.PrizeDescription);
            projectViewModel.Description = sanitizer.Sanitize(projectViewModel.Description);

            projectViewModel.SkipVoting = !enableVoting;
            projectViewModel.SkipRegistration = !enableRegistration;
            if (projectViewModel.CompetitionRegistrationDeadline == DateTime.MinValue)
                projectViewModel.CompetitionRegistrationDeadline = DateTime.UtcNow.Date;

            if (projectViewModel.VotingDeadline == DateTime.MinValue)
                projectViewModel.VotingDeadline = DateTime.UtcNow.Date;

            var project = await _projectRepository.GetAsync(projectViewModel.Id);

            if (projectViewModel.AuthorId == null)
                projectViewModel.AuthorId = project.AuthorId;

            if (projectViewModel.AuthorFullName == null)
                projectViewModel.AuthorFullName = project.AuthorFullName;

            if (projectViewModel.AuthorIdentifier == null)
                projectViewModel.AuthorIdentifier = project.AuthorIdentifier;

            project.Status = StatusHelper.GetProjectStatusFromString(project.ProjectStatus);

            var user = UserModel.GetAuthenticatedUser(User.Identity);
            var userRole = await _userRolesRepository.GetAsync(user.Email.ToLower());

            //Don't let non-admin users edit their draft projects to Initiative status
            if (userRole == null || userRole.Role != "ADMIN")
            {
                var kycStatus = await GetUserKycStatus(user.Email);
                if (kycStatus == "\"Ok\"" && projectViewModel.Status != Status.Draft)
                {
                    return View("CreateInitiativeClosed");
                }
            }

            projectViewModel.LastModified = DateTime.UtcNow;

            projectViewModel.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            projectViewModel.ParticipantsCount = project.ParticipantsCount;

            var projectId = projectViewModel.Id;
            var statusAndUrlChanged = projectViewModel.Status != Status.Draft &&
                                      projectViewModel.ProjectUrl != projectId;

            // TODO: This definitely needs some love. Not totally sure what's happening here, we should dissect
            // but it's definitely too complicated
            if (!statusAndUrlChanged)
            {
                var currentProjectIsInStream = false;
                var currentProjectWasInOldStream = false;
                var streamId = "";

                if (projectViewModel.StreamType == "New")
                {
                    var streamProjects = JsonConvert.DeserializeObject<List<StreamProject>>(projectViewModel.SerializedStream);

                    if (streamProjects.Any())
                    {
                        var newStream = new StreamEntity
                        {
                            Name = projectViewModel.NewStreamName,
                            AuthorId = user.Id,
                            AuthorEmail = user.Email,
                            Stream = projectViewModel.SerializedStream
                        };

                        streamId = await _streamRepository.SaveAsync(newStream);

                        foreach (var proj in streamProjects)
                        {
                            var streamProject = await _projectRepository.GetAsync(proj.ProjectId);
                            streamProject.StreamId = streamId;
                            await _projectRepository.UpdateAsync(streamProject);
                        }

                        currentProjectIsInStream = streamProjects.Any(p => p.ProjectId == projectViewModel.Id);
                    }
                }
                if (projectViewModel.StreamType == "Existing")
                {
                    var existingStream = await _streamRepository.GetAsync(projectViewModel.ExistingStreamId);
                    var oldProjects = JsonConvert.DeserializeObject<List<StreamProject>>(existingStream.Stream);
                    currentProjectWasInOldStream = oldProjects.Any(p => p.ProjectId == projectViewModel.Id);

                    foreach (var oldProj in oldProjects)
                    {
                        var oldProject = await _projectRepository.GetAsync(oldProj.ProjectId);
                        oldProject.StreamId = null;
                        await _projectRepository.UpdateAsync(oldProject);
                    }

                    existingStream.Stream = projectViewModel.SerializedStream;
                    await _streamRepository.UpdateAsync(existingStream);

                    var newProjects = JsonConvert.DeserializeObject<List<StreamProject>>(projectViewModel.SerializedStream);

                    foreach (var newProj in newProjects)
                    {
                        var streamProject = await _projectRepository.GetAsync(newProj.ProjectId);
                        streamProject.StreamId = existingStream.Id;
                        await _projectRepository.UpdateAsync(streamProject);
                    }

                    currentProjectIsInStream = newProjects.Any(p => p.ProjectId == projectViewModel.Id);
                    streamId = existingStream.Id;
                }

                if (currentProjectIsInStream)
                {
                    projectViewModel.StreamId = streamId;
                }
                else if (currentProjectWasInOldStream)
                {
                    projectViewModel.StreamId = null;
                }
                await _projectRepository.UpdateAsync(projectViewModel);
            }


            if (project.Status == Status.Draft && projectViewModel.Status == Status.Initiative)
            {
                await SendProjectCreateNotification(projectViewModel);
            }

            var idValid = false;

            if (!string.IsNullOrEmpty(projectViewModel.ProjectUrl))
            {
                idValid = Regex.IsMatch(projectViewModel.ProjectUrl, @"^[a-z0-9-]+$");
            }
            else
            {
                return await EditWithProjectUrlError(projectViewModel.Id, "Project Url cannot be empty!");
            }

            if (!idValid)
            {
                return await EditWithProjectUrlError(projectViewModel.Id, "Project Url can only contain lowercase letters, numbers and the dash symbol!");
            }

            if (projectViewModel.ProjectUrl != projectId)
            {
                if (projectViewModel.Status != Status.Draft)
                {
                    var oldProjUrl = projectViewModel.ProjectUrl;
                    projectViewModel = await GetProjectViewModel(projectId);
                    projectViewModel.ProjectUrl = oldProjUrl;
                    projectViewModel.ProjectCategories = _categoriesRepository.GetCategories();
                    ModelState.AddModelError("Status", "Status cannot be changed while changing Project Url!");
                    return View("EditProject", projectViewModel);
                }

                if (!string.IsNullOrEmpty(projectViewModel.ProjectUrl))
                {
                    idValid = Regex.IsMatch(projectViewModel.ProjectUrl, @"^[a-z0-9-]+$");
                }
                else
                {
                    return await EditWithProjectUrlError(projectViewModel.Id, "Project Url cannot be empty!");
                }

                if (!idValid)
                {
                    return await EditWithProjectUrlError(projectViewModel.Id, "Project Url can only contain lowercase letters, numbers and the dash symbol!");
                }

                var projectExists = await _projectRepository.GetAsync(projectViewModel.ProjectUrl);

                if (projectExists != null)
                {
                    return await EditWithProjectUrlError(projectViewModel.Id, "Project with that Project Url already exists!");
                }

                projectViewModel.Id = projectViewModel.ProjectUrl;
                projectViewModel.Created = DateTime.UtcNow;

                await _projectRepository.SaveAsync(projectViewModel);
                await _projectRepository.DeleteAsync(projectId);

                return RedirectToAction("ProjectDetails", "Project", new { id = projectViewModel.ProjectUrl });
            }

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

            if (projectViewModel.Status == Status.Archive) //project.Status != Status.Archive && 
            {
                if (!project.SkipVoting)
                {
                    await _winnersService.SaveWinners(projectViewModel.Id, projectViewModel.Winners);
                }
                else
                {
                    await _winnersService.SaveCustomWinners(projectViewModel.Id, projectViewModel.Winners);
                }

                await AddArchiveMailToQueue(project);
            }

            await SaveProjectFile(projectViewModel.File, projectId);

            return RedirectToAction("ProjectDetails", "Project", new { id = projectViewModel.Id });
        }

        // TODO: These notification tasks should probably be done elsewhere -
        // I believe I saw that we've got additional requirements around notifications
        // coming up, so maybe a separate notification controller is in order
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
        // TODO: Move to the new ProjectModel, which will contain details about the project. ProjectViewModel is just
        // the view-specific data
        private async Task<IEnumerable<IProjectFollowData>> GetProjectFollows(string projectId)
        {
            var follows = await _projectFollowRepository.GetFollowAsync();
            var projectFollows = follows.Where(f => f.ProjectId == projectId).ToList();
            return projectFollows;
        }

        // TODO: Name this something more descriptive - looks like it's filling in the view-specific data for the project?
        public async Task<IActionResult> ProjectDetails(string id, bool commentsActive = false, bool participantsActive = false, bool resultsActive = false, bool winnersActive = false)
        {
            if (TempData["ShowParticipantAddedModal"] != null)
            {
                ViewBag.ParticipantAdded = (bool)TempData["ShowParticipantAddedModal"];
            }

            if (TempData["ShowVotedForResultModal"] != null)
            {
                ViewBag.VotedForResult = (bool)TempData["ShowVotedForResultModal"];
            }

            if (TempData["ShowVotedTwiceModal"] != null)
            {
                ViewBag.VotedTwice = (bool)TempData["ShowVotedTwiceModal"];
            }

            // TODO: these can just be ViewBag.CommentsActive = commentsActive
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

            var projectExists = await _projectRepository.GetAsync(id);

            if (projectExists == null)
            {
                return View("ProjectNotFound");
            }

            var viewModel = await GetProjectViewModel(id);
            ViewBag.FacebookShareDescription = viewModel.Overview;

            return View(viewModel);
        }
        // TODO: A lot of this, like the fetch methods should probably be its own ProjectModel class - it's really about the 
        // Project, not just the ProjectView and anything that's related to the view only (like the projectCategories) 
        // can get pushed into the separate ProjectViewModel class that has a ProjectModel property on it.
        private async Task<ProjectViewModel> GetProjectViewModel(string id)
        {
            var projectCategories = _categoriesRepository.GetCategories();

            var project = await _projectRepository.GetAsync(id);

            project.Status = StatusHelper.GetProjectStatusFromString(project.ProjectStatus);
            var projectDetailsAvatarIds = new List<string>();
            projectDetailsAvatarIds.Add(project.AuthorIdentifier);

            // TODO: And these type of additional fetch methods can be methods on the model - the more we break
            // it up, the easier it is to test and reuse. 
            var comments = await _commentsRepository.GetProjectCommentsAsync(id);

            // TODO: Definitely should be a FormatComments()
            foreach (var comment in comments)
            {
                if (string.IsNullOrEmpty(comment.UserIdentifier))
                {
                    comment.UserIdentifier = await ClaimsHelper.GetUserIdByEmail(_settings.LykkeStreams.Authentication.Authority,
                        _settings.LykkeStreams.Authentication.ClientId, comment.UserId);
                    await _commentsRepository.UpdateAsync(comment, id);
                }

                projectDetailsAvatarIds.Add(comment.UserIdentifier);
                var commenterStreamsId = await _streamsIdRepository.GetOrCreateAsync(comment.UserIdentifier);
                comment.StreamsId = commenterStreamsId.StreamsId;

                if (!string.IsNullOrEmpty(comment.Comment))
                {
                    comment.Comment = Regex.Replace(comment.Comment, @"\r\n?|\n", "<br />");
                }
            }

            var participants = await _participantsRepository.GetProjectParticipantsAsync(id);

            var results = await _resultRepository.GetResultsAsync(id);

            var user = UserModel.GetAuthenticatedUser(User.Identity);

            var participant = (user.Email == null) ? null : await _participantsRepository.GetAsync(id, user.Email);

            var userRole = (user.Email == null) ? null : await _userRolesRepository.GetAsync(user.Email.ToLower());

            var isAdmin = (userRole != null) && userRole.Role == StreamsRoles.Admin;
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

            // TODO: As I go through this, wondering if we need a CommentsList model, especially since 
            // comments are probably going to be important to the platform going forwards
            comments = SortComments(comments);

            var commenterIsModerator = new Dictionary<string, bool>();

            foreach (var comment in comments)
            {
                var role = await _userRolesRepository.GetAsync(comment.UserId);
                var isModerator = role != null && role.Role == StreamsRoles.Admin;
                commenterIsModerator.Add(comment.Id, isModerator);
            }

            // TODO: The votes might also need to be broken out, especially if we want to do
            // 5-star or numeric scoring
            var userVotedForResults = new Dictionary<string, bool>();
            var resultVotes = await _resultVoteRepository.GetProjectResultVotesAsync(project.Id);

            foreach (var part in participants)
            {
                if (string.IsNullOrEmpty(part.UserIdentifier))
                {
                    part.UserIdentifier = await ClaimsHelper.GetUserIdByEmail(_settings.LykkeStreams.Authentication.Authority,
                        _settings.LykkeStreams.Authentication.ClientId, part.UserId);
                    await _participantsRepository.UpdateAsync(part);
                }

                projectDetailsAvatarIds.Add(part.UserIdentifier);
                var participantStreamsId = await _streamsIdRepository.GetOrCreateAsync(part.UserIdentifier);
                part.StreamsId = participantStreamsId.StreamsId;
            }

            foreach (var result in results)
            {
                if (string.IsNullOrEmpty(result.ParticipantIdentifier))
                {
                    result.ParticipantIdentifier = await ClaimsHelper.GetUserIdByEmail(_settings.LykkeStreams.Authentication.Authority,
                        _settings.LykkeStreams.Authentication.ClientId, result.ParticipantId);
                    await _resultRepository.UpdateAsync(result);
                }

                var match =
                    resultVotes.FirstOrDefault(x => x.ParticipantId == result.ParticipantId && x.VoterUserId == user.Email);

                userVotedForResults.Add(result.ParticipantId, match != null && user.Email != null);
                var resultStreamsId = await _streamsIdRepository.GetOrCreateAsync(result.ParticipantIdentifier);
                result.StreamsId = resultStreamsId.StreamsId;
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
                UserId = user.Email,
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
                UserVotedForResults = userVotedForResults,
                SubmissionsDeadline = project.ImplementationDeadline
            };

            var experts = await _projectExpertsRepository.GetProjectExpertsAsync(id);

            foreach (var expert in experts)
            {
                if (string.IsNullOrEmpty(expert.UserIdentifier) && !string.IsNullOrEmpty(expert.UserId))
                {
                    expert.UserIdentifier = await ClaimsHelper.GetUserIdByEmail(_settings.LykkeStreams.Authentication.Authority,
                         _settings.LykkeStreams.Authentication.ClientId, expert.UserId);
                    await _projectExpertsRepository.UpdateAsync(expert);
                }
                if (!string.IsNullOrEmpty(expert.UserIdentifier))
                {
                    projectDetailsAvatarIds.Add(expert.UserIdentifier);
                    UserModel.GenerateStreamsId(_streamsIdRepository, expert.UserIdentifier);
                }
            }

            var avatarsDictionary = await _personalDataService.GetClientAvatarsAsync(projectDetailsAvatarIds);
            participantsPartial.Avatars = avatarsDictionary;
            commentsPartial.Avatars = avatarsDictionary;
            resultsPartial.Avatars = avatarsDictionary;

            experts = experts.OrderBy(x => x.Priority == 0).ThenBy(x => x.Priority);
            var authorStreamsId = await _streamsIdRepository.GetOrCreateAsync(project.AuthorIdentifier);

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
                AuthorIdentifier = project.AuthorIdentifier,
                ParticipantId = participantId,
                IsParticipant = isParticipant,
                IsAdmin = isAdmin,
                IsFollowing = isFollowing,
                OtherProjects = await GetOtherProjects(project.Id),
                ProgrammingResourceName = project.ProgrammingResourceName,
                ProgrammingResourceLink = project.ProgrammingResourceLink,
                SkipVoting = project.SkipVoting,
                SkipRegistration = project.SkipRegistration,
                ProjectExperts = !experts.Any() ? null : experts,
                PrizeDescription = project.PrizeDescription,
                StreamId = project.StreamId,
                AllStreamProjects = await GetStreamProjects(),
                CompactStreams = await GetCompactStreams(),
                NameTag = project.NameTag,
                AuthorAvatarUrl = avatarsDictionary[project.AuthorIdentifier],
                AuthorStreamsId = authorStreamsId.StreamsId
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

            projectViewModel.EditStreamProjects = new EditStreamProjects { ProjectsList = new List<StreamProject>() };
            if (!string.IsNullOrEmpty(project.StreamId))
            {
                var stream = await _streamRepository.GetAsync(project.StreamId);
                projectViewModel.StreamProjects = JsonConvert.DeserializeObject<List<StreamProject>>(stream.Stream);
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
            {
                projectViewModel = await PopulateResultsViewModel(projectViewModel);
                var winners = await _winnersRepository.GetWinnersAsync(project.Id);
                projectViewModel.Winners = winners.Select(x => WinnerViewModel.Create(x)).ToList();
            }

            projectViewModel.ProjectUrl = project.Id;

            return projectViewModel;
        }

        private async Task<List<OtherProjectViewModel>> GetOtherProjects(string id)
        {
            var projects = await _projectRepository.GetProjectsAsync();
            var filteredProjects = projects.Where(x => x.Id != id && x.ProjectStatus == Status.Submission.ToString() && x.BudgetFirstPlace > 0).Take(7).ToList();

            var otherProjects = new List<OtherProjectViewModel>();

            foreach (var project in filteredProjects)
            {
                var otherProject = new OtherProjectViewModel
                {
                    Id = project.Id,
                    Name = project.Name,
                    BudgetFirstPlace = project.BudgetFirstPlace,
                    BudgetSecondPlace = project.BudgetSecondPlace,
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
            model.ResultsPartial.DaysOfContest = (model.VotingDeadline - model.Created).Days;

            var winnersList = await _winnersRepository.GetWinnersAsync(model.Id);

            winnersList = winnersList.OrderBy(x => x.Place).ThenByDescending(x => x.Votes).ThenByDescending(x => x.Score);

            foreach (var winner in winnersList)
            {
                if (string.IsNullOrEmpty(winner.WinnerIdentifier))
                {
                    winner.ProjectId = model.Id;
                    winner.WinnerIdentifier = await ClaimsHelper.GetUserIdByEmail(_settings.LykkeStreams.Authentication.Authority,
                        _settings.LykkeStreams.Authentication.ClientId, winner.WinnerId);
                    await _winnersRepository.UpdateAsync(winner);
                }
                if (!string.IsNullOrEmpty(winner.WinnerIdentifier))
                {
                    UserModel.GenerateStreamsId(_streamsIdRepository, winner.WinnerIdentifier);

                    var winnerStreamsId = await _streamsIdRepository.GetOrCreateAsync(winner.WinnerIdentifier);
                    winner.StreamsId = winnerStreamsId.StreamsId;
                }
            }

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

        private async Task<string> GetUserKycStatus(string email)
        {
            var authLink = _settings.LykkeStreams.Authentication.Authority;
            var appId = _settings.LykkeStreams.Authentication.ClientId;

            var webRequest = (HttpWebRequest)WebRequest.Create(authLink + "/getkycstatus?email=" + email);
            webRequest.Method = "GET";
            webRequest.ContentType = "text/html";
            webRequest.Headers["application_id"] = appId;
            var webResponse = await webRequest.GetResponseAsync();

            using (var receiveStream = webResponse.GetResponseStream())
            {
                using (var sr = new StreamReader(receiveStream))
                {
                    return await sr.ReadToEndAsync();
                }

            }
        }

        private async Task<IActionResult> EditWithProjectUrlError(string projectId, string errorText)
        {
            var projectViewModel = await GetProjectViewModel(projectId);
            projectViewModel.ProjectCategories = _categoriesRepository.GetCategories();
            ModelState.AddModelError("ProjectUrl", errorText);
            return View("EditProject", projectViewModel);
        }

        private async Task SendProjectCreateNotification(IProjectData model)
        {
            var message = new Lykke.Messages.Email.MessageData.PlainTextData
            {
                Sender = "Lykke Notifications",
                Text = "New Project was created. Project name - " + model.Name + ", Project author - " + model.AuthorFullName +
                ", Project Link - https://streams.lykke.com/Project/ProjectDetails/" + model.Id,
                Subject = "New Project Created!"
            };

            foreach (var email in _settings.LykkeStreams.ProjectCreateNotificationReceiver)
            {
                await _emailSender.SendEmailAsync("Lykke", email, message);
            }
        }

        private async Task<List<StreamProject>> GetStreamProjects()
        {
            var projects = await _projectRepository.GetProjectsAsync();
            return (from project in projects
                    where project.ProjectStatus != Status.Draft.ToString() && string.IsNullOrEmpty(project.StreamId)
                    select new StreamProject
                    {
                        ProjectId = project.Id,
                        ProjectName = project.Name
                    }).ToList();
        }

        private async Task<List<CompactStream>> GetCompactStreams()
        {
            var streams = await _streamRepository.GetStreamsAsync();

            return streams.Select(stream => new CompactStream
            {
                StreamId = stream.Id,
                StreamName = stream.Name
            })
                .ToList();
        }

        public async Task<IActionResult> GetEditStreamsTable(string streamId)
        {
            var model = new EditStreamProjects { ProjectsList = new List<StreamProject>() };
            if (!string.IsNullOrEmpty(streamId))
            {
                var stream = await _streamRepository.GetAsync(streamId);
                var streamProjects = JsonConvert.DeserializeObject<List<StreamProject>>(stream.Stream);
                model.ProjectsList = streamProjects;
            }

            return PartialView("EditStreamTablePartial", model);
        }
    }
}