using System.IO;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Result
{
    public interface IProjectResultRepository
    {
        Task InsertProjectResult(Stream stream, string resultId);
        Task<Stream> GetProjectResult(string resultId);
    }
}
