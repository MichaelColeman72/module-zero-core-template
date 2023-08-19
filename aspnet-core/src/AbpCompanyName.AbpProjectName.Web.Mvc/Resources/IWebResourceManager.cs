using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;

namespace AbpCompanyName.AbpProjectName.Web.Resources
{
    public interface IWebResourceManager
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "By design")]
        void AddScript(string url, bool addMinifiedOnProd = true);

        IReadOnlyList<string> GetScripts();

        HelperResult RenderScripts();
    }
}
