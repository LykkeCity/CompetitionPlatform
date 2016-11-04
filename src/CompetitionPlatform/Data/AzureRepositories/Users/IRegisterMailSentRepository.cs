using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IRegisterMailSentData
    {
        string UserId { get; set; }
    }

    public interface IRegisterMailSentRepository
    {
        Task SaveRegisterAsync(string userId);
        Task<IRegisterMailSentData> GetRegisterAsync(string userId);
    }
}
