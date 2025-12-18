using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillSync.Api.Data.Entities;

namespace SkillSync.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<SkillCategory> SkillCategories { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Milestone> Milestones { get; set; }
    public DbSet<ProgressHistory> ProgressHistories { get; set; }
    public DbSet<GitHubProfile> GitHubProfiles { get; set; }
    public DbSet<GitHubActivity> GitHubActivities { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AIRecommendation> AIRecommendations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser relationships
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasMany(u => u.Skills)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Activities)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.GitHubProfile)
                .WithOne(gp => gp.User)
                .HasForeignKey<GitHubProfile>(gp => gp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Skill relationships
        builder.Entity<Skill>(entity =>
        {
            entity.HasOne(s => s.Category)
                .WithMany(c => c.Skills)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(s => s.Activities)
                .WithOne(a => a.Skill)
                .HasForeignKey(a => a.SkillId)
                .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

            entity.HasMany(s => s.Milestones)
                .WithOne(m => m.Skill)
                .HasForeignKey(m => m.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.ProgressHistories)
                .WithOne(ph => ph.Skill)
                .HasForeignKey(ph => ph.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Description).HasMaxLength(500);
        });

        // SkillCategory configuration
        builder.Entity<SkillCategory>(entity =>
        {
            entity.Property(sc => sc.Name).IsRequired().HasMaxLength(100);
            entity.Property(sc => sc.ColorHex).HasMaxLength(7);
        });

        // Activity configuration
        builder.Entity<Activity>(entity =>
        {
            entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Description).HasMaxLength(1000);
            entity.Property(a => a.ResourceUrl).HasMaxLength(500);
        });

        // Milestone configuration
        builder.Entity<Milestone>(entity =>
        {
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).HasMaxLength(1000);
        });

        // GitHubProfile relationships
        builder.Entity<GitHubProfile>(entity =>
        {
            entity.HasMany(gp => gp.Activities)
                .WithOne(ga => ga.GitHubProfile)
                .HasForeignKey(ga => ga.GitHubProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(gp => gp.GitHubUsername).IsRequired().HasMaxLength(100);
        });

        // GitHubActivity configuration
        builder.Entity<GitHubActivity>(entity =>
        {
            entity.Property(ga => ga.RepositoryName).IsRequired().HasMaxLength(200);
            entity.Property(ga => ga.PrimaryLanguage).HasMaxLength(50);
        });

        // Notification configuration
        builder.Entity<Notification>(entity =>
        {
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            entity.Property(n => n.ActionUrl).HasMaxLength(500);
        });

        // AIRecommendation configuration
        builder.Entity<AIRecommendation>(entity =>
        {
            entity.Property(r => r.Title).IsRequired().HasMaxLength(200);
            entity.Property(r => r.Content).IsRequired();
        });

        // RefreshToken configuration
        builder.Entity<RefreshToken>(entity =>
        {
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
        });

        // Seed data
        SeedData(builder);
    }

    private void SeedData(ModelBuilder builder)
    {
        builder.Entity<SkillCategory>().HasData(
            new SkillCategory { Id = 1, Name = "Programming Languages", Description = "Languages like C#, Python, JavaScript", IconName = "code", ColorHex = "#3B82F6" },
            new SkillCategory { Id = 2, Name = "Frameworks & Libraries", Description = "Frameworks like .NET, React, Angular", IconName = "layers", ColorHex = "#10B981" },
            new SkillCategory { Id = 3, Name = "Database & Storage", Description = "SQL, NoSQL, Cloud Storage", IconName = "database", ColorHex = "#F59E0B" },
            new SkillCategory { Id = 4, Name = "DevOps & Cloud", Description = "CI/CD, Docker, Azure, AWS", IconName = "cloud", ColorHex = "#8B5CF6" },
            new SkillCategory { Id = 5, Name = "Soft Skills", Description = "Communication, Leadership, Problem Solving", IconName = "users", ColorHex = "#EC4899" },
            new SkillCategory { Id = 6, Name = "Tools & Practices", Description = "Git, Agile, Testing", IconName = "settings", ColorHex = "#6366F1" }
        );
    }
}