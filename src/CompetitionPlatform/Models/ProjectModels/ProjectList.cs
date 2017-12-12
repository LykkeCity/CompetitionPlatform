using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Users;

namespace CompetitionPlatform.Models.ProjectModels
{
    
    public class ProjectList
    {
        private IEnumerable<IProjectData> _projectList;

        private ProjectList()
        {

        }

        // In order to deal with the async data fetch, use this method instead of a constructor (factory pattern)
        // e.g. var projectList = await ProjectList.CreateProjectList(projectRepository)
        public static Task<ProjectList> CreateProjectList(IProjectRepository projectRepository)
        {
            var returnProjectList = new ProjectList();
            return returnProjectList.FetchData(projectRepository);
        }
        private async Task<ProjectList> FetchData(IProjectRepository projectRepository)
        {
            _projectList = await projectRepository.GetProjectsAsync();
            return this;
        }

        // Note: Filter methods mutate the internal list of projects

        // Remove projects where the status is Draft
        public ProjectList RemoveDrafts()
        {
            _projectList = _projectList.Where(x => x.ProjectStatus != Status.Draft.ToString());
            return this;
        }

        // Remove projects where a given user is not following
        public async Task<ProjectList> FilterByFollowing(string userId, IProjectFollowRepository projectFollowRepository)
        {
            // Create the list of following projects, then filter out projects not on the list

            var followingProjectIds = new List<string>();
            foreach (var project in _projectList)
            {
                if(await projectFollowRepository.GetAsync(userId, project.Id) != null)
                    followingProjectIds.Add(project.Id);
            }
            _projectList = _projectList.Where(x => followingProjectIds.IndexOf(x.Id) > 0);
            return this;
        }

        // Remove projects where a given user is not participating
        public async Task<ProjectList> FilterByParticipating(string userId, IProjectParticipantsRepository projectParticipantsRepository)
        {
            // Create the list of following projects, then filter out projects not on the list

            var participatingProjectIds = new List<string>();
            foreach (var project in _projectList)
            {
                if (await projectParticipantsRepository.GetAsync(userId, project.Id) != null)
                    participatingProjectIds.Add(project.Id);
            }
            _projectList = _projectList.Where(x => participatingProjectIds.IndexOf(x.Id) > 0);
            return this;
        }
        
        // Select projects whose status matches a string
        public ProjectList FilterByStatus(string projectStatusFilter)
        {
            if (!string.IsNullOrEmpty(projectStatusFilter) && projectStatusFilter != "All")
                _projectList = _projectList.Where(x => x.ProjectStatus == projectStatusFilter);
            return this;
        }

        // Select projects whose category matches a string
        public ProjectList FilterByCategory(string projectCategoryFilter)
        {
            if (!string.IsNullOrEmpty(projectCategoryFilter) && projectCategoryFilter != "All")
                _projectList = _projectList.Where(x => x.Category.Replace(" ", "") == projectCategoryFilter);
            return this;
        }

        // Select projects whose author ID matches a string
        public ProjectList FilterByAuthorId(string projectAuthorId)
        {
            if (!string.IsNullOrEmpty(projectAuthorId))
                _projectList = _projectList.Where(x => x.AuthorId == projectAuthorId);
            return this;
        }

        // Select projects that are not archived and not an Initiative
        public ProjectList FilterByCurrentProjects(bool currentProjectsFilter = true)
        {
            if(currentProjectsFilter == true)
                _projectList = _projectList.Where(x => x.ProjectStatus != Status.Initiative.ToString() && x.ProjectStatus != Status.Archive.ToString());
            return this;
        }

        // Order projects by the first place prize (default: ascending)
        public ProjectList OrderByPrize(string prizeOrder = "Ascending")
        {
            if (!string.IsNullOrEmpty(prizeOrder))
            {
                if (prizeOrder == "Ascending")
                    _projectList = _projectList.OrderBy(x => x.BudgetFirstPlace);

                if (prizeOrder == "Descending")
                    _projectList = _projectList.OrderByDescending(x => x.BudgetFirstPlace);
            }
            return this;
        }

        // Order projects by the first place prize (default: ascending)
        public ProjectList OrderByLastModified(string modifiedOrder = "Descending")
        {
            if (!string.IsNullOrEmpty(modifiedOrder))
            {
                if (modifiedOrder == "Ascending")
                    _projectList = _projectList.OrderBy(x => x.LastModified);

                if (modifiedOrder == "Descending")
                    _projectList = _projectList.OrderByDescending(x => x.LastModified);
            }
            return this;
        }

        // Combine two ProjectLists
        public ProjectList DistinctUnion(ProjectList addedProjectList)
        {
            // Because there is no easy lambda function for Distinct, use
            // groupBy and then take the first. https://stackoverflow.com/questions/1300088/distinct-with-lambda
            _projectList = _projectList.Concat(addedProjectList.GetProjects()).GroupBy(x => x.Id)
                .Select(group => group.FirstOrDefault());
            return this;
        }

        public IEnumerable<IProjectData> GetProjects()
        {
            return _projectList;
        }
    }
}