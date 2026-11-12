
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

        await context.Database.MigrateAsync();

        // Create roles if they do not exist
        string[] roleNames =
        {
            "SuperAdmin",
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

        // Get SuperAdmin configuration
        var email = configuration["Seed:SuperAdminEmail"]
            ?? throw new InvalidOperationException(
                "SuperAdmin email is missing.");

        var password = configuration["Seed:SuperAdminPassword"]
            ?? throw new InvalidOperationException(
                "SuperAdmin password is missing.");

        var superAdminRole = await context.Roles
            .FirstAsync(r => r.Name == "SuperAdmin");

        // Find existing SuperAdmin user
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                UserName = "system_admin",
                Email = email,
                RoleId = superAdminRole.Id,
                ApprovalStatus = "Approved"
            };

            user.PasswordHash =
                hasher.HashPassword(user, password);

            context.Users.Add(user);
        }

        else
        {
            // Update existing user's role and approval status
            user.RoleId = superAdminRole.Id;
            user.ApprovalStatus = "Approved";
        }

        await context.SaveChangesAsync();
    }
}
