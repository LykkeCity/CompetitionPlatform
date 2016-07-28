using System.IO;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Result
{
    public interface IProjectResultRepository
    {
        Task InsertProjectResult(Stream stream, string projectId, string userName);
        Task<Stream> GetProjectResult(string projectId, string userName);
    }
}
