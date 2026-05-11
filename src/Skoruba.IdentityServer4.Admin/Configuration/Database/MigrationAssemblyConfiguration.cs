using System.Reflection;
using PostgreSQLMigrationAssembly = Skoruba.IdentityServer4.Admin.EntityFramework.PostgreSQL.Helpers.MigrationAssembly;

namespace Skoruba.IdentityServer4.Admin.Configuration.Database
{
    public static class MigrationAssemblyConfiguration
    {
        public static string GetMigrationAssemblyByProvider()
        {
            return typeof(PostgreSQLMigrationAssembly).GetTypeInfo().Assembly.GetName().Name;
        }
    }
}