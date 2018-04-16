using CompetitionPlatform.Models.ProjectViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Services
{
    public interface IProjectWinnersService
    {
        Task SaveWinners(string projectId, IEnumerable<WinnerViewModel> winners = null);
        Task SaveCustomWinners(string projectId, IEnumerable<WinnerViewModel> winners);
    }
}
