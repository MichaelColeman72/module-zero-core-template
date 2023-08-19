using Abp.MultiTenancy;

namespace AbpCompanyName.AbpProjectName.Web.Models.Account
{
    public class LoginFormViewModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "By design")]
        public string ReturnUrl { get; set; }

        public bool IsMultiTenancyEnabled { get; set; }

        public bool IsSelfRegistrationAllowed { get; set; }

        public MultiTenancySides MultiTenancySide { get; set; }
    }
}
