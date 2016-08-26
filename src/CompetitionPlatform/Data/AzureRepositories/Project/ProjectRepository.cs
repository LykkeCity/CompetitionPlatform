using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CompetitionPlatform.Models;
using Microsoft.WindowsAzure.Storage.Table;
using AzureStorage.Tables;

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

        internal void Update(IProjectData src)
        {
            Name = src.Name;
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
        }

        public static ProjectEntity Create(IProjectData src)
        {
            var result = new ProjectEntity
            {
                RowKey = Guid.NewGuid().ToString("N"),
                PartitionKey = GeneratePartitionKey(),
                Name = src.Name,
                Description = src.Description,
                ProjectStatus = Status.Initiative.ToString(),
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
                ParticipantsCount = src.ParticipantsCount
            };

            return result;
        }
    }

    public class ProjectRepository : IProjectRepository
    {
        private readonly IAzureTableStorage<ProjectEntity> _projectsTableStorage;

        public ProjectRepository(IAzureTableStorage<ProjectEntity> projectsTableStorage)
        {
            _projectsTableStorage = projectsTableStorage;
        }

        public async Task<IProjectData> GetAsync(string id)
        {
            var partitionKey = ProjectEntity.GeneratePartitionKey();
            var rowKey = ProjectEntity.GenerateRowKey(id);

            return await _projectsTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IProjectData>> GetProjectsAsync()
        {
            var partitionKey = ProjectEntity.GeneratePartitionKey();
            return await _projectsTableStorage.GetDataAsync(partitionKey);
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
    }
}
