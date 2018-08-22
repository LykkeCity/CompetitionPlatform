using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Helpers;
using Lykke.Common.Log;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;

namespace CompetitionPlatform.Authentication
{
    public class CompPlatformAuthenticationEvents : OpenIdConnectEvents
    {
        private readonly IRegisterMailSentRepository _mailSentRepository;
        private readonly ILog _log;

        public CompPlatformAuthenticationEvents(ILog log, IHostingEnvironment hostingEnvironment, IReloadingManager<string> connString)
        {
            _mailSentRepository = new RegisterMailSentRepository(AzureTableStorage<RegisterMailSentEntity>.Create(connString, "RegisterMailSent", log));
            _log = log;
        }

        public override Task RemoteFailure(RemoteFailureContext context)
        {
            _log.Error("RemoteFailure", context.Failure, context.Failure.Message, context.Failure.Message + context.Failure.InnerException);

            context.HandleResponse();
            context.Response.Redirect("/Home/AuthenticationFailed");

            return Task.FromResult(0);
        }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var email = context.Principal.Claims.Where(c => c.Type == ClaimTypes.Email)
                   .Select(c => c.Value).SingleOrDefault();

            var sentMail = await _mailSentRepository.GetRegisterAsync(email);

            if (sentMail == null)
            {
                var firstName = context.Principal.Claims.Where(c => c.Type == ClaimTypes.GivenName)
                   .Select(c => c.Value).SingleOrDefault();

                var message = NotificationMessageHelper.GenerateRegistrationMessage(firstName, email);
                //await _emailsQueue.PutMessageAsync(message);

                await _mailSentRepository.SaveRegisterAsync(email);
            }
            await base.TokenValidated(context);
        }

        public override Task TicketReceived(TicketReceivedContext context)
        {
            context.Properties.Items.Clear();

            context.Properties.Items.Clear();

            foreach (var principalClaim in context.Principal.Claims)
            {
                principalClaim.Properties.Clear();
            }

            return base.TicketReceived(context);
        }
    }
}