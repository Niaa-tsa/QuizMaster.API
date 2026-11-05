using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        var email = configuration["Seed:Lecturer@gmail.com"];
        var password = configuration["Seed:Pass123."];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
            return;

        var exists = await context.Users.AnyAsync(u =>
            u.Email == email || u.UserName == "lecturer");

        if (exists)
            return;

        var lecturerRole = await context.Roles
            .FirstAsync(r => r.Name == "Lecturer");

        var lecturer = new User
        {
            UserName = "lecturer",
            Email = email,
            RoleId = lecturerRole.Id
        };

        lecturer.PasswordHash = hasher.HashPassword(lecturer, password);

        context.Users.Add(lecturer);
        await context.SaveChangesAsync();
    }
}