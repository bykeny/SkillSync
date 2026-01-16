using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.DTOs.Skills;
using SkillSync.Api.Services.Skills;

namespace SkillSync.Tests.Unit.Services;

public class SkillServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SkillService _skillService;
    private readonly string _testUserId = "test-user-123";

    public SkillServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        
        var mockLogger = new Mock<ILogger<SkillService>>();
        _skillService = new SkillService(_context, mockLogger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var category = new SkillCategory
        {
            Id = 1,
            Name = "Programming Languages",
            ColorHex = "#3B82F6"
        };

        var skill = new Skill
        {
            Id = 1,
            Name = "C# Programming",
            Description = "Learning C# and .NET",
            ProficiencyLevel = 3,
            TargetLevel = 5,
            CategoryId = 1,
            UserId = _testUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.SkillCategories.Add(category);
        _context.Skills.Add(skill);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllSkillsAsync_ShouldReturnUserSkills()
    {
        // Act
        var result = await _skillService.GetAllSkillsAsync(_testUserId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("C# Programming");
    }

    [Fact]
    public async Task GetSkillByIdAsync_WithValidId_ShouldReturnSkill()
    {
        // Act
        var result = await _skillService.GetSkillByIdAsync(1, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("C# Programming");
        result.ProficiencyLevel.Should().Be(3);
    }

    [Fact]
    public async Task GetSkillByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _skillService.GetSkillByIdAsync(999, _testUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateSkillAsync_WithValidData_ShouldCreateSkill()
    {
        // Arrange
        var dto = new CreateSkillDto
        {
            Name = "Python Programming",
            Description = "Learning Python",
            ProficiencyLevel = 2,
            TargetLevel = 4,
            CategoryId = 1
        };

        // Act
        var result = await _skillService.CreateSkillAsync(dto, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Python Programming");
        result.ProficiencyLevel.Should().Be(2);

        var skillInDb = await _context.Skills.FindAsync(result.Id);
        skillInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateSkillAsync_ShouldCreateProgressHistory()
    {
        // Arrange
        var dto = new UpdateSkillDto
        {
            Name = "C# Programming",
            Description = "Advanced C#",
            ProficiencyLevel = 4, // Changed from 3 to 4
            TargetLevel = 5,
            CategoryId = 1,
            IsActive = true
        };

        // Act
        var result = await _skillService.UpdateSkillAsync(1, dto, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result!.ProficiencyLevel.Should().Be(4);

        var progressHistory = await _context.ProgressHistories
            .Where(p => p.SkillId == 1)
            .ToListAsync();

        progressHistory.Should().HaveCount(1);
        progressHistory.First().PreviousLevel.Should().Be(3);
        progressHistory.First().NewLevel.Should().Be(4);
    }

    [Fact]
    public async Task DeleteSkillAsync_WithActivities_ShouldDeleteBoth()
    {
        // Arrange
        var activity = new Activity
        {
            Title = "Test Activity",
            SkillId = 1,
            UserId = _testUserId,
            Type = ActivityType.Course,
            Status = ActivityStatus.NotStarted,
            DurationMinutes = 60,
            CreatedAt = DateTime.UtcNow
        };
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _skillService.DeleteSkillAsync(1, _testUserId);

        // Assert
        result.Should().BeTrue();
        
        var skillInDb = await _context.Skills.FindAsync(1);
        skillInDb.Should().BeNull();

        var activitiesInDb = await _context.Activities.Where(a => a.SkillId == 1).ToListAsync();
        activitiesInDb.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnAllCategories()
    {
        // Act
        var result = await _skillService.GetCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Programming Languages");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}