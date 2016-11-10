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
            var settingsConnectionString = Configuration["SettingsConnString"];
            var settingsContainer = Configuration["SettingsContainerName"];
            var settingsFileName = Configuration["SettingsFileName"];
            var connectionStringLogs = Configuration["LogsConnString"];

            Log = new LogToTable(new AzureTableStorage<LogEntity>(connectionStringLogs, "LogCompPlatform", null));

            try
            {
                Settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(settingsConnectionString,
                    settingsContainer, settingsFileName);
            }
            catch (Exception ex)
            {
                Log.WriteError("Startup", "ReadSettingsFile", "Reading Settings File", ex).Wait();
            }

            var connectionString = Settings.Azure.StorageConnString;

            CheckSettings(Settings, Log);
            try
            {
                // Add framework services.
                services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

                services.AddAuthentication(
                    options => { options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; });

                services.AddMvc();
                services.RegisterLyykeServices();

                var notificationEmailsQueueConnString = Settings.Notifications.EmailsQueueConnString;
                var notificationSlackQueueConnString = Settings.Notifications.SlackQueueConnString;

                if (HostingEnvironment.IsProduction() && !string.IsNullOrEmpty(notificationEmailsQueueConnString) &&
                    !string.IsNullOrEmpty(notificationSlackQueueConnString))
                {
                    services.RegisterNotificationServices(notificationEmailsQueueConnString,
                        notificationSlackQueueConnString);
                }
                else
                {
                    services.RegisterInMemoryNotificationServices();
                }

                services.RegisterRepositories(connectionString, Log);
                JobScheduler.Start(connectionString, Log);
            }
            catch (Exception ex)
            {
                Log.WriteError("Startup", "RegisterServices", "Registering Repositories and services", ex).Wait();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            try
            {
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();

                if (env.IsStaging() || env.IsProduction())
                {
                    app.Use(async (context, next) =>
                    {
                        if (context.Request.IsHttps)
                        {
                            await next();
                        }
                        else
                        {
                            var withHttps = "https://" + context.Request.Host + context.Request.Path;
                            context.Response.Redirect(withHttps);
                        }
                    });
                }

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

                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true,
                    ExpireTimeSpan = TimeSpan.FromMinutes(20),
                    LoginPath = new PathString("/signin"),
                    AccessDeniedPath = "/Home/Error"
                });

                app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
                {
                    RequireHttpsMetadata = false,
                    SaveTokens = true,

                    // Note: these settings must match the application details
                    // inserted in the database at the server level.
                    ClientId = Settings.Authentication.ClientId,
                    ClientSecret = Settings.Authentication.ClientSecret,
                    PostLogoutRedirectUri = Settings.Authentication.PostLogoutRedirectUri,
                    CallbackPath = "/auth",

                    // Use the authorization code flow.
                    ResponseType = OpenIdConnectResponseType.Code,
                    Events = new CompPlatformAuthenticationEvents(Log, HostingEnvironment, Settings.Azure.StorageConnString, Settings.Notifications.EmailsQueueConnString),

                    // Note: setting the Authority allows the OIDC client middleware to automatically
                    // retrieve the identity provider's configuration and spare you from setting
                    // the different endpoints URIs or the token validation parameters explicitly.
                    Authority = Settings.Authentication.Authority,
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
                Log.WriteError("Startup", "Configure", "Configuring App and Authentication", ex).Wait();
            }
        }

        private void CheckSettings(BaseSettings settings, ILog log)
        {
            if (string.IsNullOrEmpty(settings.Azure.StorageConnString))
                WriteSettingsReadError(log, "StorageConnString");

            if (string.IsNullOrEmpty(settings.Authentication.ClientId))
                WriteSettingsReadError(log, "ClientId");

            if (string.IsNullOrEmpty(settings.Authentication.ClientSecret))
                WriteSettingsReadError(log, "ClientSecret");

            if (string.IsNullOrEmpty(settings.Authentication.PostLogoutRedirectUri))
                WriteSettingsReadError(log, "PostLogoutRedirectUri");

            if (string.IsNullOrEmpty(settings.Authentication.Authority))
                WriteSettingsReadError(log, "Authority");

            if (string.IsNullOrEmpty(settings.Notifications.EmailsQueueConnString))
                WriteSettingsReadError(log, "EmailsQueueConnString");

            if (string.IsNullOrEmpty(settings.Notifications.SlackQueueConnString))
                WriteSettingsReadError(log, "SlackQueueConnString");
        }

        private void WriteSettingsReadError(ILog log, string elementName)
        {
            log.WriteError("Startup:ReadSettings", "Read " + elementName, elementName + " Missing or Empty",
                new Exception(elementName + " is missing from the settings file!")).Wait();
        }
    }
}
