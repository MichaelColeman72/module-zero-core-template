using AbpCompanyName.AbpProjectName.Sessions;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace AbpCompanyName.AbpProjectName.Tests.Sessions
{
    public class SessionAppServiceTests : AbpProjectNameTestBase
    {
        private readonly ISessionAppService _sessionAppService;

        public SessionAppServiceTests()
        {
            _sessionAppService = Resolve<ISessionAppService>();
        }

        [MultiTenantFact]
        public async Task ShouldGetCurrentUserWhenLoggedInAsHost()
        {
            // Arrange
            LoginAsHostAdmin();

            // Act
            var output = await _sessionAppService.GetCurrentLoginInformations().ConfigureAwait(false);

            // Assert
            var currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
            output.User.ShouldNotBe(null);
            output.User.Name.ShouldBe(currentUser.Name);
            output.User.Surname.ShouldBe(currentUser.Surname);

            output.Tenant.ShouldBe(null);
        }

        [Fact]
        public async Task ShouldGetCurrentUserAndTenantWhenLoggedInAsTenant()
        {
            // Act
            var output = await _sessionAppService.GetCurrentLoginInformations().ConfigureAwait(false);

            // Assert
            var currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
            var currentTenant = await GetCurrentTenantAsync().ConfigureAwait(false);

            output.User.ShouldNotBe(null);
            output.User.Name.ShouldBe(currentUser.Name);

            output.Tenant.ShouldNotBe(null);
            output.Tenant.Name.ShouldBe(currentTenant.Name);
        }
    }
}
