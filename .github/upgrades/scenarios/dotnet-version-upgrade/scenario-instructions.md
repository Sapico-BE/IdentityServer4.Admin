# Scenario Instructions

## Parameters
- **Target Framework**: net10.0 (.NET 10.0 LTS)
- **Solution**: Skoruba.IdentityServer4.Admin.sln
- **Source Branch**: fix/docker-image-naming
- **Working Branch**: upgrade-to-NET10

## Preferences

### Flow Mode
**Automatic** — Run end-to-end, only pause when blocked.

### Technical Preferences
- **IdentityServer**: Keep IdentityServer4 — do NOT migrate to Duende IdentityServer (commercial)
- **AutoMapper**: Do NOT update if license changed to commercial — find OSS alternative or keep current version
- **General rule**: Only use non-commercial / open-source licensed packages

## Strategy
**Selected**: All-At-Once
**Rationale**: 21 projects, all on net10.0, zero breaking changes, package bumps only.

### Execution Constraints
- Single atomic upgrade — all projects updated together
- Validate full solution build after upgrade
- Commit strategy: Single Commit at End

## Key Decisions Log
- 2025-07-14: User confirmed .NET 10.0 target, no commercial package upgrades
- 2025-07-14: AutoMapper kept at 12.0.1 (commercial license at v13+)
- 2025-07-14: FluentAssertions kept at 6.x (commercial license at v7+)
- 2025-07-14: User wants docker-compose build verification