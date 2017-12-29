using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Models.ProjectViewModels;
using Lykke.Service.PersonalData.Contract;
using Newtonsoft.Json;

namespace CompetitionPlatform.Models.ProjectModels
{

    public class CompactProjectList
    {
        /*
         * TODO: Determine if this really needs its own model or if it should be a build method
         * in the ProjectCompactViewModel class. It is frequently used, has nontrivial logic built in,
         * and may grow in the future - putting it as a class for now
         */

        private List<ProjectCompactViewModel> _compactProjectsModelList = new List<ProjectCompactViewModel>();

        private CompactProjectList(IEnumerable<IProjectData> baseProjectList)
        {
            /*
             * Populate the base project data into the CompactProjectList. Lazy instantiation here -
             * the thinking is that it's better to explicitly handle data fetches and handle potential errors
             * separately for the countsin case comments/results/winners count fails
             */
            foreach (var BaseProject in baseProjectList)
            {
                var CompactProject = new ProjectCompactViewModel
                {
                    CommentsCount = 0,
                    ResultsCount = 0,
                    WinnersCount = 0,
                    Tags = new List<string>(),
                    IsFollowing = false,
                    BaseProjectData = BaseProject
                };
                _compactProjectsModelList.Add(CompactProject);
            }
        }

        // In order to deal with the async data fetch, use this method instead of a constructor (factory pattern)
        // e.g. var compactProjectList = await CreateCompactProject(baseProjectList)
        public static Task<CompactProjectList> CreateCompactProjectList(
            IEnumerable<IProjectData> baseProjectList,
            IProjectCommentsRepository commentsRepository,
            IProjectParticipantsRepository participantsRepository,
            IProjectFollowRepository projectFollowRepository,
            IProjectResultRepository resultsRepository,
            IProjectWinnersRepository winnersRepository,
            string userEmail,
            IPersonalDataService personalDataService
        )
        {
            var returnCompactProjectList = new CompactProjectList(baseProjectList);
            return returnCompactProjectList.FetchData(
                commentsRepository,
                participantsRepository,
                projectFollowRepository,
                resultsRepository,
                winnersRepository,
                userEmail,
                personalDataService
            );
        }

        /*
         * TODO: Refactor? Tradeoff is seamless list creation on construction instead of separate function to populate all the data,
         * downside is having to pass in a lot of arguments to a model
         */
        private async Task<CompactProjectList> FetchData(
            IProjectCommentsRepository commentsRepository,
            IProjectParticipantsRepository participantsRepository,
            IProjectFollowRepository projectFollowRepository,
            IProjectResultRepository resultsRepository,
            IProjectWinnersRepository winnersRepository,
            string userEmail,
            IPersonalDataService personalDataService
        )
        {
            foreach (var compactProject in _compactProjectsModelList)
            {
                compactProject.CommentsCount = await commentsRepository.GetProjectCommentsCountAsync(compactProject.BaseProjectData.Id);
                compactProject.ParticipantsCount = await participantsRepository.GetProjectParticipantsCountAsync(compactProject.BaseProjectData.Id);
                compactProject.ResultsCount = await resultsRepository.GetResultsCountAsync(compactProject.BaseProjectData.Id);
                compactProject.WinnersCount = await winnersRepository.GetWinnersCountAsync(compactProject.BaseProjectData.Id);
                compactProject.Tags = FetchTags(compactProject.BaseProjectData.Tags);
                compactProject.IsFollowing = await CheckIsFollowing(projectFollowRepository, userEmail,
                    compactProject.BaseProjectData.Id);
            }

            _compactProjectsModelList = await FetchAuthorAvatars(_compactProjectsModelList, personalDataService);
            return this;
        }

        /*
         * Determine if a given user, identified by email, is following this project
         */
        private async Task<bool> CheckIsFollowing(IProjectFollowRepository projectFollowRepository, string userEmail, string projectId)
        {
            if (userEmail != null)
            {
                if (await projectFollowRepository.GetAsync(userEmail, projectId) != null)
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * Deserialize the tags string into individual tags
         */
        private List<string> FetchTags(string tagsString)
        {
            if (!string.IsNullOrEmpty(tagsString))
            {
                return JsonConvert.DeserializeObject<List<string>>(tagsString);
            }

            return new List<string>();
        }

        public List<ProjectCompactViewModel> GetProjects()
        {
            return _compactProjectsModelList;
        }

        public static async Task<List<ProjectCompactViewModel>> FetchAuthorAvatars(List<ProjectCompactViewModel> compactProjectsModelList, IPersonalDataService personalDataService)
        {
            var authorIdList = compactProjectsModelList.Select(compactProject => compactProject.BaseProjectData.AuthorIdentifier).ToList();
            var authorAvatarUrls = await personalDataService.GetClientAvatarsAsync(authorIdList);

            foreach (var compactProject in compactProjectsModelList)
            {
                compactProject.AuthorAvatarUrl = authorAvatarUrls[compactProject.BaseProjectData.AuthorIdentifier];
            }

            return compactProjectsModelList;
        }
    }
}