﻿using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.TestBase;
using Abp.Zero.Configuration;
using Abp.Zero.EntityFrameworkCore;
using AbpCompanyName.AbpProjectName.EntityFrameworkCore;
using AbpCompanyName.AbpProjectName.Tests.DependencyInjection;
using Castle.MicroKernel.Registration;
using NSubstitute;
using System;

namespace AbpCompanyName.AbpProjectName.Tests
{
    [DependsOn(
        typeof(AbpProjectNameApplicationModule),
        typeof(AbpProjectNameEntityFrameworkModule),
        typeof(AbpTestBaseModule))]
    public class AbpProjectNameTestModule : AbpModule
    {
        public AbpProjectNameTestModule(AbpProjectNameEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
            abpProjectNameEntityFrameworkModule.SkipDbSeed = true;
        }

        public override void PreInitialize()
        {
            Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
            Configuration.UnitOfWork.IsTransactional = false;

            // Disable static mapper usage since it breaks unit tests (see https://github.com/aspnetboilerplate/aspnetboilerplate/issues/2052)
#pragma warning disable CS0618 // Type or member is obsolete
            Configuration.Modules.AbpAutoMapper().UseStaticMapper = false;
#pragma warning restore CS0618 // Type or member is obsolete

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

            // Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            RegisterFakeService<AbpZeroDbMigrator<AbpProjectNameDbContext>>();

            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
        }

        public override void Initialize()
        {
            ServiceCollectionRegistrar.Register(IocManager);
        }

        private void RegisterFakeService<TService>()
            where TService : class
        {
            _ = IocManager.IocContainer.Register(
                Component.For<TService>()
                    .UsingFactoryMethod(() => Substitute.For<TService>())
                    .LifestyleSingleton());
        }
    }
}
