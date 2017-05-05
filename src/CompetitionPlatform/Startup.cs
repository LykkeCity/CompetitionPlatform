using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CompetitionPlatform.Data;
using CompetitionPlatform.Data.AzureRepositories.Log;
using AzureStorage.Tables;
using Common.Log;
using CompetitionPlatform.Authentication;
using CompetitionPlatform.Data.AzureRepositories.Settings;
using CompetitionPlatform.Exceptions;
using CompetitionPlatform.ScheduledJobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace CompetitionPlatform
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                // builder.AddUserSecrets();
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            Configuration = builder.Build();
            HostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }
        private BaseSettings Settings { get; set; }
        public IHostingEnvironment HostingEnvironment { get; }
        private ILog Log { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //var settingsConnectionString = Configuration["SettingsConnString"];
            //var settingsContainer = Configuration["SettingsContainerName"];
            //var settingsFileName = Configuration["SettingsFileName"];
            var connectionStringLogs = Configuration["LogsConnString"];

            var settingsUrl = Configuration["SettingsUrl"];

            Log = new LogToTable(new AzureTableStorage<LogEntity>(connectionStringLogs, "LogCompPlatform", null));

            try
            {
                //Settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(settingsConnectionString, settingsContainer, settingsFileName);
                Settings = GeneralSettingsReader.ReadGeneralSettingsFromUrl<BaseSettings>(settingsUrl);
                services.AddSingleton(Settings);
            }
            catch (Exception ex)
            {
                Log.WriteErrorAsync("Startup", "ReadSettingsFile", "Reading Settings File", ex).Wait();
            }

            var connectionString = Settings.LykkeStreams.Azure.StorageConnString;

            CheckSettings(Settings, Log);

            try
            {
                // Add framework services.
                services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

                services.AddAuthentication(
                    options => { options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; });

                //var notificationEmailsQueueConnString = "";
                var notificationSlackQueueConnString = Settings.SlackNotifications.AzureQueue.ConnectionString;

                services.AddApplicationInsightsTelemetry(Configuration);
                var builder = services.AddMvc();

                builder.AddMvcOptions(o => { o.Filters.Add(new GlobalExceptionFilter(Log, notificationSlackQueueConnString)); });

                services.RegisterLyykeServices();

                //if (HostingEnvironment.IsProduction() && !string.IsNullOrEmpty(notificationEmailsQueueConnString) &&
                //    !string.IsNullOrEmpty(notificationSlackQueueConnString))
                //{
                //    services.RegisterEmailNotificationServices(notificationEmailsQueueConnString);
                //}
                //else
                //{
                    services.RegisterInMemoryNotificationServices();
                //}

                services.RegisterRepositories(connectionString, Log);
                JobScheduler.Start(connectionString, Log);
            }
            catch (Exception ex)
            {
                Log.WriteErrorAsync("Startup", "RegisterServices", "Registering Repositories and services", ex).Wait();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseApplicationInsightsRequestTelemetry();

            try
            {
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();

                if (env.IsDevelopment() || env.IsStaging())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                }
                
                if (env.IsDevelopment())
                {
                    app.UseDatabaseErrorPage();
                    app.UseBrowserLink();
                }

                app.UseApplicationInsightsExceptionTelemetry();

                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true,
                    ExpireTimeSpan = TimeSpan.FromHours(24),
                    LoginPath = new PathString("/signin"),
                    AccessDeniedPath = "/Home/Error"
                });

                app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
                {
                    RequireHttpsMetadata = false,
                    SaveTokens = true,

                    // Note: these settings must match the application details
                    // inserted in the database at the server level.
                    ClientId = Settings.LykkeStreams.Authentication.ClientId,
                    ClientSecret = Settings.LykkeStreams.Authentication.ClientSecret,
                    PostLogoutRedirectUri = Settings.LykkeStreams.Authentication.PostLogoutRedirectUri,
                    CallbackPath = "/auth",

                    // Use the authorization code flow.
                    ResponseType = OpenIdConnectResponseType.Code,
                    Events = new CompPlatformAuthenticationEvents(Log, HostingEnvironment, Settings.LykkeStreams.Azure.StorageConnString),

                    // Note: setting the Authority allows the OIDC client middleware to automatically
                    // retrieve the identity provider's configuration and spare you from setting
                    // the different endpoints URIs or the token validation parameters explicitly.
                    Authority = Settings.LykkeStreams.Authentication.Authority,
                    Scope = { "email profile" }
                });

                app.UseStaticFiles();

                // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });
            }
            catch (Exception ex)
            {
                Log.WriteErrorAsync("Startup", "Configure", "Configuring App and Authentication", ex).Wait();
            }
        }

        private void CheckSettings(BaseSettings settings, ILog log)
        {
            if (string.IsNullOrEmpty(settings.LykkeStreams.Azure.StorageConnString))
                WriteSettingsReadError(log, "StorageConnString");

            if (string.IsNullOrEmpty(settings.LykkeStreams.Authentication.ClientId))
                WriteSettingsReadError(log, "ClientId");

            if (string.IsNullOrEmpty(settings.LykkeStreams.Authentication.ClientSecret))
                WriteSettingsReadError(log, "ClientSecret");

            if (string.IsNullOrEmpty(settings.LykkeStreams.Authentication.PostLogoutRedirectUri))
                WriteSettingsReadError(log, "PostLogoutRedirectUri");

            if (string.IsNullOrEmpty(settings.LykkeStreams.Authentication.Authority))
                WriteSettingsReadError(log, "Authority");

            if(string.IsNullOrEmpty(settings.EmailServiceBus.Key))
                WriteSettingsReadError(log, "EmailServiceBus-Key");

            if (string.IsNullOrEmpty(settings.EmailServiceBus.NamespaceUrl))
                WriteSettingsReadError(log, "EmailServiceBus-NamespaceUrl");

            if (string.IsNullOrEmpty(settings.EmailServiceBus.PolicyName))
                WriteSettingsReadError(log, "EmailServiceBus-PolicyName");

            if (string.IsNullOrEmpty(settings.EmailServiceBus.QueueName))
                WriteSettingsReadError(log, "EmailServiceBus-QueueName");
        }

        private void WriteSettingsReadError(ILog log, string elementName)
        {
            log.WriteErrorAsync("Startup:ReadSettings", "Read " + elementName, elementName + " Missing or Empty",
                new Exception(elementName + " is missing from the settings file!")).Wait();
        }
    }
}
