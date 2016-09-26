using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CompetitionPlatform.Authentication
{
    public class CompPlatformAuthenticationEvents : OpenIdConnectEvents
    {
        public override Task RemoteFailure(FailureContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/AuthenticationFailed");
            return Task.FromResult(0);
        }
    }
}
