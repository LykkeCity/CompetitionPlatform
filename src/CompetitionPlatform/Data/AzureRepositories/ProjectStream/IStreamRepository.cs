using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.ProjectStream
{
    public interface IStreamData
    {
        string Id { get; set; }
        string Name { get; set; }
        string Stream { get; set; }
        string AuthorId { get; set; }
        string AuthorEmail { get; set; }
    }

    public interface IStreamRepository
    {
        Task<IStreamData> GetAsync(string id);
        Task<IEnumerable<IStreamData>> GetStreamsAsync();
        Task<string> SaveAsync(IStreamData streamData);
        Task UpdateAsync(IStreamData streamData);
        Task DeleteAsync(string id);
    }
}
