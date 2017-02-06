using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;

namespace CompetitionPlatform.Authentication
{
    public class CompPlatformAuthenticationEvents : OpenIdConnectEvents
    {
        private readonly IRegisterMailSentRepository _mailSentRepository;
        private readonly IQueueExt _emailsQueue;

        public CompPlatformAuthenticationEvents(ILog log, IHostingEnvironment hostingEnvironment, string connString, string emailsQueueConnString)
        {
            _mailSentRepository = new RegisterMailSentRepository(new AzureTableStorage<RegisterMailSentEntity>(connString, "RegisterMailSent", log));

            if (hostingEnvironment.IsProduction() && !string.IsNullOrEmpty(emailsQueueConnString))
            {
                _emailsQueue = new AzureQueueExt(emailsQueueConnString, "emailsqueue");
            }
            else
            {
                _emailsQueue = new QueueExtInMemory();
            }
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

            var sentMail = await _mailSentRepository.GetRegisterAsync(email);

            if (sentMail == null)
            {
                var firstName = context.Ticket.Principal.Claims.Where(c => c.Type == ClaimTypes.GivenName)
                   .Select(c => c.Value).SingleOrDefault();

                var message = NotificationMessageHelper.GenerateRegistrationMessage(firstName, email);
                await _emailsQueue.PutMessageAsync(message);

                await _mailSentRepository.SaveRegisterAsync(email);
            }
            await base.TokenValidated(context);
        }

        public override Task TicketReceived(TicketReceivedContext context)
        {
            context.Ticket.Properties.Items.Clear();

            context.Properties.Items.Clear();

            foreach (var principalClaim in context.Ticket.Principal.Claims)
            {
                principalClaim.Properties.Clear();
            }

            return base.TicketReceived(context);
        }
    }
}
