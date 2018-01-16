using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Settings;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Helpers;
using CompetitionPlatform.Models;
using CompetitionPlatform.Models.ProjectModels;
using CompetitionPlatform.Models.ProjectViewModels;
using CompetitionPlatform.Models.UserProfile;
using Lykke.Service.PersonalData.Contract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CompetitionPlatform.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly BaseSettings _settings;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectCommentsRepository _commentsRepository;
        private readonly IProjectParticipantsRepository _participantsRepository;
        private readonly IProjectResultRepository _resultsRepository;
        private readonly IProjectWinnersRepository _winnersRepository;
        private readonly IPersonalDataService _personalDataService;
        private readonly IProjectFollowRepository _projectFollowRepository;
        private readonly IUserRolesRepository _userRolesRepository;

        public UserProfileController(BaseSettings settings, IProjectRepository projectRepository, 
            IProjectCommentsRepository commentsRepository, IProjectParticipantsRepository participantsRepository, 
            IProjectResultRepository resultsRepository, IProjectWinnersRepository winnersRepository, 
            IPersonalDataService personalDataService, IProjectFollowRepository projectFollowRepository,
            IUserRolesRepository userRolesRepository)
        {
            _settings = settings;
            _projectRepository = projectRepository;
            _commentsRepository = commentsRepository;
            _participantsRepository = participantsRepository;
            _resultsRepository = resultsRepository;
            _winnersRepository = winnersRepository;
            _personalDataService = personalDataService;
            _projectFollowRepository = projectFollowRepository;
            _userRolesRepository = userRolesRepository;
        }

        private async Task<string> GetUserEmailById(string id)
        {
            var authLink = _settings.LykkeStreams.Authentication.Authority;
            var appId = _settings.LykkeStreams.Authentication.ClientId;

            var webRequest = (HttpWebRequest)WebRequest.Create(authLink + "/getemailbyid?id=" + id);
            webRequest.Method = "GET";
            webRequest.ContentType = "text/html";
            webRequest.Headers["application_id"] = appId;
            var webResponse = await webRequest.GetResponseAsync();

            using (var receiveStream = webResponse.GetResponseStream())
            {
                using (var sr = new StreamReader(receiveStream))
                {
                    var userId = await sr.ReadToEndAsync();
                    return JsonConvert.DeserializeObject(userId).ToString();
                }
            }
        }

        [HttpGet("~/userprofile/{id}")]
        public async Task<IActionResult> DisplayUserProfile(string id)
        {
            if (!IsGuid(id)) return View("ProfileNotFound");
            var email = await GetUserEmailById(id);
            var profile = await _personalDataService.GetProfilePersonalDataAsync(id);
            var user = GetAuthenticatedUser();

            if (profile.ClientId == user.Id && profile.FirstName != user.FirstName)
            {
                var newIdentity = ClaimsHelper.UpdateFirstNameClaim(User.Identity, profile.FirstName);
                var principal = new ClaimsPrincipal();
                principal.AddIdentity(newIdentity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }

            var commentsViewModel = new CommentsViewModel
            {
                CommentsList = await GetUserComments(email),
                CommentAvatar = profile.AvatarUrl
            };

            var userProfileViewModel = new UserProfileViewModel
            {
                Profile = profile,
                WinningsSum = await GetUserWinnigsSum(email),
                CreatedProjects = await GetCreatedProjects(email),
                ParticipatedProjects = await GetParticipatedProjects(email),
                WonProjects = await GetWonProjects(email),
                Comments = commentsViewModel,
                AuthLink = _settings.LykkeStreams.Authentication.Authority,
                IsLykkeMember = await UserIsLykkeMember(email)
            };

            return View("~/Views/UserProfile/UserProfile.cshtml", userProfileViewModel);
        }

        private async Task<List<ProjectCompactViewModel>> GetParticipatedProjects(string userId)
        {
            var participatedProjects = new List<IProjectData>();
            var projects = await _projectRepository.GetProjectsAsync();

            foreach (var project in projects)
            {
                if (project.ProjectStatus != Status.Draft.ToString())
                {
                    var participants = await _participantsRepository.GetProjectParticipantsAsync(project.Id);

                    foreach (var participant in participants)
                    {
                        if (participant.UserId == userId)
                        {
                            participatedProjects.Add(project);
                        }
                    }
                }
            }

            return await GetCompactProjectsList(participatedProjects);
        }

        private async Task<List<ProjectCompactViewModel>> GetWonProjects(string userId)
        {
            var wonProjects = new List<IProjectData>();

            var projects = await _projectRepository.GetProjectsAsync();

            foreach (var project in projects)
            {
                if (project.ProjectStatus != Status.Draft.ToString())
                {
                    var winners = await _winnersRepository.GetWinnersAsync(project.Id);

                    foreach (var winner in winners)
                    {
                        if (winner.WinnerId == userId)
                        {
                            wonProjects.Add(project);
                        }
                    }
                }
            }

            return await GetCompactProjectsList(wonProjects);
        }

        private async Task<List<ProjectCompactViewModel>> GetCreatedProjects(string userId)
        {
            var createdProjects = new List<IProjectData>();

            var projects = await _projectRepository.GetProjectsAsync();

            foreach (var project in projects)
            {
                if (project.ProjectStatus != Status.Draft.ToString())
                {
                    if (project.AuthorId == userId)
                    {
                        createdProjects.Add(project);
                    }
                }

            }

            return await GetCompactProjectsList(createdProjects);
        }

        private async Task<double> GetUserWinnigsSum(string email)
        {
            double sum = 0;

            var projects = await _projectRepository.GetProjectsAsync();

            foreach (var project in projects)
            {
                var winners = await _winnersRepository.GetWinnersAsync(project.Id);
                foreach (var winner in winners)
                {
                    if (winner.WinnerId == email)
                        sum += (double)winner.Budget;
                }
            }

            return sum;
        }

        private async Task<List<UserProfileCommentData>> GetUserComments(string email)
        {
            var userComments = new List<UserProfileCommentData>();

            var projects = await _projectRepository.GetProjectsAsync();

            foreach (var project in projects)
            {
                var comments = await _commentsRepository.GetProjectCommentsAsync(project.Id);

                foreach (var comment in comments)
                {
                    if (comment.UserId == email && !comment.Deleted)
                    {
                        userComments.Add(new UserProfileCommentData
                        {
                            ProjectName = project.Name,
                            ProjectId = project.Id,
                            Comment = comment.Comment,
                            FullName = comment.FullName,
                            LastModified = comment.LastModified
                        });
                    }
                }
            }

            return userComments;
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

                if (string.IsNullOrEmpty(project.AuthorIdentifier))
                {
                    project.AuthorIdentifier = await ClaimsHelper.GetUserIdByEmail(
                        _settings.LykkeStreams.Authentication.Authority, _settings.LykkeStreams.Authentication.ClientId,
                        project.AuthorId);
                    await _projectRepository.UpdateAsync(project);
                }

                var compactModel = new ProjectCompactViewModel
                {           
                    CommentsCount = projectCommentsCount,
                    ParticipantsCount = participantsCount,
                    ResultsCount = resultsCount,
                    WinnersCount = winnersCount,
                    Tags = tagsList,
                    BaseProjectData = project
                };

                if (!string.IsNullOrEmpty(project.ProjectStatus))
                {
                    compactModel.BaseProjectData.Status = StatusHelper.GetProjectStatusFromString(project.ProjectStatus);
                }

                compactModels.Add(compactModel);
            }
            compactModels = await CompactProjectList.FetchAuthorAvatars(compactModels, _personalDataService);

            return compactModels;
        }

        private CompetitionPlatformUser GetAuthenticatedUser()
        {
            return ClaimsHelper.GetUser(User.Identity);
        }

        public static bool IsGuid(string value)
        {
            Guid x;
            return Guid.TryParse(value, out x);
        }

        private async Task<bool> IsUserLykkeMember(string email)
        {
            var userRole = await _userRolesRepository.GetAsync(email);
            var isAdmin = (userRole != null) && userRole.Role == StreamsRoles.Admin;

            if (isAdmin) return true;

            var domain = email.Split('@').Last();
            if (domain == LykkeEmailDomains.LykkeCom || domain == LykkeEmailDomains.LykkexCom)
                return true;
            return false;
        }
    }
}