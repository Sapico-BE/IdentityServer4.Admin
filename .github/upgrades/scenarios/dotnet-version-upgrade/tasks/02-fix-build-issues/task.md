# 02-fix-build-issues: Fix any compilation errors from package updates

After package updates, fix any breaking API changes introduced by major version bumps (e.g., EF Core 6→10, Serilog, Swashbuckle, xunit 2→2.9).

**Done when**: `dotnet build` succeeds with 0 errors.
