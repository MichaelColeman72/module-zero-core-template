using Abp.AutoMapper;
using AbpCompanyName.AbpProjectName.Sessions.Dto;

#pragma warning disable CA1716
namespace AbpCompanyName.AbpProjectName.Web.Views.Shared.Components.TenantChange
{
    [AutoMapFrom(typeof(GetCurrentLoginInformationsOutput))]
    public class TenantChangeViewModel
    {
        public TenantLoginInfoDto Tenant { get; set; }
    }
}
