using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Dependency;
using AbpCompanyName.AbpProjectName.Authentication.JwtBearer;
using AbpCompanyName.AbpProjectName.Configuration;
using AbpCompanyName.AbpProjectName.EntityFrameworkCore;
using AbpCompanyName.AbpProjectName.Identity;
using AbpCompanyName.AbpProjectName.Web.Resources;
using AbpCompanyName.AbpProjectName.Web.Startup;
using Castle.MicroKernel.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace AbpCompanyName.AbpProjectName.Web.Tests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1724: Type names should not match namespaces", Justification = "By design")]
    public class Startup
    {
        private readonly IConfigurationRoot _appConfiguration;

        public Startup(IWebHostEnvironment env)
        {
            _appConfiguration = env.GetAppConfiguration();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "By design")]
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            UseInMemoryDb(app.ApplicationServices);

            app.UseAbp(); // Initializes ABP framework.

            _ = app.UseExceptionHandler("/Error");

            _ = app.UseStaticFiles();
            _ = app.UseRouting();

            _ = app.UseAuthentication();

            _ = app.UseJwtTokenMiddleware();

            _ = app.UseAuthorization();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            _ = services.AddEntityFrameworkInMemoryDatabase();

            _ = services.AddMvc();

            _ = IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);

            _ = services.AddScoped<IWebResourceManager, WebResourceManager>();

            // Configure Abp and Dependency Injection
            return services.AddAbp<AbpProjectNameWebTestModule>(options =>
            {
                options.SetupTest();
            });
        }

        private static void UseInMemoryDb(IServiceProvider serviceProvider)
        {
            var builder = new DbContextOptionsBuilder<AbpProjectNameDbContext>();
            _ = builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).UseInternalServiceProvider(serviceProvider);
            var options = builder.Options;

            var iocManager = serviceProvider.GetRequiredService<IIocManager>();
            _ = iocManager.IocContainer
                .Register(
                    Component.For<DbContextOptions<AbpProjectNameDbContext>>()
                        .Instance(options)
                        .LifestyleSingleton());
        }
    }
}