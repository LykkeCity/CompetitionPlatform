using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IPublicFeedbackData
    {
        string User { get; set; }
        string Title { get; set; }
        string Feedback { get; set; }
        string RowKey { get; set; }
    }

    public interface IPublicFeedbackRepository
    {
        Task SaveAsync(IPublicFeedbackData feedbackData);
        Task<IPublicFeedbackData> GetFeedbackAsync(string rowkey);
        Task<IEnumerable<IPublicFeedbackData>> GetFeedbacksAsync();
        Task DeleteFeedbacksAsync(string rowkey);
    }
}
