using Microsoft.EntityFrameworkCore;
using QuizMaster.Domain.Entities;

namespace QuizMaster.Infrastructure.Data;

public class QuizMasterDbContext : DbContext
{
    public QuizMasterDbContext(
        DbContextOptions<QuizMasterDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();
    public DbSet<QuizResult> QuizResults => Set<QuizResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<Quiz>()
            .Property(q => q.Title)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<Quiz>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Quizzes_TimeLimit",
                "[TimeLimitMinutes] > 0"));

        modelBuilder.Entity<Question>()
            .Property(q => q.Points)
            .HasPrecision(8, 2);

        modelBuilder.Entity<QuizResult>()
            .Property(r => r.Score)
            .HasPrecision(8, 2);

        modelBuilder.Entity<QuizResult>()
            .Property(r => r.MaxScore)
            .HasPrecision(8, 2);

        modelBuilder.Entity<QuizResult>()
            .HasIndex(r => r.AttemptId)
            .IsUnique();

        modelBuilder.Entity<QuizResult>()
            .HasOne(r => r.QuizAttempt)
            .WithOne(a => a.QuizResult)
            .HasForeignKey<QuizResult>(r => r.AttemptId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Role>()
            .HasData(
                new Role { Id = 1, Name = "Lecturer" },
                new Role { Id = 2, Name = "Student" }
            );

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.Questions)
            .WithOne(q => q.Quiz)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Question>()
            .HasMany(q => q.AnswerOptions)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizAttempt>()
            .HasOne(a => a.User)
            .WithMany(u => u.QuizAttempts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizAttempt>()
            .HasOne(a => a.Quiz)
            .WithMany(q => q.QuizAttempts)
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentAnswer>()
            .HasOne(a => a.QuizAttempt)
            .WithMany(a => a.StudentAnswers)
            .HasForeignKey(a => a.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentAnswer>()
            .HasOne(a => a.AnswerOption)
            .WithMany()
            .HasForeignKey(a => a.AnswerOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}