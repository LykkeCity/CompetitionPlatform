using System;
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
using Newtonsoft.Json;

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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"RemoteFailure: {JsonConvert.SerializeObject(context, new JsonSerializerSettings(){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore})}");
            Console.ResetColor();

            _log.Error(context.Failure.Message + context.Failure.InnerException, context.Failure);

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

        public override Task RedirectToIdentityProvider(RedirectContext context)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"RedirectUri: {context.ProtocolMessage.RedirectUri}");
            Console.ResetColor();
            //context.ProtocolMessage.RedirectUri = urlWithHttps;
            return base.RedirectToIdentityProvider(context);
        }
    }
}