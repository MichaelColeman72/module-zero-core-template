using System.Collections.Generic;

namespace AbpCompanyName.AbpProjectName.Authentication.External
{
    public interface IExternalAuthConfiguration
    {
        IReadOnlyList<ExternalLoginProviderInfo> Providers { get; }
    }
}
