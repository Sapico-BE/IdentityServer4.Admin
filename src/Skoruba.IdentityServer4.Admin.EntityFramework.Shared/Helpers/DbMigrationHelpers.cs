using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Skoruba.AuditLogging.EntityFramework.DbContexts;
using Skoruba.AuditLogging.EntityFramework.Entities;
using Skoruba.IdentityServer4.Admin.EntityFramework.Configuration.Configuration;
using Skoruba.IdentityServer4.Admin.EntityFramework.Interfaces;
using IS4Entities = IdentityServer4.EntityFramework.Entities;

namespace Skoruba.IdentityServer4.Admin.EntityFramework.Shared.Helpers
{
	public static class DbMigrationHelpers
    {
        /// <summary>
        /// Generate migrations before running this method, you can use these steps bellow:
        /// https://github.com/skoruba/IdentityServer4.Admin#ef-core--data-access
        /// </summary>
        /// <param name="host"></param>
        /// <param name="applyDbMigrationWithDataSeedFromProgramArguments"></param>
        /// <param name="seedConfiguration"></param>
        /// <param name="databaseMigrationsConfiguration"></param>
        public static async Task<bool> ApplyDbMigrationsWithDataSeedAsync<TIdentityServerDbContext, TIdentityDbContext,
            TPersistedGrantDbContext, TLogDbContext, TAuditLogDbContext, TDataProtectionDbContext, TUser, TRole>(
            IHost host, bool applyDbMigrationWithDataSeedFromProgramArguments, SeedConfiguration seedConfiguration,
            DatabaseMigrationsConfiguration databaseMigrationsConfiguration)
            where TIdentityServerDbContext : DbContext, IAdminConfigurationDbContext
            where TIdentityDbContext : DbContext
            where TPersistedGrantDbContext : DbContext, IAdminPersistedGrantDbContext
            where TLogDbContext : DbContext, IAdminLogDbContext
            where TAuditLogDbContext : DbContext, IAuditLoggingDbContext<AuditLog>
            where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
            where TUser : IdentityUser, new()
            where TRole : IdentityRole, new()
        {
            bool migrationComplete = false;
            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                if ((databaseMigrationsConfiguration != null && databaseMigrationsConfiguration.ApplyDatabaseMigrations)
                    || (applyDbMigrationWithDataSeedFromProgramArguments))
                {
                    migrationComplete = await EnsureDatabasesMigratedAsync<TIdentityDbContext, TIdentityServerDbContext, TPersistedGrantDbContext, TLogDbContext, TAuditLogDbContext, TDataProtectionDbContext>(services);
                }

                if ((seedConfiguration != null && seedConfiguration.ApplySeed)
                    || (applyDbMigrationWithDataSeedFromProgramArguments))
                {
                    var seedComplete = await EnsureSeedDataAsync<TIdentityServerDbContext, TUser, TRole>(services);
                    return migrationComplete && seedComplete;
                }
                
            }
            return migrationComplete;
        }

        public static async Task<bool> EnsureDatabasesMigratedAsync<TIdentityDbContext, TConfigurationDbContext, TPersistedGrantDbContext, TLogDbContext, TAuditLogDbContext, TDataProtectionDbContext>(IServiceProvider services)
            where TIdentityDbContext : DbContext
            where TPersistedGrantDbContext : DbContext
            where TConfigurationDbContext : DbContext
            where TLogDbContext : DbContext
            where TAuditLogDbContext : DbContext
            where TDataProtectionDbContext : DbContext
        {
            int pendingMigrationCount = 0;
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<TPersistedGrantDbContext>())
                {
                    await context.Database.MigrateAsync();
                    pendingMigrationCount += (await context.Database.GetPendingMigrationsAsync()).Count();
                }

                using (var context = scope.ServiceProvider.GetRequiredService<TIdentityDbContext>())
                {
                    await context.Database.MigrateAsync();
                    pendingMigrationCount += (await context.Database.GetPendingMigrationsAsync()).Count();
                }

                using (var context = scope.ServiceProvider.GetRequiredService<TConfigurationDbContext>())
                {
                    await context.Database.MigrateAsync();
                    pendingMigrationCount += (await context.Database.GetPendingMigrationsAsync()).Count();
                }

                using (var context = scope.ServiceProvider.GetRequiredService<TLogDbContext>())
                {
                    await context.Database.MigrateAsync();
                    pendingMigrationCount += (await context.Database.GetPendingMigrationsAsync()).Count();
                }

                using (var context = scope.ServiceProvider.GetRequiredService<TAuditLogDbContext>())
                {
                    await context.Database.MigrateAsync();
                    pendingMigrationCount += (await context.Database.GetPendingMigrationsAsync()).Count();
                }

                using (var context = scope.ServiceProvider.GetRequiredService<TDataProtectionDbContext>())
                {
                    await context.Database.MigrateAsync();
                    pendingMigrationCount += (await context.Database.GetPendingMigrationsAsync()).Count();
                }
            }

            return pendingMigrationCount == 0;
        }

        public static async Task<bool> EnsureSeedDataAsync<TIdentityServerDbContext, TUser, TRole>(IServiceProvider serviceProvider)
        where TIdentityServerDbContext : DbContext, IAdminConfigurationDbContext
        where TUser : IdentityUser, new()
        where TRole : IdentityRole, new()
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TIdentityServerDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<TRole>>();
                var idsDataConfiguration = scope.ServiceProvider.GetRequiredService<IdentityServerData>();
                var idDataConfiguration = scope.ServiceProvider.GetRequiredService<IdentityData>();

                await EnsureSeedIdentityServerData(context, idsDataConfiguration);
                await EnsureSeedIdentityData(userManager, roleManager, idDataConfiguration);
                return true;
            }
        }

        /// <summary>
        /// Generate default admin user / role
        /// </summary>
        private static async Task EnsureSeedIdentityData<TUser, TRole>(UserManager<TUser> userManager,
            RoleManager<TRole> roleManager, IdentityData identityDataConfiguration)
            where TUser : IdentityUser, new()
            where TRole : IdentityRole, new()
        {
            // adding roles from seed
            foreach (var r in identityDataConfiguration.Roles)
            {
                if (!await roleManager.RoleExistsAsync(r.Name))
                {
                    var role = new TRole
                    {
                        Name = r.Name
                    };

                    var result = await roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        foreach (var claim in r.Claims)
                        {
                            await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(claim.Type, claim.Value));
                        }
                    }
                }
            }

            // adding users from seed
            foreach (var user in identityDataConfiguration.Users)
            {
                var identityUser = new TUser
                {
                    UserName = user.Username,
                    Email = user.Email,
                    EmailConfirmed = true
                };

                var userByUserName = await userManager.FindByNameAsync(user.Username);
                var userByEmail = await userManager.FindByEmailAsync(user.Email);

                // User is already exists in database
                if (userByUserName != default || userByEmail != default)
                {
                    continue;
                }

                // if there is no password we create user without password
                // user can reset password later, because accounts have EmailConfirmed set to true
                var result = !string.IsNullOrEmpty(user.Password)
                ? await userManager.CreateAsync(identityUser, user.Password)
                : await userManager.CreateAsync(identityUser);

                if (result.Succeeded)
                {
                    foreach (var claim in user.Claims)
                    {
                        await userManager.AddClaimAsync(identityUser, new System.Security.Claims.Claim(claim.Type, claim.Value));
                    }

                    foreach (var role in user.Roles)
                    {
                        await userManager.AddToRoleAsync(identityUser, role);
                    }
                }
            }
        }

        /// <summary>
        /// Generate default clients, identity and api resources
        /// </summary>
        private static async Task EnsureSeedIdentityServerData<TIdentityServerDbContext>(TIdentityServerDbContext context, IdentityServerData identityServerDataConfiguration)
            where TIdentityServerDbContext : DbContext, IAdminConfigurationDbContext
        {
            foreach (var resource in identityServerDataConfiguration.IdentityResources)
            {
                var exits = await context.IdentityResources.AnyAsync(a => a.Name == resource.Name);

                if (exits)
                {
                    continue;
                }

                await context.IdentityResources.AddAsync(SeedMapper.ToEntity(resource));
            }

            foreach (var apiScope in identityServerDataConfiguration.ApiScopes)
            {
                var exits = await context.ApiScopes.AnyAsync(a => a.Name == apiScope.Name);

                if (exits)
                {
                    continue;
                }

                await context.ApiScopes.AddAsync(SeedMapper.ToEntity(apiScope));
            }

            foreach (var resource in identityServerDataConfiguration.ApiResources)
            {
                var exits = await context.ApiResources.AnyAsync(a => a.Name == resource.Name);

                if (exits)
                {
                    continue;
                }

                foreach (var s in resource.ApiSecrets)
                {
                    s.Value = s.Value.ToSha256();
                }

                await context.ApiResources.AddAsync(SeedMapper.ToEntity(resource));
            }

            foreach (var client in identityServerDataConfiguration.Clients)
            {
                var exits = await context.Clients.AnyAsync(a => a.ClientId == client.ClientId);

                if (exits)
                {
                    continue;
                }

                foreach (var secret in client.ClientSecrets)
                {
                    secret.Value = secret.Value.ToSha256();
                }

                client.Claims = client.ClientClaims
                    .Select(c => new ClientClaim(c.Type, c.Value))
                    .ToList();

                await context.Clients.AddAsync(SeedMapper.ToEntity(client));
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Manual mappers replacing IdentityServer4.EntityFramework.Mappers which use AutoMapper
        /// internally and crash on .NET 10 due to MakeGenericMethod restrictions.
        /// </summary>
        private static class SeedMapper
        {
            public static IS4Entities.IdentityResource ToEntity(IdentityResource model) =>
                new IS4Entities.IdentityResource
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    Required = model.Required,
                    Emphasize = model.Emphasize,
                    ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                    Enabled = model.Enabled,
                    UserClaims = model.UserClaims.Select(c => new IS4Entities.IdentityResourceClaim { Type = c }).ToList(),
                    Properties = model.Properties.Select(p => new IS4Entities.IdentityResourceProperty { Key = p.Key, Value = p.Value }).ToList(),
                };

            public static IS4Entities.ApiScope ToEntity(ApiScope model) =>
                new IS4Entities.ApiScope
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    Required = model.Required,
                    Emphasize = model.Emphasize,
                    ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                    Enabled = model.Enabled,
                    UserClaims = model.UserClaims.Select(c => new IS4Entities.ApiScopeClaim { Type = c }).ToList(),
                    Properties = model.Properties.Select(p => new IS4Entities.ApiScopeProperty { Key = p.Key, Value = p.Value }).ToList(),
                };

            public static IS4Entities.ApiResource ToEntity(ApiResource model) =>
                new IS4Entities.ApiResource
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    Enabled = model.Enabled,
                    AllowedAccessTokenSigningAlgorithms = model.AllowedAccessTokenSigningAlgorithms.Any()
                        ? string.Join(",", model.AllowedAccessTokenSigningAlgorithms) : null,
                    ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                    UserClaims = model.UserClaims.Select(c => new IS4Entities.ApiResourceClaim { Type = c }).ToList(),
                    Scopes = model.Scopes.Select(s => new IS4Entities.ApiResourceScope { Scope = s }).ToList(),
                    Secrets = model.ApiSecrets.Select(s => new IS4Entities.ApiResourceSecret { Type = s.Type, Value = s.Value, Description = s.Description, Expiration = s.Expiration }).ToList(),
                    Properties = model.Properties.Select(p => new IS4Entities.ApiResourceProperty { Key = p.Key, Value = p.Value }).ToList(),
                };

            public static IS4Entities.Client ToEntity(Client model) =>
                new IS4Entities.Client
                {
                    ClientId = model.ClientId,
                    ClientName = model.ClientName,
                    Description = model.Description,
                    ClientUri = model.ClientUri,
                    LogoUri = model.LogoUri,
                    Enabled = model.Enabled,
                    ProtocolType = model.ProtocolType,
                    RequireClientSecret = model.RequireClientSecret,
                    RequireConsent = model.RequireConsent,
                    AllowRememberConsent = model.AllowRememberConsent,
                    AlwaysIncludeUserClaimsInIdToken = model.AlwaysIncludeUserClaimsInIdToken,
                    RequirePkce = model.RequirePkce,
                    AllowPlainTextPkce = model.AllowPlainTextPkce,
                    RequireRequestObject = model.RequireRequestObject,
                    AllowAccessTokensViaBrowser = model.AllowAccessTokensViaBrowser,
                    FrontChannelLogoutUri = model.FrontChannelLogoutUri,
                    FrontChannelLogoutSessionRequired = model.FrontChannelLogoutSessionRequired,
                    BackChannelLogoutUri = model.BackChannelLogoutUri,
                    BackChannelLogoutSessionRequired = model.BackChannelLogoutSessionRequired,
                    AllowOfflineAccess = model.AllowOfflineAccess,
                    IdentityTokenLifetime = model.IdentityTokenLifetime,
                    AllowedIdentityTokenSigningAlgorithms = model.AllowedIdentityTokenSigningAlgorithms.Any()
                        ? string.Join(",", model.AllowedIdentityTokenSigningAlgorithms) : null,
                    AccessTokenLifetime = model.AccessTokenLifetime,
                    AuthorizationCodeLifetime = model.AuthorizationCodeLifetime,
                    ConsentLifetime = model.ConsentLifetime,
                    AbsoluteRefreshTokenLifetime = model.AbsoluteRefreshTokenLifetime,
                    SlidingRefreshTokenLifetime = model.SlidingRefreshTokenLifetime,
                    RefreshTokenUsage = (int)model.RefreshTokenUsage,
                    UpdateAccessTokenClaimsOnRefresh = model.UpdateAccessTokenClaimsOnRefresh,
                    RefreshTokenExpiration = (int)model.RefreshTokenExpiration,
                    AccessTokenType = (int)model.AccessTokenType,
                    EnableLocalLogin = model.EnableLocalLogin,
                    IncludeJwtId = model.IncludeJwtId,
                    AlwaysSendClientClaims = model.AlwaysSendClientClaims,
                    ClientClaimsPrefix = model.ClientClaimsPrefix,
                    PairWiseSubjectSalt = model.PairWiseSubjectSalt,
                    UserSsoLifetime = model.UserSsoLifetime,
                    UserCodeType = model.UserCodeType,
                    DeviceCodeLifetime = model.DeviceCodeLifetime,
                    NonEditable = false,
                    AllowedGrantTypes = model.AllowedGrantTypes.Select(g => new IS4Entities.ClientGrantType { GrantType = g }).ToList(),
                    RedirectUris = model.RedirectUris.Select(u => new IS4Entities.ClientRedirectUri { RedirectUri = u }).ToList(),
                    PostLogoutRedirectUris = model.PostLogoutRedirectUris.Select(u => new IS4Entities.ClientPostLogoutRedirectUri { PostLogoutRedirectUri = u }).ToList(),
                    AllowedScopes = model.AllowedScopes.Select(s => new IS4Entities.ClientScope { Scope = s }).ToList(),
                    ClientSecrets = model.ClientSecrets.Select(s => new IS4Entities.ClientSecret { Type = s.Type, Value = s.Value, Description = s.Description, Expiration = s.Expiration }).ToList(),
                    Claims = model.Claims.Select(c => new IS4Entities.ClientClaim { Type = c.Type, Value = c.Value }).ToList(),
                    IdentityProviderRestrictions = model.IdentityProviderRestrictions.Select(r => new IS4Entities.ClientIdPRestriction { Provider = r }).ToList(),
                    AllowedCorsOrigins = model.AllowedCorsOrigins.Select(o => new IS4Entities.ClientCorsOrigin { Origin = o }).ToList(),
                    Properties = model.Properties.Select(p => new IS4Entities.ClientProperty { Key = p.Key, Value = p.Value }).ToList(),
                };
        }
    }
}
