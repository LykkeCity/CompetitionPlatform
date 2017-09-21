using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using CompetitionPlatform.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public class ProjectEntity : TableEntity, IProjectData
    {
        public static string GeneratePartitionKey()
        {
            return "Project";
        }

        public static string GenerateRowKey(string projectId)
        {
            return projectId;
        }

        public string Id => RowKey;
        public string Name { get; set; }
        public string Overview { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public string ProjectStatus { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public DateTime CompetitionRegistrationDeadline { get; set; }
        public DateTime ImplementationDeadline { get; set; }
        public DateTime VotingDeadline { get; set; }
        public double BudgetFirstPlace { get; set; }
        public double? BudgetSecondPlace { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public string AuthorId { get; set; }
        public string AuthorFullName { get; set; }
        public int ParticipantsCount { get; set; }
        public string ProgrammingResourceName { get; set; }
        public string ProgrammingResourceLink { get; set; }
        public string UserAgent { get; set; }
        public bool SkipVoting { get; set; }
        public bool SkipRegistration { get; set; }
        public string PrizeDescription { get; set; }
        public string StreamId { get; set; }

        internal void Update(IProjectData src)
        {
            Name = src.Name;
            Overview = src.Overview;
            Description = src.Description;
            Category = src.Category;
            Tags = src.Tags;
            ProjectStatus = src.ProjectStatus;
            CompetitionRegistrationDeadline = src.CompetitionRegistrationDeadline;
            ImplementationDeadline = src.ImplementationDeadline;
            VotingDeadline = src.VotingDeadline;
            BudgetFirstPlace = src.BudgetFirstPlace;
            BudgetSecondPlace = src.BudgetSecondPlace;
            VotesFor = src.VotesFor;
            VotesAgainst = src.VotesAgainst;
            LastModified = src.LastModified;
            ParticipantsCount = src.ParticipantsCount;
            ProgrammingResourceName = src.ProgrammingResourceName;
            ProgrammingResourceLink = src.ProgrammingResourceLink;
            UserAgent = src.UserAgent;
            SkipVoting = src.SkipVoting;
            SkipRegistration = src.SkipRegistration;
            AuthorId = src.AuthorId;
            AuthorFullName = src.AuthorFullName;
            PrizeDescription = src.PrizeDescription;
            StreamId = src.StreamId;
        }

        public static ProjectEntity Create(IProjectData src)
        {
            var projectStatus = src.Status == Status.Draft ? Status.Draft.ToString() : Status.Initiative.ToString();

            var result = new ProjectEntity
            {
                RowKey = src.Id,
                PartitionKey = GeneratePartitionKey(),
                Name = src.Name,
                Overview = src.Overview,
                Description = src.Description,
                ProjectStatus = projectStatus,
                Category = src.Category,
                Tags = src.Tags,
                CompetitionRegistrationDeadline = src.CompetitionRegistrationDeadline,
                ImplementationDeadline = src.ImplementationDeadline,
                VotingDeadline = src.VotingDeadline,
                BudgetFirstPlace = src.BudgetFirstPlace,
                BudgetSecondPlace = src.BudgetSecondPlace,
                Created = src.Created,
                LastModified = src.Created,
                AuthorId = src.AuthorId,
                AuthorFullName = src.AuthorFullName,
                ParticipantsCount = src.ParticipantsCount,
                ProgrammingResourceName = src.ProgrammingResourceName,
                ProgrammingResourceLink = src.ProgrammingResourceLink,
                UserAgent = src.UserAgent,
                SkipVoting = src.SkipVoting,
                SkipRegistration = src.SkipRegistration,
                PrizeDescription = src.PrizeDescription,
                StreamId = src.StreamId
            };

            return result;
        }
    }

    public class ProjectRepository : IProjectRepository
    {
        private readonly INoSQLTableStorage<ProjectEntity> _projectsTableStorage;

        public ProjectRepository(INoSQLTableStorage<ProjectEntity> projectsTableStorage)
        {
            _projectsTableStorage = projectsTableStorage;
        }

        public async Task<IProjectData> GetAsync(string id)
        {
            var partitionKey = ProjectEntity.GeneratePartitionKey();
            var rowKey = ProjectEntity.GenerateRowKey(id);

            var project = await _projectsTableStorage.GetDataAsync(partitionKey, rowKey);
            return project == null ? null : ChangeNullWithDefault(project);
        }

        public async Task<IEnumerable<IProjectData>> GetProjectsAsync()
        {
            var partitionKey = ProjectEntity.GeneratePartitionKey();

            var projects = await _projectsTableStorage.GetDataAsync(partitionKey);

            return projects.ToList().Select(c =>
            {
                c = ChangeNullWithDefault(c);
                return c;
            });
        }

        public async Task<string> SaveAsync(IProjectData projectData)
        {
            var newEntity = ProjectEntity.Create(projectData);
            await _projectsTableStorage.InsertAsync(newEntity);
            return newEntity.Id;
        }

        public Task UpdateAsync(IProjectData projectData)
        {
            var partitionKey = ProjectEntity.GeneratePartitionKey();
            var rowKey = ProjectEntity.GenerateRowKey(projectData.Id);

            return _projectsTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(projectData);
                return itm;
            });
        }

        public async Task DeleteAsync(string id)
        {
            var partitionKey = ProjectEntity.GeneratePartitionKey();
            var rowKey = ProjectEntity.GenerateRowKey(id);

            await _projectsTableStorage.DeleteAsync(partitionKey, rowKey);
        }

        private ProjectEntity ChangeNullWithDefault(ProjectEntity projectData)
        {
            if (projectData.Overview == null) projectData.Overview = "Overview was not provided";
            if (projectData.Description == null) projectData.Overview = "Description was not provided";

            return projectData;
        }
    }
}
