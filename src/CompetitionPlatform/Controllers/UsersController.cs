using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Models;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Authentication.Internal;
using Microsoft.AspNetCore.Mvc;

namespace CompetitionPlatform.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUsersRepository _usersRepository;

        public UsersController(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        [HttpPost]
        public async Task<ActionResult> Authenticate(AuthenticateModel data)
        {
            var user = await _usersRepository.AuthenticateAsync(data.Username, data.Password);
            SignIn(user);

            return null;
        }

        private void SignIn(IUser user)
        {
            var authManager = HttpContext.Authentication;

            var identity = MakeIdentity(user);

            authManager.SignInAsync("MyCookieMiddlewareInstance", identity, new AuthenticationProperties { IsPersistent = false });
        }

        private static ClaimsPrincipal MakeIdentity(IUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id),
                new Claim(ClaimTypes.Email, user.Id)
            };
            
            var identity = new ClaimsIdentity(claims, "ApplicationCookie");

            var principalIdentity = new ClaimsPrincipal(identity);

            return principalIdentity;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}