using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Models;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace CompetitionPlatform.Controllers
{
    public static class UsersControllerExtension
    {
        public static string SessionCookie => "Session";

        public static string GetSession(this Controller contr)
        {
            var sessionCookie = contr.HttpContext.Request.Cookies[SessionCookie];
            if (sessionCookie != null)
                return sessionCookie;

            var sessionId = Guid.NewGuid().ToString();
            //var newCookie = new HttpCookie(SessionCookie, sessionId) { Expires = DateTime.UtcNow.AddYears(5) };
            //contr.Response.SetCookie(newCookie);
            return sessionId;
        }
    }

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
            var user = await _usersRepository.AuthenticateAsync(data.FullName, data.Password);
            SignIn(user);
            var authenticated = User.Identity.IsAuthenticated;
            return View("~/Views/Home/Index.cshtml");
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
    }
}