using System.Threading.Tasks;

namespace CompetitionPlatform.Services
{
    public interface IProjectWinnersService
    {
        Task SaveWinners(string projectId);
    }
}
