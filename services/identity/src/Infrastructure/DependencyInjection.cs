using Microsoft.Extensions.DependencyInjection;
using Vendo.IdentityManagement.Application.Common.Interfaces;
using Vendo.IdentityManagement.Domain.Repositories;
using Vendo.IdentityManagement.Infrastructure.Identity.Configuration;
using Vendo.IdentityManagement.Infrastructure.Identity.ProfileService;
using Vendo.IdentityManagement.Infrastructure.Persistence.Repositories;
using Vendo.IdentityManagement.Infrastructure.Services;
using Duende.IdentityServer.Services;

namespace Vendo.IdentityManagement.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure layer services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        // Register services
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Configure IdentityServer
        services.AddIdentityServer(options =>
        {
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;
            options.EmitStaticAudienceClaim = true;
        })
        .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
        .AddInMemoryApiResources(IdentityServerConfig.ApiResources)
        .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
        .AddInMemoryClients(IdentityServerConfig.Clients)
        .AddProfileService<CustomProfileService>()
        .AddDeveloperSigningCredential(); // For development only - use proper certificate in production

        // Register custom services for IdentityServer
        services.AddTransient<IProfileService, CustomProfileService>();

        return services;
    }
}
