using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Common.Log;
using CompetitionPlatform.Data.AzureRepositories.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace CompetitionPlatform.Authentication
{
    public class CompPlatformAuthenticationEvents : OpenIdConnectEvents
    {
        private readonly IProjectFollowRepository _projectFollowRepository;

        public CompPlatformAuthenticationEvents(string connString, ILog log)
        {
            _projectFollowRepository = new ProjectFollowRepository(new AzureTableStorage<ProjectFollowEntity>(connString, "ProjectFollows", log));
        }

        public override Task RemoteFailure(FailureContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/AuthenticationFailed");

            return Task.FromResult(0);
        }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var email = context.Ticket.Principal.Claims.Where(c => c.Type == ClaimTypes.Email)
                   .Select(c => c.Value).SingleOrDefault();

            var follows = await _projectFollowRepository.GetFollowAsync();
            await base.TokenValidated(context);
        }
    }
}
