using Vendo.IdentityManagement.Application.Common.Interfaces;
using Vendo.IdentityManagement.Domain.Entities;
using Vendo.IdentityManagement.Domain.Repositories;
using Vendo.IdentityManagement.Domain.ValueObjects;

namespace Vendo.IdentityManagement.Infrastructure.Persistence;

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

        // Create merchant user
        var merchantEmail = Email.Create("merchant@vendo.com");
        var merchantPassword = passwordHasher.HashPassword("Merchant@123");
        var merchantUser = User.Create(
            "merchant",
            merchantEmail,
            merchantPassword,
            "Test",
            "Merchant",
            new List<string> { "Merchant", "User" });

        await userRepository.AddAsync(merchantUser);

        // Create customer user
        var customerEmail = Email.Create("customer@vendo.com");
        var customerPassword = passwordHasher.HashPassword("Customer@123");
        var customerUser = User.Create(
            "customer",
            customerEmail,
            customerPassword,
            "Test",
            "Customer",
            new List<string> { "Customer", "User" });

        await userRepository.AddAsync(customerUser);

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
