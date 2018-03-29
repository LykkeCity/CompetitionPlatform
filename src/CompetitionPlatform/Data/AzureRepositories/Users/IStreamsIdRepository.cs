using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IStreamsIdData
    {
        string ClientId { get; set; }
        string StreamsId { get; set; }
    }

    public interface IStreamsIdRepository
    {
        Task SaveAsync(IStreamsIdData streamsIdData);
        Task<IStreamsIdData> GetOrCreateAsync(string clientId);
        Task<IEnumerable<IStreamsIdData>> GetStreamsIdsAsync();
    }
}
