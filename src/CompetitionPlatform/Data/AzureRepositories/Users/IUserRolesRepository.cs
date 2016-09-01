using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IUSerRoleData
    {
        string Role { get; set; }
    }

    public interface IUserRolesRepository
    {
        Task<IUSerRoleData> GetAsync(string userId);
    }
}
