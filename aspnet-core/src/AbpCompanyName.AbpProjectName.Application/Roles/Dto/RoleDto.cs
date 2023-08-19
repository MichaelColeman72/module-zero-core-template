using Abp.Application.Services.Dto;
using Abp.Authorization.Roles;
using AbpCompanyName.AbpProjectName.Authorization.Roles;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AbpCompanyName.AbpProjectName.Roles.Dto
{
    public class RoleDto : EntityDto<int>
    {
        [Required]
        [StringLength(AbpRoleBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(AbpRoleBase.MaxDisplayNameLength)]
        public string DisplayName { get; set; }

        public string NormalizedName { get; set; }

        [StringLength(Role.MaxDescriptionLength)]
        public string Description { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "By design")]
        public List<string> GrantedPermissions { get; set; }
    }
}