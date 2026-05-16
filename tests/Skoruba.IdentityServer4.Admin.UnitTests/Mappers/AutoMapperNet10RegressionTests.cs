using FluentAssertions;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using Skoruba.IdentityServer4.Admin.UnitTests.Helpers;
using Xunit;

namespace Skoruba.IdentityServer4.Admin.UnitTests.Mappers
{
    /// <summary>
    /// Regression test for AutoMapper/.NET 10 incompatibility.
    /// IdentityServer4.EntityFramework uses AutoMapper internally with static mapper configurations.
    /// On .NET 10, AutoMapper's BuildPublicNoArgExtensionMethods scans LINQ extension methods and
    /// calls MakeGenericMethod with types (e.g., System.Char) that violate new generic constraints
    /// on methods like MaxFloat&lt;T&gt; (requires INumber&lt;T&gt;), causing a TypeInitializationException.
    /// Fix: AutoMapperNet10Fix patches IS4 static mappers with safe configurations before use.
    /// </summary>
    public class AutoMapperNet10RegressionTests
    {
        public AutoMapperNet10RegressionTests()
        {
            AutoMapperNet10Fix.Apply();
        }

        [Fact]
        public void IdentityResourceMapper_InitializesWithoutTypeInitializationException()
        {
            var entity = new IdentityResource
            {
                Name = "openid",
                DisplayName = "OpenID",
                Enabled = true
            };

            var act = () => entity.ToModel();

            act.Should().NotThrow<System.TypeInitializationException>();
        }

        [Fact]
        public void ClientMapper_InitializesWithoutTypeInitializationException()
        {
            var entity = new Client
            {
                ClientId = "test-client",
                ClientName = "Test Client"
            };

            var act = () => entity.ToModel();

            act.Should().NotThrow<System.TypeInitializationException>();
        }

        [Fact]
        public void ApiResourceMapper_InitializesWithoutTypeInitializationException()
        {
            var entity = new ApiResource
            {
                Name = "api1",
                DisplayName = "Test API"
            };

            var act = () => entity.ToModel();

            act.Should().NotThrow<System.TypeInitializationException>();
        }
    }
}
