using AbpCompanyName.AbpProjectName.Models.TokenAuth;
using AbpCompanyName.AbpProjectName.Web.Controllers;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace AbpCompanyName.AbpProjectName.Web.Tests.Controllers
{
    public class HomeControllerTests : AbpProjectNameWebTestBase
    {
        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "By design")]
        public async Task IndexTest()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            }).ConfigureAwait(false);

            // Act
            var response = await GetResponseAsStringAsync(GetUrl<HomeController>(nameof(HomeController.Index))).ConfigureAwait(false);

            // Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}