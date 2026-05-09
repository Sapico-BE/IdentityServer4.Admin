# AGENTS.md

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

- `https://admin.skoruba.local` -> Admin Web
- `https://admin-api.skoruba.local` -> Admin API
- `https://sts.skoruba.local` -> STS (IdentityServer + Identity UI)

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