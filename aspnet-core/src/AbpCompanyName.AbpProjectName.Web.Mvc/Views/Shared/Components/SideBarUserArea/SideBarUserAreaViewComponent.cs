using Abp.Configuration.Startup;
using AbpCompanyName.AbpProjectName.Sessions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

#pragma warning disable CA1716
namespace AbpCompanyName.AbpProjectName.Web.Views.Shared.Components.SideBarUserArea
{
    public class SideBarUserAreaViewComponent : AbpProjectNameViewComponent
    {
        private readonly ISessionAppService _sessionAppService;
        private readonly IMultiTenancyConfig _multiTenancyConfig;

        public SideBarUserAreaViewComponent(
            ISessionAppService sessionAppService,
            IMultiTenancyConfig multiTenancyConfig)
        {
            _sessionAppService = sessionAppService;
            _multiTenancyConfig = multiTenancyConfig;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new SideBarUserAreaViewModel
            {
                LoginInformations = await _sessionAppService.GetCurrentLoginInformations().ConfigureAwait(false),
                IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled,
            };

            return View(model);
        }
    }
}
