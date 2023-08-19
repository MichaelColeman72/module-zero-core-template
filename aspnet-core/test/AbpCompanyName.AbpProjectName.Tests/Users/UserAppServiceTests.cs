using AbpCompanyName.AbpProjectName.Users;
using AbpCompanyName.AbpProjectName.Users.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace AbpCompanyName.AbpProjectName.Tests.Users
{
    public class UserAppServiceTests : AbpProjectNameTestBase
    {
        private readonly IUserAppService _userAppService;

        public UserAppServiceTests()
        {
            _userAppService = Resolve<IUserAppService>();
        }

        [Fact]
        public async Task GetUsersTest()
        {
            // Act
            var output = await _userAppService.GetAllAsync(new PagedUserResultRequestDto { MaxResultCount = 20, SkipCount = 0 }).ConfigureAwait(false);

            // Assert
            output.Items.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task CreateUserTest()
        {
            // Act
            _ = await _userAppService.CreateAsync(
                new CreateUserDto
                {
                    EmailAddress = "john@volosoft.com",
                    IsActive = true,
                    Name = "John",
                    Surname = "Nash",
                    Password = "123qwe",
                    UserName = "john.nash"
                }).ConfigureAwait(false);

            await UsingDbContextAsync(async context =>
            {
                var johnNashUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "john.nash").ConfigureAwait(false);
                _ = johnNashUser.ShouldNotBeNull();
            }).ConfigureAwait(false);
        }
    }
}
