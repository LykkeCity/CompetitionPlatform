using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Expert;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.AzureRepositories.Vote;
using CompetitionPlatform.Models.ProjectViewModels;

namespace CompetitionPlatform.Services
{
    public class ExpertsService : IExpertsService
    {
        private readonly IProjectExpertsRepository _expertRepository;

        public ExpertsService(IProjectExpertsRepository expertRepository)
        {
            _expertRepository = expertRepository;
        }

        public async Task SaveExperts(string projectId, IEnumerable<ExpertViewModel> experts)
        {
            foreach (var expertUser in experts.Where(x => !string.IsNullOrEmpty(x.UserId)))
            {
                var expert = await _expertRepository.GetAsync(expertUser.UserId);
                expert.ProjectId = projectId;
                expert.Priority = 0;
                expert.Description = "Expert";
                await _expertRepository.SaveAsync(expert);
            }
        }
    }
}
