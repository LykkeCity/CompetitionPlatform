using System.IO;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Project
{
    public interface IProjectFileRepository
    {
        Task InsertProjectFile(Stream stream, string projectId);
        Task<Stream> GetProjectFile(string projectId);
    }
}
