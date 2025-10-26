using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vendo.IdentityManagement.Domain.Entities;
using Vendo.IdentityManagement.Infrastructure.Persistence.SeedDtos;

namespace Vendo.IdentityManagement.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data into the Identity database from JSON files.
/// </summary>
public class DataSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DataSeeder> _logger;
    private readonly string _dataPath;

    public DataSeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<DataSeeder> logger,
        IConfiguration configuration)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;

        // Get the data directory path from configuration
        var configuredPath = configuration["SeedDataSettings:DataPath"] ?? "Infrastructure/Persistence/SeedData";
        var baseDirectory = AppContext.BaseDirectory;

        // Navigate up from bin/Debug/net9.0 to the src folder, then to the configured path
        var srcRoot = Path.Combine(baseDirectory, "..", "..", "..", "..");
        _dataPath = Path.GetFullPath(Path.Combine(srcRoot, configuredPath));
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding for Identity service...");

            await SeedRolesAsync();
            await SeedUsersAsync();

            _logger.LogInformation("Data seeding completed successfully for Identity service.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the Identity database.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        // Check if roles already exist
        var existingRoles = _roleManager.Roles.ToList();
        if (existingRoles.Any())
        {
            _logger.LogInformation("Roles already exist. Skipping role seeding.");
            return;
        }

        var rolesFilePath = Path.Combine(_dataPath, "roles.json");

        if (!File.Exists(rolesFilePath))
        {
            _logger.LogWarning($"Roles seed file not found at {rolesFilePath}");
            return;
        }

        _logger.LogInformation($"Reading roles from {rolesFilePath}");
        var rolesJson = await File.ReadAllTextAsync(rolesFilePath);
        var roleDtos = JsonSerializer.Deserialize<List<RoleSeedDto>>(rolesJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });

        if (roleDtos == null || !roleDtos.Any())
        {
            _logger.LogWarning("No roles found in seed file.");
            return;
        }

        foreach (var dto in roleDtos)
        {
            var role = new ApplicationRole
            {
                Id = dto.Id,
                Name = dto.Name,
                NormalizedName = dto.NormalizedName ?? dto.Name.ToUpperInvariant(),
                Description = dto.Description,
                Permissions = dto.Permissions,
                IsSystemRole = dto.IsSystemRole,
                CreatedAt = dto.CreatedAt
            };

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Created role: {role.Name}");
            }
            else
            {
                _logger.LogError($"Failed to create role: {role.Name}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        _logger.LogInformation($"Seeded {roleDtos.Count} roles successfully.");
    }

    private async Task SeedUsersAsync()
    {
        // Check if users already exist
        var existingUsers = _userManager.Users.ToList();
        if (existingUsers.Any())
        {
            _logger.LogInformation("Users already exist. Skipping user seeding.");
            return;
        }

        var usersFilePath = Path.Combine(_dataPath, "users.json");

        if (!File.Exists(usersFilePath))
        {
            _logger.LogWarning($"Users seed file not found at {usersFilePath}");
            return;
        }

        _logger.LogInformation($"Reading users from {usersFilePath}");
        var usersJson = await File.ReadAllTextAsync(usersFilePath);
        var userDtos = JsonSerializer.Deserialize<List<UserSeedDto>>(usersJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });

        if (userDtos == null || !userDtos.Any())
        {
            _logger.LogWarning("No users found in seed file.");
            return;
        }

        // Default password for all seed users
        const string defaultPassword = "DefaultPassword@123";

        foreach (var dto in userDtos)
        {
            var user = new ApplicationUser
            {
                Id = dto.Id,
                UserName = dto.Email.Split('@')[0], // Use email prefix as username
                Email = dto.Email,
                EmailConfirmed = dto.EmailConfirmed,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                PhoneNumberConfirmed = dto.PhoneNumberConfirmed,
                TwoFactorEnabled = dto.TwoFactorEnabled,
                IsActive = dto.IsActive,
                ProfilePictureUrl = dto.ProfilePictureUrl,
                DateOfBirth = dto.DateOfBirth,
                StoreId = dto.StoreId,
                Address = dto.Address != null ? JsonSerializer.Serialize(dto.Address) : null,
                Preferences = dto.Preferences != null ? JsonSerializer.Serialize(dto.Preferences) : null,
                LastLoginAt = dto.LastLoginAt,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };

            var result = await _userManager.CreateAsync(user, defaultPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Created user: {user.Email}");

                // Assign roles to user
                if (dto.Roles != null && dto.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, dto.Roles);
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation($"Assigned roles {string.Join(", ", dto.Roles)} to user {user.Email}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to assign roles to user {user.Email}. Errors: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }
            else
            {
                _logger.LogError($"Failed to create user: {user.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        _logger.LogInformation($"Seeded {userDtos.Count} users successfully.");
        _logger.LogInformation($"Default password for all seeded users: {defaultPassword}");
    }
}
