using Sentinel.Configuration;
using Sentinel.Identity;
using Authentication.Models;
using AspNetCoreRateLimit;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentinel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Sentinel
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(Configuration["ConnectionStrings:ApplicationConnection"]))
                .AddDbContext<SentinelContext>(options =>
                options.UseSqlServer(Configuration["ConnectionStrings:ApplicationConnection"]));

            services.AddDefaultIdentity<User>()
                .AddRoles<Role>()
                .AddUserManager<LdapUserManager>()
                .AddRoleStore<LdapRoleStore>()
                .AddUserStore<LdapUserStore>()
                .AddSignInManager<LdapSignInManager>()
                .AddClaimsPrincipalFactory<LdapUserClaimsPrincipalFactory>();

            services.AddCsp(nonceByteAmount: 32);

            services.AddHttpClient();

            services.AddTransient<ILdapService, LdapService>();
            services.AddTransient<IUserStore<User>, LdapUserStore>();
            services.AddTransient<IRoleStore<Role>, LdapRoleStore>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();

            services.AddOptions();
            services.AddMemoryCache();

            // IP rate limting
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddHttpContextAccessor();

            services.Configure<ApplicationOptions>(Configuration.GetSection("ApplicationOptions"));

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            // Uncomment/edit this to set the session timeout interval
            //services.Configure<SecurityStampValidatorOptions>(options =>
            //    options.ValidationInterval = TimeSpan.FromMinutes(1));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts(new Joonasw.AspNetCore.SecurityHeaders.HstsOptions
                {
                    IncludeSubDomains = true // Uses Joonasw.AspNetCore.SecurityHeaders
                });
            }
            app.UseIpRateLimiting();

            app.UseCsp(csp =>
            {
                csp.AllowScripts
                        .FromSelf()
                        .AddNonce()
                        .From("ajax.aspnetcdn.com")
                        .AllowUnsafeInline(); // NOTE - this will be IGNORED by browsers that recognise the nonce but is needed for Safari compatibility
                csp.AllowStyles
                        .FromSelf()
                        .From("ajax.aspnetcdn.com")
                        .AllowUnsafeInline(); // Needed for font-awesome
            }); // Content-Security-Policy, uses Joonasw.AspNetCore.SecurityHeaders

            app.UseHttpsRedirection();

            // Set a max-age header for static assets
            // Note that this won't stop site.js/site.css from updating correctly
            // as they are referenced with asp-append-version="true"
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    const int durationInSeconds = 60 * 60 * 24 * 30; // 30 days
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
                }
            });
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors("CorsPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
