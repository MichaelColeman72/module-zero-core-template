using System;
using System.Collections.Generic;

namespace AbpCompanyName.AbpProjectName.Sessions.Dto
{
    public class ApplicationInfoDto
    {
        public string Version { get; set; }

        public DateTime ReleaseDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "By design")]
        public Dictionary<string, bool> Features { get; set; }
    }
}
