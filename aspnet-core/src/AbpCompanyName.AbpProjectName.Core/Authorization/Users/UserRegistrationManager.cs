using Abp.Authorization.Users;
using Abp.Domain.Services;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Abp.UI;
using AbpCompanyName.AbpProjectName.Authorization.Roles;
using AbpCompanyName.AbpProjectName.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AbpCompanyName.AbpProjectName.Authorization.Users
{
    public class UserRegistrationManager : DomainService
    {
        private readonly TenantManager _tenantManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserRegistrationManager(
            TenantManager tenantManager,
            UserManager userManager,
            RoleManager roleManager,
            IPasswordHasher<User> passwordHasher)
        {
            _tenantManager = tenantManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;

            AbpSession = NullAbpSession.Instance;
        }

        public IAbpSession AbpSession { get; set; }

        public async Task<User> RegisterAsync(string name, string surname, string emailAddress, string userName, string plainPassword, bool isEmailConfirmed)
        {
            CheckForTenant();

            var tenant = await GetActiveTenantAsync().ConfigureAwait(false);

            var user = new User
            {
                TenantId = tenant.Id,
                Name = name,
                Surname = surname,
                EmailAddress = emailAddress,
                IsActive = true,
                UserName = userName,
                IsEmailConfirmed = isEmailConfirmed,
                Roles = new List<UserRole>()
            };

            user.SetNormalizedNames();

            foreach (var defaultRole in await _roleManager.Roles.Where(r => r.IsDefault).ToListAsync().ConfigureAwait(false))
            {
                user.Roles.Add(new UserRole(tenant.Id, user.Id, defaultRole.Id));
            }

            await _userManager.InitializeOptionsAsync(tenant.Id).ConfigureAwait(false);

            CheckErrors(await _userManager.CreateAsync(user, plainPassword).ConfigureAwait(false));
            await CurrentUnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            return user;
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        private void CheckForTenant()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                throw new InvalidOperationException("Can not register host users!");
            }
        }

        private async Task<Tenant> GetActiveTenantAsync()
        {
            return !AbpSession.TenantId.HasValue ? null : await GetActiveTenantAsync(AbpSession.TenantId.Value).ConfigureAwait(false);
        }

        private async Task<Tenant> GetActiveTenantAsync(int tenantId)
        {
            var tenant = await _tenantManager.FindByIdAsync(tenantId).ConfigureAwait(false);
            return tenant == null
                ? throw new UserFriendlyException(L("UnknownTenantId{0}", tenantId))
                : !tenant.IsActive ? throw new UserFriendlyException(L("TenantIdIsNotActive{0}", tenantId)) : tenant;
        }
    }
}
