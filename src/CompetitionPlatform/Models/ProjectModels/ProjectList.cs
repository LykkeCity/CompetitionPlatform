using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;

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

        public IEnumerable<IProjectData> GetProjects()
        {
            return _projectList;
        }
    }
}