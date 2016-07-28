using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Models;
using Microsoft.AspNetCore.Mvc;

namespace CompetitionPlatform.Controllers
{
    public class UsersManagementController : Controller
    {
        private readonly IUsersRepository _usersRepository;

        public UsersManagementController(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        [HttpPost]
        public async Task<ActionResult> CreateUser(EditUserModel model)
        {
                if (string.IsNullOrEmpty(model.FullName))
                    return null;

            await _usersRepository.SaveAsync(model, model.Password);

            return View("~/Views/Account/Login.cshtml");
        }
    }
}