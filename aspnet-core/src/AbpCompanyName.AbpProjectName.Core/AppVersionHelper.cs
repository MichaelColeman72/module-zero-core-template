using Abp.Reflection.Extensions;
using System;
using System.IO;

namespace AbpCompanyName.AbpProjectName
{
    /// <summary>
    /// Central point for application version.
    /// </summary>
    public static class AppVersionHelper
    {
        /// <summary>
        /// Gets current version of the application.
        /// It's also shown in the web page.
        /// </summary>
        public const string Version = "8.0.0.0";

        private static readonly Lazy<DateTime> _lzyReleaseDate = new (() => new FileInfo(typeof(AppVersionHelper).GetAssembly().Location).LastWriteTime);

        /// <summary>
        /// Gets release (last build) date of the application.
        /// It's shown in the web page.
        /// </summary>
        public static DateTime ReleaseDate => _lzyReleaseDate.Value;
    }
}
