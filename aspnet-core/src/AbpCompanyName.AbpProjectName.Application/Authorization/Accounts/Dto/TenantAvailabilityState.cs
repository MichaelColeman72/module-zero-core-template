namespace AbpCompanyName.AbpProjectName.Authorization.Accounts.Dto
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = "By design")]
    public enum TenantAvailabilityState
    {
        /// <summary>
        /// Available
        /// </summary>
        Available = 1,

        /// <summary>
        /// InActive
        /// </summary>
        InActive = 2,

        /// <summary>
        /// NotFound
        /// </summary>
        NotFound = 3
    }
}
