using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace AbpCompanyName.AbpProjectName.Authorization
{
    public class AbpProjectNameAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            _ = context.CreatePermission(PermissionNames.PagesUsers, L("Users"));
            _ = context.CreatePermission(PermissionNames.PagesUsersActivation, L("UsersActivation"));
            _ = context.CreatePermission(PermissionNames.PagesRoles, L("Roles"));
            _ = context.CreatePermission(PermissionNames.PagesTenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, AbpProjectNameConsts.LocalizationSourceName);
        }
    }
}
