# AGENTS.md — Sapico IdentityServer4 Admin

> Forked from [skoruba/IdentityServer4.Admin](https://github.com/skoruba/IdentityServer4.Admin), migrated to **.NET 10** and rebranded for **Sapico**.

## Purpose

This document is a quick orientation guide for coding agents and contributors working in this repository.

It focuses on:

- solution and project structure
- runtime website structure (domains, app boundaries, and key paths)
- responsibilities of each host application

## Solution Structure

Main solution:

- `Skoruba.IdentityServer4.Admin.sln`

Primary source folders:

- `src/Skoruba.IdentityServer4.Admin` - Admin MVC host app
- `src/Skoruba.IdentityServer4.Admin.Api` - Admin API host app
- `src/Skoruba.IdentityServer4.STS.Identity` - STS/IdentityServer host app
- `src/Skoruba.IdentityServer4.Admin.UI` - reusable UI package for administration screens
- `src/Skoruba.IdentityServer4.Admin.BusinessLogic*` - business services
- `src/Skoruba.IdentityServer4.Admin.EntityFramework*` - DbContexts, migrations, provider-specific EF layers
- `src/Skoruba.IdentityServer4.Shared*` - shared configuration and helpers
- `tests/*` - integration and unit tests
- `templates/template-publish/content/src/*` - dotnet template source projects

## Website Structure

In Docker topology (see `docker-compose.yml`), nginx routes three public hostnames:

- `https://admin.sapico.local` -> Admin Web
- `https://admin-api.sapico.local` -> Admin API
- `https://sts.sapico.local` -> STS (IdentityServer + Identity UI)

In local development defaults (appsettings):

- Admin Web base URL: `https://localhost:44303`
- Admin API base URL: `https://localhost:44302`
- STS base URL: `https://localhost:44310`

## App Documentation

### 1) Admin Web App

Project:

- `src/Skoruba.IdentityServer4.Admin`

Startup pipeline highlights:

- `UseIdentityServer4AdminUI()`
- maps Admin UI endpoints and health checks

Role:

- primary administration UI for clients, resources, grants, users, and operational tasks
- OIDC client of STS

Operational notes:

- supports `/seed` and `/migrateonly` command-line startup modes in `Program.cs`

### 2) Admin API App

Project:

- `src/Skoruba.IdentityServer4.Admin.Api`

Startup pipeline highlights:

- Swagger/OpenAPI enabled
- authentication/authorization policies enforced
- controller endpoints + `/health`

Role:

- programmatic admin surface consumed by tools/UIs
- exposes API docs and OAuth2 Swagger flow

Typical paths:

- `/swagger`
- `/health`
- controller-based API routes

### 3) STS Identity App

Project:

- `src/Skoruba.IdentityServer4.STS.Identity`

Startup pipeline highlights:

- `UseIdentityServer()`
- ASP.NET Core Identity + IdentityServer4 host
- MVC routes and `/health`

Role:

- central authentication authority (token issuing, login/logout, identity flows)
- source of `.well-known` OIDC metadata and `/connect/*` endpoints

Typical paths:

- `/Account/*` (login, logout, etc.)
- `/.well-known/openid-configuration`
- `/connect/*`
- `/health`

## Startup Combination

When running the full stack in Visual Studio, use these startup projects together:

- `Skoruba.IdentityServer4.Admin`
- `Skoruba.IdentityServer4.Admin.Api`
- `Skoruba.IdentityServer4.STS.Identity`

## Data Layer at a Glance

Core DbContexts used across apps:

- `AdminIdentityDbContext`
- `AdminLogDbContext`
- `AdminAuditLogDbContext`
- `IdentityServerConfigurationDbContext`
- `IdentityServerPersistedGrantDbContext`
- `IdentityServerDataProtectionDbContext`

Provider switching is configuration-driven via `DatabaseProviderConfiguration:ProviderType`.

## Change Guidance for Agents

- Keep Admin Web, Admin API, and STS configuration in sync (URLs, client IDs, redirect URIs).
- If adjusting auth flows, evaluate impact on all three host apps.
- For template-impacting changes, update both `src/*` and `templates/template-publish/content/src/*` where relevant.
- Prefer documenting new architecture decisions in `docs/` and linking from `README.md`.

## Docker Naming Convention

Docker image names use **`saas-sapico-sts-`** as prefix with dash-separated suffixes:

| Service | Image | Container name |
|---|---|---|
| STS | `saas-sapico-sts-web` | `saas.sapico.sts.identityserver4` |
| Admin UI | `saas-sapico-sts-admin` | `saas.sapico.sts.identityserver4.admin` |
| Admin API | `saas-sapico-sts-admin.api` | `saas.sapico.sts.identityserver4.admin-api` |
| Database | `postgres:18-alpine` | `saas.sapico.sts.identityserver4.db` |

## CI/CD

- GitHub Actions workflow: `.github/workflows/deploy.yml`
- Triggers on `master` push
- Publishes via `dotnet publish /t:PublishContainer` to `registry.sapico.me`
- Three parallel deploy jobs: STS, Admin, Admin API

## Technology Stack

- **.NET 10** — do not downgrade target frameworks
- **IdentityServer4** with ASP.NET Core Identity
- **Entity Framework Core** — PostgreSQL default (SqlServer, MySql supported)
- **Serilog** for structured logging
- **Bootstrap 4** with Bootswatch theming
- **xUnit** with Fluent Assertions and Bogus for tests

## Coding Guidelines

- Health checks: every service exposes `/healthz`; Docker Compose uses `CMD-SHELL curl` health checks.
- Seed data: use `dotnet run /seed` or `SeedConfiguration` in `appsettings.json`.
- Migrations: use `build/add-migrations.ps1 -migration <Name> -migrationProviderName <Provider>`.
- Database provider switching: `DatabaseProviderConfiguration.ProviderType` in `appsettings.json`.
- Integration tests use in-memory databases with cookie auth stubs.