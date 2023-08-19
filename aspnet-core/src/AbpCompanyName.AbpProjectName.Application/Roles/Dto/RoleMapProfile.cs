using Abp.Authorization;
using Abp.Authorization.Roles;
using AbpCompanyName.AbpProjectName.Authorization.Roles;
using AutoMapper;
using System.Linq;

namespace AbpCompanyName.AbpProjectName.Roles.Dto
{
    public class RoleMapProfile : Profile
    {
        public RoleMapProfile()
        {
            // Role and permission
            CreateMap<Permission, string>().ConvertUsing(r => r.Name);
            CreateMap<RolePermissionSetting, string>().ConvertUsing(r => r.Name);

            _ = CreateMap<CreateRoleDto, Role>();

            _ = CreateMap<RoleDto, Role>();

            _ = CreateMap<Role, RoleDto>().ForMember(
                x => x.GrantedPermissions,
                opt => opt.MapFrom(x => x.Permissions.Where(p => p.IsGranted)));

            _ = CreateMap<Role, RoleListDto>();
            _ = CreateMap<Role, RoleEditDto>();
            _ = CreateMap<Permission, FlatPermissionDto>();
        }
    }
}
