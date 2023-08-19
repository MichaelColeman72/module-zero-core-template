using AbpCompanyName.AbpProjectName.Authorization.Users;
using AutoMapper;

namespace AbpCompanyName.AbpProjectName.Users.Dto
{
    public class UserMapProfile : Profile
    {
        public UserMapProfile()
        {
            _ = CreateMap<UserDto, User>();
            _ = CreateMap<UserDto, User>()
                .ForMember(x => x.Roles, opt => opt.Ignore())
                .ForMember(x => x.CreationTime, opt => opt.Ignore());

            _ = CreateMap<CreateUserDto, User>();
            _ = CreateMap<CreateUserDto, User>().ForMember(x => x.Roles, opt => opt.Ignore());
        }
    }
}
