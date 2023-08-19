using Abp.Dependency;
using AbpCompanyName.AbpProjectName.EntityFrameworkCore;
using AbpCompanyName.AbpProjectName.Identity;
using Castle.MicroKernel.Registration;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AbpCompanyName.AbpProjectName.Tests.DependencyInjection
{
    public static class ServiceCollectionRegistrar
    {
        public static void Register(IIocManager iocManager)
        {
            var services = new ServiceCollection();

            _ = IdentityRegistrar.Register(services);

            _ = services.AddEntityFrameworkInMemoryDatabase();

            var serviceProvider = WindsorRegistrationHelper.CreateServiceProvider(iocManager.IocContainer, services);

            var builder = new DbContextOptionsBuilder<AbpProjectNameDbContext>();
            _ = builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).UseInternalServiceProvider(serviceProvider);

            _ = iocManager.IocContainer.Register(
                Component
                    .For<DbContextOptions<AbpProjectNameDbContext>>()
                    .Instance(builder.Options)
                    .LifestyleSingleton());
        }
    }
}
