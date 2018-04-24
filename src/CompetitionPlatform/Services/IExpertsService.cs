using CompetitionPlatform.Models.ProjectViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Services
{
    public interface IExpertsService
    {
        Task SaveExperts(string projectId, IEnumerable<ExpertViewModel> experts);
    }
}
