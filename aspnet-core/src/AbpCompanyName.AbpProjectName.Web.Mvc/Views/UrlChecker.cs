using System;
using System.Text.RegularExpressions;

namespace AbpCompanyName.AbpProjectName.Web.Views
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "By design")]
    public static partial class UrlChecker
    {
        private static readonly Regex _urlWithProtocolRegex = UrlWithProtocolRegex();

        public static bool IsRooted(string url) => url.StartsWith("/", System.StringComparison.OrdinalIgnoreCase) || _urlWithProtocolRegex.IsMatch(url);

        public static bool IsRooted(Uri url) => throw new System.NotImplementedException();

        [GeneratedRegex("^.{1,10}://.*$")]
        private static partial Regex UrlWithProtocolRegex();
    }
}
