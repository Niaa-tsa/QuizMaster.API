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

        string[] roleNames = { "Admin", "Lecturer", "Student" };
        foreach (var roleName in roleNames)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == roleName))
            {
                context.Roles.Add(new Role { Name = roleName });
            }
        }
        await context.SaveChangesAsync();

        var lecturerEmail = configuration["Seed:LecturerEmail"] ?? "lecturer@quizmaster.com";
        var lecturerPassword = configuration["Seed:LecturerPassword"]
                               ?? configuration["Seed:LecturerPassword."]
                               ?? "Lecturer123!";

        if (!await context.Users.AnyAsync(u => u.Email == lecturerEmail))
        {
            var lecturerRole = await context.Roles.FirstAsync(r => r.Name == "Lecturer");
            var lecturer = new User
            {
                UserName = "giorgi_lecturer",
                Email = lecturerEmail,
                RoleId = lecturerRole.Id
            };
            lecturer.PasswordHash = hasher.HashPassword(lecturer, lecturerPassword);
            context.Users.Add(lecturer);
        }

        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@quizmaster.com";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Admin123!";

        if (!await context.Users.AnyAsync(u => u.Email == adminEmail))
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var admin = new User
            {
                UserName = "system_admin",
                Email = adminEmail,
                RoleId = adminRole.Id
            };
            admin.PasswordHash = hasher.HashPassword(admin, adminPassword);
            context.Users.Add(admin);
        }

        await context.SaveChangesAsync();
    }
}