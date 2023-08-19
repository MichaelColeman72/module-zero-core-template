using Abp.MultiTenancy;
using AbpCompanyName.AbpProjectName.Authorization.Users;

namespace AbpCompanyName.AbpProjectName.MultiTenancy
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1501:Avoid excessive inheritance", Justification = "By design")]
    public class Tenant : AbpTenant<User>
    {
        public Tenant()
        {
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
