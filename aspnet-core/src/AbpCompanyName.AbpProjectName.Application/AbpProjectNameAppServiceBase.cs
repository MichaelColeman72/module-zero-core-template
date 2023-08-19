using Abp.Application.Services;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using AbpCompanyName.AbpProjectName.Authorization.Users;
using AbpCompanyName.AbpProjectName.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace AbpCompanyName.AbpProjectName
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class AbpProjectNameAppServiceBase : ApplicationService
    {
        protected AbpProjectNameAppServiceBase()
        {
            LocalizationSourceName = AbpProjectNameConsts.LocalizationSourceName;
        }

        public TenantManager TenantManager { get; set; }

        public UserManager UserManager { get; set; }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
            return user ?? throw new Exception("There is no current user!");
        }

        protected virtual Task<Tenant> GetCurrentTenantAsync()
        {
            return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
