using AbpCompanyName.AbpProjectName.Sessions.Dto;

#pragma warning disable CA1716
namespace AbpCompanyName.AbpProjectName.Web.Views.Shared.Components.RightNavbarUserArea
{
    public class RightNavbarUserAreaViewModel
    {
        public GetCurrentLoginInformationsOutput LoginInformations { get; set; }

        public bool IsMultiTenancyEnabled { get; set; }

        public string ShownLoginName
        {
            get
            {
                var userName = LoginInformations.User.UserName;

                return !IsMultiTenancyEnabled
                    ? userName
                    : LoginInformations.Tenant == null
                    ? ".\\" + userName
                    : LoginInformations.Tenant.TenancyName + "\\" + userName;
            }
        }
    }
}
