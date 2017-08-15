using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using System.Security.Claims;
using ShmotoActvSync.Services;

namespace ShmotoActvSync
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDbService, DbService>();
            services.AddAuthentication(options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookies",
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                ExpireTimeSpan = TimeSpan.FromDays(30),
                SlidingExpiration = true
            });

            app.UseOAuthAuthentication(new OAuthOptions {
                AuthenticationScheme = "Strava",
                ClientId = "5772",
                ClientSecret = "4dee57ffb053d47bf2bceab72c97060cf1f1133b",
                CallbackPath = new Microsoft.AspNetCore.Http.PathString("/strava/callback"),
                AuthorizationEndpoint = "https://www.strava.com/oauth/authorize",
                TokenEndpoint = "https://www.strava.com/oauth/token",
                Scope = { "write" },
                SignInScheme = "Cookies",
                Events = new OAuthEvents {
                    OnCreatingTicket = async context =>
                    {
                        var userName = context.TokenResponse.Response["athlete"].Value<string>("username");
                        var id = context.TokenResponse.Response["athlete"].Value<string>("id");
                        context.Identity.AddClaim(new Claim(ClaimTypes.Name, userName));
                        context.Identity.AddClaim(new Claim("strava_id", id));
                        var dbService = new DbService();
                        dbService.AddUser(new Models.User { StravaId = long.Parse(id), StravaUserName = userName, StravaToken = context.AccessToken });

                        context.Ticket.Properties.IsPersistent = true; // persistent cookie
                    }
                }
                
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
