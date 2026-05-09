# Sapico-Specific Configuration

This document describes customizations specific to the Sapico deployment of IdentityServer4.Admin.

## Database Configuration

### Default Provider: PostgreSQL

The Sapico deployment uses **PostgreSQL** as the default database provider instead of SQL Server.

**Changes made:**

1. **appsettings.json files** – Updated `DatabaseProviderConfiguration.ProviderType` to `PostgreSQL`:
   - `src/Skoruba.IdentityServer4.Admin/appsettings.json`
   - `src/Skoruba.IdentityServer4.Admin.Api/appsettings.json`
   - `src/Skoruba.IdentityServer4.STS.Identity/appsettings.json`
   - `templates/template-publish/content/src/SkorubaIdentityServer4Admin.STS.Identity/appsettings.json`

2. **Connection Strings** – Updated all connection strings to PostgreSQL format:
   ```
   Server=localhost;Database=IdentityServer4Admin;User Id=postgres;Password=postgres
   ```

3. **Docker Compose** – Updated `docker-compose.yml`:
   - Database service now uses `postgres:15-alpine` image
   - Port mapping: `5432:5432` (PostgreSQL default port)
   - Environment variables:
     - `POSTGRES_DB=IdentityServer4Admin`
     - `POSTGRES_USER=postgres`
     - `POSTGRES_PASSWORD=${DB_PASSWORD:-postgres}` (default: `postgres`)
   - All container connection strings use PostgreSQL credentials

### Local Development

To run locally with the Sapico configuration:

```powershell
cd d:\source\IdentityServer4.Admin
docker-compose up --build
```

Environment variables can be overridden:
- `DB_PASSWORD` – PostgreSQL password (default: `postgres`)
- `DOCKER_REGISTRY` – Docker registry prefix (default: empty)

### Connection String Examples

**Admin Service:**
```
Server=db;Database=IdentityServer4Admin;User Id=postgres;Password=postgres;
```

**appsettings.json (local):**
```json
{
  "ConnectionStrings": {
    "ConfigurationDbConnection": "Server=localhost;Database=IdentityServer4Admin;User Id=postgres;Password=postgres"
  }
}
```

## Runtime Environment

### Hostnames (Docker)

- **Admin UI**: `https://admin.skoruba.local`
- **Admin API**: `https://admin-api.skoruba.local`
- **STS**: `https://sts.skoruba.local`

### Local Development Ports

- Admin UI: `https://localhost:44303`
- Admin API: `https://localhost:44302`
- STS: `https://localhost:44310`
- PostgreSQL: `localhost:5432`

## .NET Target Framework

All projects target **.NET 10** (`net10.0`):
- Web hosts use `net10.0`
- Class libraries use `net10.0`
- Test projects use `net10.0`

Docker images:
- SDK: `mcr.microsoft.com/dotnet/sdk:10.0`
- Runtime: `mcr.microsoft.com/dotnet/aspnet:10.0`

## Database Migrations

To add migrations for PostgreSQL:

```powershell
# Using the build script with PostgreSQL provider
cd .\build
.\add-migrations.ps1 -provider PostgreSQL
```

Or manually:

```powershell
dotnet ef migrations add InitialCreate \
  --project src/Skoruba.IdentityServer4.Admin.EntityFramework.PostgreSQL \
  --startup-project src/Skoruba.IdentityServer4.Admin \
  --context IdentityServerConfigurationDbContext
```

## Key Changes Summary

| Aspect | Before | After |
|--------|--------|-------|
| **Database** | SQL Server (localdb or mssql/server) | PostgreSQL 15 |
| **Default Provider** | SqlServer | PostgreSQL |
| **Connection Format** | `Server=(localdb)\mssqllocaldb;...` | `Server=localhost;User Id=postgres;...` |
| **.NET Target** | net6.0 | net10.0 |
| **Docker SDK/Runtime** | 6.0 | 10.0 |
| **DB Default Password** | `Password_123` | `postgres` |
| **DB Port (Docker)** | 7900:1433 | 5432:5432 |

## Override for SQL Server

To revert to SQL Server (not recommended for Sapico):

1. Update `DatabaseProviderConfiguration.ProviderType` to `SqlServer` in appsettings files
2. Update connection strings to SQL Server format:
   ```
   Server=db;Database=IdentityServer4Admin;User Id=sa;Password=Password_123;
   ```
3. Update `docker-compose.yml` to use SQL Server image

## Troubleshooting

### Database Connection Issues

If containers fail to connect:
- Verify PostgreSQL is running: `docker ps | grep postgres`
- Check credentials in docker-compose environment variables
- Verify database exists: `SELECT datname FROM pg_database;`

### Migration Issues

If migrations fail:
- Ensure correct provider is selected in `DatabaseProviderConfiguration`
- Verify migration assembly is included for PostgreSQL provider
- Check that `User Id=postgres` (not `sa`) in connection strings

