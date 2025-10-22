using Vendo.Identity.Application.Common.Interfaces;
using Vendo.Identity.Domain.Entities;
using Vendo.Identity.Domain.Repositories;
using Vendo.Identity.Domain.ValueObjects;

namespace Vendo.Identity.Infrastructure.Persistence;

/// <summary>
/// Seeds initial test users into the repository
/// </summary>
public static class SeedData
{
    public static async Task SeedUsersAsync(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        // Check if users already exist
        var existingUsers = await userRepository.GetAllAsync();
        if (existingUsers.Any())
        {
            return; // Already seeded
        }

        // Create admin user
        var adminEmail = Email.Create("admin@vendo.com");
        var adminPassword = passwordHasher.HashPassword("Admin@123");
        var adminUser = User.Create(
            "admin",
            adminEmail,
            adminPassword,
            "System",
            "Administrator",
            new List<string> { "Admin", "User" });

        await userRepository.AddAsync(adminUser);

        // Create regular user
        var userEmail = Email.Create("user@vendo.com");
        var userPassword = passwordHasher.HashPassword("User@123");
        var regularUser = User.Create(
            "testuser",
            userEmail,
            userPassword,
            "Test",
            "User",
            new List<string> { "User" });

        await userRepository.AddAsync(regularUser);

        // Create another test user
        var user2Email = Email.Create("john.doe@vendo.com");
        var user2Password = passwordHasher.HashPassword("JohnDoe@123");
        var johnDoe = User.Create(
            "johndoe",
            user2Email,
            user2Password,
            "John",
            "Doe",
            new List<string> { "User" });

        await userRepository.AddAsync(johnDoe);
    }
}
