namespace Skoruba.IdentityServer4.Admin.UnitTests.Helpers
{
    /// <summary>
    /// No longer needed — Cnblogs.IdentityServer4.* packages are compiled against AutoMapper 13
    /// and work natively on .NET 10. Kept as a no-op for backward compatibility.
    /// </summary>
    public static class AutoMapperNet10Fix
    {
        public static void Apply()
        {
            // No-op: Cnblogs.IdentityServer4 packages resolve the AutoMapper incompatibility.
        }
    }
}
