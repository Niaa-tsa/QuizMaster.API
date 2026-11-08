
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuizMaster.Domain.Entities;
using QuizMaster.Infrastructure.Data;

namespace QuizMaster.API.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<QuizMasterDbContext>();

        var hasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<User>>();

        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

        // Database migration
        await context.Database.MigrateAsync();

        // Create roles if they do not exist
        string[] roleNames =
        {
            "Admin",
            "Lecturer",
            "Student"
        };

        foreach (var roleName in roleNames)
        {
            var roleExists = await context.Roles
                .AnyAsync(r => r.Name == roleName);

            if (!roleExists)
            {
                context.Roles.Add(new Role
                {
                    Name = roleName
                });
            }
        }

        await context.SaveChangesAsync();

        // Seed Admin
        await SeedUserAsync(
            context,
            hasher,
            "Admin",
            "admin@quizmaster.com",
            "Admin123!",
            "system_admin");

        // Seed Lecturer
        await SeedUserAsync(
            context,
            hasher,
            "Lecturer",
            "lecturer@quizmaster.com",
            "Lecturer123!",
            "giorgi_lecturer");
    }

    private static async Task SeedUserAsync(
     QuizMasterDbContext context,
     IPasswordHasher<User> hasher,
     string roleName,
     string email,
     string password,
     string userName)
    {
        var userExists = await context.Users
            .AnyAsync(u => u.Email == email);

        if (userExists)
            return;

        var role = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName);

        if (role is null)
            throw new InvalidOperationException(
                $"{roleName} role was not found.");

        var user = new User
        {
            UserName = userName,
            Email = email,
            RoleId = role.Id
        };

        user.PasswordHash =
            hasher.HashPassword(user, password);

        context.Users.Add(user);
    }
}
