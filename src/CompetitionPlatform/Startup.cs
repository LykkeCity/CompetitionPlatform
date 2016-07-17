using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CompetitionPlatform.Data;
using CompetitionPlatform.Data.AzureRepositories.Log;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Models;
using CompetitionPlatform.Services;
using AzureStorage.Tables;
using CompetitionPlatform.Data.AzureRepositories.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
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
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(
                options => { options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; });

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();


            var log = new LogToTable(new AzureTableStorage<LogEntity>(Configuration.GetConnectionString("AzureStorageLog"), "LogJobs", null));

            services.AddSingleton<IAzureTableStorage<ProjectEntity>>(
                new AzureTableStorage<ProjectEntity>(Configuration.GetConnectionString("AzureStorage"), "Projects", log));

            services.AddSingleton<IAzureBlob>(
                new AzureBlobStorage(Configuration.GetConnectionString("AzureStorage")));

            services.AddSingleton<IAzureTableStorage<UserEntity>>(
                new AzureTableStorage<UserEntity>(Configuration.GetConnectionString("AzureStorage"), "Users", log));

            services.AddSingleton<IAzureTableStorage<CommentEntity>>(
                new AzureTableStorage<CommentEntity>(Configuration.GetConnectionString("AzureStorage"), "ProjectComments", log));

            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IProjectFileRepository, ProjectFileRepository>();
            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddTransient<IProjectCommentsRepository, ProjectCommentsRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AutomaticAuthenticate = true,
            //    AutomaticChallenge = true,
            //    ExpireTimeSpan = TimeSpan.FromMinutes(5),
            //    LoginPath = new PathString("/signin"),
            //    AccessDeniedPath = "/Home/Forbidden"
            //});

            //app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            //{
            //    RequireHttpsMetadata = false,
            //    SaveTokens = true,

            //    // Note: these settings must match the application details
            //    // inserted in the database at the server level.
            //    ClientId = "myClient",
            //    ClientSecret = "secret_secret_secret",
            //    PostLogoutRedirectUri = "https://lykke-auth-dev.azurewebsites.net/",
            //    CallbackPath = "/auth",

            //    // Use the authorization code flow.
            //    ResponseType = OpenIdConnectResponseType.Code,
            //    //Events = new TerminalAuthenticationEvents(),

            //    // Note: setting the Authority allows the OIDC client middleware to automatically
            //    // retrieve the identity provider's configuration and spare you from setting
            //    // the different endpoints URIs or the token validation parameters explicitly.
            //    Authority = "https://lykke-auth-dev.azurewebsites.net",
            //    Scope = { "email" }
            //});

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
