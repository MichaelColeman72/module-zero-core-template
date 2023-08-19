using Abp.ObjectMapping;
using AbpCompanyName.AbpProjectName.Sessions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

#pragma warning disable CA1716
namespace AbpCompanyName.AbpProjectName.Web.Views.Shared.Components.TenantChange
{
    public class TenantChangeViewComponent : AbpProjectNameViewComponent
    {
        private readonly ISessionAppService _sessionAppService;
        private readonly IObjectMapper _objectMapper;

        public TenantChangeViewComponent(ISessionAppService sessionAppService, IObjectMapper objectMapper)
        {
            _sessionAppService = sessionAppService;
            _objectMapper = objectMapper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var loginInfo = await _sessionAppService.GetCurrentLoginInformations().ConfigureAwait(false);
            var model = _objectMapper.Map<TenantChangeViewModel>(loginInfo);
            return View(model);
        }
    }
}
