using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CompetitionPlatform.Models;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public interface IProjectData
    {
        string Id { get; }
        string Name { get; set; }
        string Overview { get; set; }
        string Description { get; set; }
        Status Status { get; set; }
        string ProjectStatus { get; set; }
        string Category { get; set; }
        string Tags { get; set; }
        DateTime CompetitionRegistrationDeadline { get; set; }
        DateTime ImplementationDeadline { get; set; }
        DateTime VotingDeadline { get; set; }
        double BudgetFirstPlace { get; set; }
        double? BudgetSecondPlace { get; set; }
        int VotesFor { get; set; }
        int VotesAgainst { get; set; }
        DateTime Created { get; set; }
        DateTime LastModified { get; set; }
        string AuthorId { get; set; }
        string AuthorFullName { get; set; }
        int ParticipantsCount { get; set; }
        string ProgrammingResourceName { get; set; }
        string ProgrammingResourceLink { get; set; }
        string UserAgent { get; set; }
        bool SkipVoting { get; set; }
    }

    public interface IProjectRepository
    {
        Task<IProjectData> GetAsync(string id);
        Task<IEnumerable<IProjectData>> GetProjectsAsync();
        Task<string> SaveAsync(IProjectData projectData);
        Task UpdateAsync(IProjectData projectData);
    }
}
