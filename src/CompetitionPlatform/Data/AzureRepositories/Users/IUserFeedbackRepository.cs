using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IUserFeedbackData
    {
        string Email { get; set; }
        string Name { get; set; }
        string Feedback { get; set; }
        DateTime Created { get; set; }
    }

    public interface IUserFeedbackRepository
    {
        Task SaveAsync(IUserFeedbackData feedbackData);
        Task<IEnumerable<IUserFeedbackData>> GetFeedbacksAsync();
    }
}
