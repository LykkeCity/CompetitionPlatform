using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Models;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public interface IProjectData
    {
        string Id { get;  }
        string Name { get; set; }
        string Description { get; set; }
        Status Status { get; set; }
        List<string> Categories { get; set; }
        DateTime CompetitionRegistrationDeadline { get; set; }
        DateTime ImplementationDeadline { get; set; }
        DateTime VotingDeadline { get; set; }
        double BudgetFirstPlace { get; set; }
        double? BudgetSecondPlace { get; set; }
        double? BudgetThirdPlace { get; set; }
        int VotesFor { get; set; }
        int VotesAgainst { get; set; }

        
    }

    interface IProjectRepository
    {
        Task<IProjectData> GetAsync(string id);
        Task SaveAsync(IProjectData projectData);
        Task UpdateAsync(IProjectData projectData);
    }
}
