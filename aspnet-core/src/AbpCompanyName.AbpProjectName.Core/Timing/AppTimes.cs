using Abp.Dependency;
using System;

namespace AbpCompanyName.AbpProjectName.Timing
{
    public class AppTimes : ISingletonDependency
    {
        /// <summary>
        /// Gets or sets the startup time of the application.
        /// </summary>
        public DateTime StartupTime { get; set; }
    }
}
