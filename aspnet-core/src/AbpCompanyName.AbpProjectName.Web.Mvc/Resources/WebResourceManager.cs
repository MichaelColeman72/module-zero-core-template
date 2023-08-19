using Abp.Collections.Extensions;
using Abp.Extensions;
using Abp.Timing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace AbpCompanyName.AbpProjectName.Web.Resources
{
    public class WebResourceManager : IWebResourceManager
    {
        private readonly IWebHostEnvironment _environment;
        private readonly List<string> _scriptUrls;

        public WebResourceManager(IWebHostEnvironment environment)
        {
            _environment = environment;
            _scriptUrls = new List<string>();
        }

        public void AddScript(string url, bool addMinifiedOnProd = true)
        {
            _ = _scriptUrls.AddIfNotContains(NormalizeUrl(url, "js"));
        }

        public IReadOnlyList<string> GetScripts()
        {
            return _scriptUrls.ToImmutableList();
        }

        public HelperResult RenderScripts()
        {
            return new HelperResult(async writer =>
            {
                foreach (var scriptUrl in _scriptUrls)
                {
                    await writer.WriteAsync($"<script src=\"{scriptUrl}?v=" + Clock.Now.Ticks + "\"></script>").ConfigureAwait(false);
                }
            });
        }

        public void AddScript(System.Uri url, bool addMinifiedOnProd = true)
        {
            throw new System.NotImplementedException();
        }

        private string NormalizeUrl(string url, string ext)
        {
            return _environment.IsDevelopment() ? url : url.EndsWith($".min.{ext}", System.StringComparison.OrdinalIgnoreCase) ? url : url.Left(url.Length - ext.Length) + $"min.{ext}";
        }
    }
}
