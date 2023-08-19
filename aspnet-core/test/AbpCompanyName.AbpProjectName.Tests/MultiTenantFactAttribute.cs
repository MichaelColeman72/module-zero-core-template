using Xunit;

namespace AbpCompanyName.AbpProjectName.Tests
{
    public sealed class MultiTenantFactAttribute : FactAttribute
    {
        public MultiTenantFactAttribute() => Skip = AbpProjectNameConsts.MultiTenancyEnabled ? string.Empty : "MultiTenancy is disabled.";
    }
}
