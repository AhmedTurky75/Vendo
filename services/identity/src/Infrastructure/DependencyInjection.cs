using Microsoft.Extensions.DependencyInjection;
using Vendo.Identity.Application.Common.Interfaces;
using Vendo.Identity.Domain.Repositories;
using Vendo.Identity.Infrastructure.Identity;
using Vendo.Identity.Infrastructure.Identity.Configuration;
using Vendo.Identity.Infrastructure.Identity.ProfileService;
using Vendo.Identity.Infrastructure.Persistence.Repositories;
using Vendo.Identity.Infrastructure.Services;
using Duende.IdentityServer.Validation;
using Duende.IdentityServer.Services;

namespace Vendo.Identity.Infrastructure;

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
        .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
        .AddDeveloperSigningCredential(); // For development only - use proper certificate in production

        // Register custom services for IdentityServer
        services.AddTransient<IProfileService, CustomProfileService>();
        services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();

        return services;
    }
}
