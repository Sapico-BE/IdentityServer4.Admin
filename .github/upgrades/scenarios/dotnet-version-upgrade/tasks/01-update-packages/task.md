# 01-update-packages: Update all NuGet packages across the solution

Update all 59 NuGet packages to their latest stable net10.0-compatible versions. Key updates include Microsoft.* packages from 6.0.x to 10.0.x, Serilog ecosystem, HealthChecks, Swashbuckle, xunit, and Azure SDK packages.

Packages kept at current versions due to license constraints: AutoMapper (12.0.1), FluentAssertions (6.4.0).
Packages kept at current versions because no newer stable version exists: NWebsec.AspNetCore.Middleware (3.0.0), IdentityServer4.* (4.1.2), Skoruba.AuditLogging.EntityFramework (1.0.0).

**Done when**: All .csproj files updated with latest stable compatible package versions, `dotnet restore` succeeds.
