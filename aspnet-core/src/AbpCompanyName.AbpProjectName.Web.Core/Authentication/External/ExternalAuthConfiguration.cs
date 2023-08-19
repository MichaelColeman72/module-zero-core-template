using Abp.Dependency;
using System.Collections.Generic;

namespace AbpCompanyName.AbpProjectName.Authentication.External
{
    public class ExternalAuthConfiguration : IExternalAuthConfiguration, ISingletonDependency
    {
        public ExternalAuthConfiguration() => Providers = new List<ExternalLoginProviderInfo>();

        public IReadOnlyList<ExternalLoginProviderInfo> Providers { get; }
    }
}
