using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionPlatform.Data.AzureRepositories.Users
{
    public interface IUser
    {
        string Id { get; }
        string FullName { get; }
        bool IsAdmin { get; }
    }

    public class User : IUser
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }

        public static IUser CreateDefaultAdminUser(string id)
        {
            return new User()
            {
                Id = id,
                FullName = "Admin",
                IsAdmin = true
            };
        }

        public static IUser CreateDefault()
        {
            return new User()
            {
                IsAdmin = false
            };
        }
    }

    public interface IUsersRepository
    {
        Task SaveAsync(IUser user, string password);
        Task<IUser> AuthenticateAsync(string username, string password);
        Task<IUser> GetAsync(string id);
        Task<bool> UserExists(string id);
        Task ChangePasswordAsync(string id, string newPassword);
        Task<IEnumerable<IUser>> GetAllAsync();
    }
}
