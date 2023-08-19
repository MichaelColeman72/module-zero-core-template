using Abp;
using Abp.Castle.Logging.Log4Net;
using Abp.Collections.Extensions;
using Abp.Dependency;
using Castle.Facilities.Logging;
using System;

namespace AbpCompanyName.AbpProjectName.Migrator
{
    public static class Program
    {
        private static bool _quietMode;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "By design")]
        public static void Main(string[] args)
        {
            ParseArgs(args);

            using var bootstrapper = AbpBootstrapper.Create<AbpProjectNameMigratorModule>();
            _ = bootstrapper.IocManager.IocContainer
                .AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig("log4net.config"));

            bootstrapper.Initialize();

            using var migrateExecuter = bootstrapper.IocManager.ResolveAsDisposable<MultiTenantMigrateExecuter>();
            var migrationSucceeded = migrateExecuter.Object.Run(_quietMode);

            if (_quietMode)
            {
                // exit clean (with exit code 0) if migration is a success, otherwise exit with code 1
                var exitCode = Convert.ToInt32(!migrationSucceeded);
                Environment.Exit(exitCode);
            }
            else
            {
                Console.WriteLine("Press ENTER to exit...");
                _ = Console.ReadLine();
            }
        }

        private static void ParseArgs(string[] args)
        {
            if (args.IsNullOrEmpty())
            {
                return;
            }

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "-q":
                        _quietMode = true;
                        break;
                }
            }
        }
    }
}
