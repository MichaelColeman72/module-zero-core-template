using Abp.Configuration;
using Abp.Zero.Configuration;
using AbpCompanyName.AbpProjectName.Authorization.Accounts.Dto;
using AbpCompanyName.AbpProjectName.Authorization.Users;
using System.Threading.Tasks;

namespace AbpCompanyName.AbpProjectName.Authorization.Accounts
{
    public class AccountAppService : AbpProjectNameAppServiceBase, IAccountAppService
    {
        // from: http://regexlib.com/REDetails.aspx?regexp_id=1923
        public const string PasswordRegex = "(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$";

        private readonly UserRegistrationManager _userRegistrationManager;

        public AccountAppService(
            UserRegistrationManager userRegistrationManager)
        {
            _userRegistrationManager = userRegistrationManager;
        }

        public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input)
        {
            var tenant = await TenantManager.FindByTenancyNameAsync(input.TenancyName).ConfigureAwait(false);
            return tenant == null
                ? new IsTenantAvailableOutput(TenantAvailabilityState.NotFound)
                : !tenant.IsActive
                ? new IsTenantAvailableOutput(TenantAvailabilityState.InActive)
                : new IsTenantAvailableOutput(TenantAvailabilityState.Available, tenant.Id);
        }

        public async Task<RegisterOutput> Register(RegisterInput input)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                input.Name,
                input.Surname,
                input.EmailAddress,
                input.UserName,
                input.Password,
                true).ConfigureAwait(false); // Assumed email address is always confirmed. Change this if you want to implement email confirmation.

            var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin).ConfigureAwait(false);

            return new RegisterOutput
            {
                CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin)
            };
        }
    }
}
