using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Admin
{
    public interface ITermsPageData
    {
        string ProjectId { get; set; }
        string Content { get; set; }
    }

    public interface ITermsPageRepository
    {
        Task SaveAsync(string projectId, string content);
        Task<ITermsPageData> GetAsync(string projectId);
    }
}
