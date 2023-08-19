using AbpCompanyName.AbpProjectName.Roles.Dto;
using System.Collections.Generic;

namespace AbpCompanyName.AbpProjectName.Web.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "By design")]
        List<FlatPermissionDto> Permissions { get; set; }
    }
}