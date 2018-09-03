using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CompetitionPlatform.Data;
using AzureStorage.Tables;
using Common.Log;
using CompetitionPlatform.Authentication;
using CompetitionPlatform.Data.AzureRepositories.Settings;
using CompetitionPlatform.Exceptions;
using CompetitionPlatform.ScheduledJobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CompetitionPlatform.Modules;
using Lykke.Logs;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

namespace CompetitionPlatform
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }
        public IContainer ApplicationContainer { get; set; }
        public ILog Log { get; private set; }
        private BaseSettings _settings;

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
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            Configuration = builder.Build();
            HostingEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                var settings = Configuration.LoadSettings<BaseSettings>();
                _settings = settings.CurrentValue;
                
                // Add framework services.
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromHours(24);
                    options.LoginPath = new PathString("/signin");
                    options.AccessDeniedPath = "/Home/Error";
                })
                .AddOpenIdConnect(options =>
                {
                    options.Authority = _settings.LykkeStreams.Authentication.Authority;
                    options.ClientId = _settings.LykkeStreams.Authentication.ClientId;
                    options.ClientSecret = _settings.LykkeStreams.Authentication.ClientSecret;
                    options.RequireHttpsMetadata = true;
                    options.SaveTokens = true;
                    options.CallbackPath = "/auth";
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.Events = new CompPlatformAuthenticationEvents(Log, HostingEnvironment,
                        settings.ConnectionString(x => x.LykkeStreams.Azure.StorageConnString));
                    options.Scope.Add("email");
                });
                
                services.AddApplicationInsightsTelemetry(Configuration);
                var mvcBuilder = services.AddMvc();

                mvcBuilder.AddMvcOptions(o => { o.Filters.Add(
                    new GlobalExceptionFilter(Log, settings)); });

                services.RegisterLyykeServices();

                services.RegisterInMemoryNotificationServices();
                
                JobScheduler.Start(settings.ConnectionString(x => x.LykkeStreams.Azure.StorageConnString), Log);

                var builder = new ContainerBuilder();
                var logFactory = CreateLog(builder, settings);
                
                builder.RegisterModule(new DbModule(settings, logFactory));
                builder.Populate(services);
                
                ApplicationContainer = builder.Build();
                Log = ApplicationContainer.Resolve<ILogFactory>().CreateLog(this);

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.Critical(nameof(ConfigureServices), ex);
                throw;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
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
                
                app.UseLykkeMiddleware(ex => new { ex.Message });
                app.UseLykkeForwardedHeaders();

                app.UseAuthentication();

                app.UseStaticFiles();

                // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "Terms",
                        template: "Terms/{id?}",
                        defaults: new { controller = "Home", action = "Terms" });

                    routes.MapRoute(
                        name: "default",
                        template: "{controller}/{action}/{id?}",
                        defaults: new { controller = "Home", action = "Index" });

                    routes.MapRoute(
                        "404",
                        "{*url}",
                        new { controller = "Home", action = "PageNotFound" }
                    );
                });

                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                Log.Error(nameof(Configure), ex, ex.Message, "Configuring App and Authentication");
            }
        }
        
        private static ILogFactory CreateLog(ContainerBuilder builder, IReloadingManager<BaseSettings> settings)
        {
            var services = new ServiceCollection();
            
            services.AddLykkeLogging(
                settings.ConnectionString(x => x.LykkeStreams.Azure.StorageLogConnString),
                "LogCompPlatform",
                settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            );

            builder.Populate(services);

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<ILogFactory>();
        }

        private void CleanUp()
        {
            try
            {
                // NOTE: Service can't recieve and process requests here, so you can destroy all resources
                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    Log.Critical(nameof(CleanUp), ex);
                    (Log as IDisposable)?.Dispose();
                }
                throw;
            }
        }
    }
}
