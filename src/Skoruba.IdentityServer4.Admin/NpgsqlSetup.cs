using System;
using System.Runtime.CompilerServices;

namespace Skoruba.IdentityServer4.Admin
{
    internal static class NpgsqlSetup
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}
