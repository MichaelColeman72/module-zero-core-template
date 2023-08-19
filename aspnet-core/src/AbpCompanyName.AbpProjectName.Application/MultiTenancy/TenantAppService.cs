using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using AbpCompanyName.AbpProjectName.Authorization;
using AbpCompanyName.AbpProjectName.Authorization.Roles;
using AbpCompanyName.AbpProjectName.Authorization.Users;
using AbpCompanyName.AbpProjectName.Editions;
using AbpCompanyName.AbpProjectName.MultiTenancy.Dto;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace AbpCompanyName.AbpProjectName.MultiTenancy
{
    [AbpAuthorize(PermissionNames.PagesTenants)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1501:Avoid excessive inheritance", Justification = "By design")]
    public class TenantAppService : AsyncCrudAppService<Tenant, TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>, ITenantAppService
    {
        private readonly TenantManager _tenantManager;
        private readonly EditionManager _editionManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;

        public TenantAppService(
            IRepository<Tenant, int> repository,
            TenantManager tenantManager,
            EditionManager editionManager,
            UserManager userManager,
            RoleManager roleManager,
            IAbpZeroDbMigrator abpZeroDbMigrator)
            : base(repository)
        {
            _tenantManager = tenantManager;
            _editionManager = editionManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _abpZeroDbMigrator = abpZeroDbMigrator;
        }

        public override async Task<TenantDto> CreateAsync(CreateTenantDto input)
        {
            CheckCreatePermission();

            // Create tenant
            var tenant = ObjectMapper.Map<Tenant>(input);
            tenant.ConnectionString = input.ConnectionString.IsNullOrEmpty()
                ? null
                : SimpleStringCipher.Instance.Encrypt(input.ConnectionString);

            var defaultEdition = await _editionManager.FindByNameAsync(EditionManager.DefaultEditionName).ConfigureAwait(false);
            if (defaultEdition != null)
            {
                tenant.EditionId = defaultEdition.Id;
            }

            await _tenantManager.CreateAsync(tenant).ConfigureAwait(false);
            await CurrentUnitOfWork.SaveChangesAsync().ConfigureAwait(false); // To get new tenant's id.

            // Create tenant database
            _abpZeroDbMigrator.CreateOrMigrateForTenant(tenant);

            // We are working entities of new tenant, so changing tenant filter
            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                // Create static roles for new tenant
                CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id).ConfigureAwait(false));

                await CurrentUnitOfWork.SaveChangesAsync().ConfigureAwait(false); // To get static role ids

                // Grant all permissions to admin role
                var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                await _roleManager.GrantAllPermissionsAsync(adminRole).ConfigureAwait(false);

                // Create admin user for the tenant
                var adminUser = User.CreateTenantAdminUser(tenant.Id, input.AdminEmailAddress);
                await _userManager.InitializeOptionsAsync(tenant.Id).ConfigureAwait(false);
                CheckErrors(await _userManager.CreateAsync(adminUser, User.DefaultPassword).ConfigureAwait(false));
                await CurrentUnitOfWork.SaveChangesAsync().ConfigureAwait(false); // To get admin user's id

                // Assign admin user to role!
                CheckErrors(await _userManager.AddToRoleAsync(adminUser, adminRole.Name).ConfigureAwait(false));
                await CurrentUnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }

            return MapToEntityDto(tenant);
        }

        public override async Task DeleteAsync(EntityDto<int> input)
        {
            CheckDeletePermission();

            var tenant = await _tenantManager.GetByIdAsync(input.Id).ConfigureAwait(false);
            await _tenantManager.DeleteAsync(tenant).ConfigureAwait(false);
        }

        protected override IQueryable<Tenant> CreateFilteredQuery(PagedTenantResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.TenancyName.Contains(input.Keyword) || x.Name.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        protected override void MapToEntity(TenantDto updateInput, Tenant entity)
        {
            // Manually mapped since TenantDto contains non-editable properties too.
            entity.Name = updateInput.Name;
            entity.TenancyName = updateInput.TenancyName;
            entity.IsActive = updateInput.IsActive;
        }

        private void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
