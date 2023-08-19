using Abp.AspNetCore;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Castle.Logging.Log4Net;
using Abp.Dependency;
using Abp.Json;
using AbpCompanyName.AbpProjectName.Authentication.JwtBearer;
using AbpCompanyName.AbpProjectName.Configuration;
using AbpCompanyName.AbpProjectName.Identity;
using AbpCompanyName.AbpProjectName.Web.Resources;
using Castle.Facilities.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using Newtonsoft.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace AbpCompanyName.AbpProjectName.Web.Startup
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1724: Type names should not match namespaces", Justification = "By design")]
    public class Startup
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfigurationRoot _appConfiguration;

        public Startup(IWebHostEnvironment env)
        {
            _hostingEnvironment = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506: Avoid excessive class coupling", Justification = "By design")]
        public void ConfigureServices(IServiceCollection services)
        {
            // MVC
            _ = services.AddControllersWithViews(
                    options =>
                    {
                        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                        options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
                    })
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new AbpMvcContractResolver(IocManager.Instance)
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };
                });

            _ = IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);

            _ = services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            _ = services.AddScoped<IWebResourceManager, WebResourceManager>();

            _ = services.AddSignalR();

            // Configure Abp and Dependency Injection
            // Configure Log4Net logging
            services.AddAbpWithoutCreatingServiceProvider<AbpProjectNameWebMvcModule>(
                options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig(
                        _hostingEnvironment.IsDevelopment()
                            ? "log4net.config"
                            : "log4net.Production.config")));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "By design")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAbp(); // Initializes ABP framework.

            _ = env.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseExceptionHandler("/Error");

            _ = app.UseStaticFiles();

            _ = app.UseRouting();

            _ = app.UseAuthentication();

            _ = app.UseJwtTokenMiddleware();

            _ = app.UseAuthorization();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapHub<AbpCommonHub>("/signalr");
                _ = endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                _ = endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
