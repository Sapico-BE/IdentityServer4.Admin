# .NET 10.0 Package Update Plan

## Overview

**Target**: Update all NuGet packages to latest stable net10.0-compatible versions
**Scope**: 21 projects, ~59 packages. Projects already target net10.0 — this is a package-only update.

### Selected Strategy
**All-At-Once** — All projects updated simultaneously in a single operation.
**Rationale**: 21 projects, all on net10.0, zero breaking changes, straightforward package bumps.

### License Constraints
- **AutoMapper**: Keep at 12.0.1 (v13+ changed to commercial license)
- **FluentAssertions**: Keep at 6.x (v7+ changed to commercial license)
- **IdentityServer4**: Keep as-is (do NOT migrate to Duende)
- Skip preview/dev-suffixed versions where stable alternatives exist

## Tasks

### 01-update-packages: Update all NuGet packages across the solution

Update all 59 NuGet packages to their latest stable net10.0-compatible versions. Key updates include Microsoft.* packages from 6.0.x to 10.0.x, Serilog ecosystem, HealthChecks, Swashbuckle, xunit, and Azure SDK packages.

Packages kept at current versions due to license constraints: AutoMapper (12.0.1), FluentAssertions (6.4.0).
Packages kept at current versions because no newer stable version exists: NWebsec.AspNetCore.Middleware (3.0.0), IdentityServer4.* (4.1.2), Skoruba.AuditLogging.EntityFramework (1.0.0).

**Done when**: All .csproj files updated with latest stable compatible package versions, `dotnet restore` succeeds.

---

### 02-fix-build-issues: Fix any compilation errors from package updates

After package updates, fix any breaking API changes introduced by major version bumps (e.g., EF Core 6→10, Serilog, Swashbuckle, xunit 2→2.9).

**Done when**: `dotnet build` succeeds with 0 errors.

---

### 03-docker-build-verification: Verify build via docker-compose

Build the solution using the docker-compose project to verify everything compiles correctly in the containerized environment.

**Done when**: `docker-compose build` completes successfully with 0 errors.
