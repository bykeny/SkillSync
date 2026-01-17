using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SkillSync.Api.DTOs.Auth;
using SkillSync.Api.DTOs.Skills;

namespace SkillSync.Tests.Integration.Controllers;

public class SkillsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private string? _accessToken;

    public SkillsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var registerDto = new RegisterDto
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseWrapper>();
        _accessToken = authResponse?.Token?.AccessToken;

        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    [Fact]
    public async Task GetSkills_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/skills");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSkills_WithAuth_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/skills");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateSkill_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await AuthenticateAsync();
        
        var createDto = new CreateSkillDto
        {
            Name = "Integration Test Skill",
            Description = "Testing skill creation",
            ProficiencyLevel = 2,
            TargetLevel = 4,
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/skills", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var skill = await response.Content.ReadFromJsonAsync<SkillDto>();
        skill.Should().NotBeNull();
        skill!.Name.Should().Be("Integration Test Skill");
    }

    [Fact]
    public async Task GetSkillById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsync();
        
        var createDto = new CreateSkillDto
        {
            Name = "Test Skill for Get",
            ProficiencyLevel = 3,
            TargetLevel = 5
        };

        var createResponse = await _client.PostAsJsonAsync("/api/skills", createDto);
        var createdSkill = await createResponse.Content.ReadFromJsonAsync<SkillDto>();

        // Act
        var response = await _client.GetAsync($"/api/skills/{createdSkill!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var skill = await response.Content.ReadFromJsonAsync<SkillDto>();
        skill.Should().NotBeNull();
        skill!.Name.Should().Be("Test Skill for Get");
    }

    [Fact]
    public async Task UpdateSkill_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsync();
        
        var createDto = new CreateSkillDto
        {
            Name = "Skill to Update",
            ProficiencyLevel = 2,
            TargetLevel = 5
        };

        var createResponse = await _client.PostAsJsonAsync("/api/skills", createDto);
        var createdSkill = await createResponse.Content.ReadFromJsonAsync<SkillDto>();

        var updateDto = new UpdateSkillDto
        {
            Name = "Updated Skill Name",
            ProficiencyLevel = 3,
            TargetLevel = 5,
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/skills/{createdSkill!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedSkill = await response.Content.ReadFromJsonAsync<SkillDto>();
        updatedSkill.Should().NotBeNull();
        updatedSkill!.Name.Should().Be("Updated Skill Name");
        updatedSkill.ProficiencyLevel.Should().Be(3);
    }

    [Fact]
    public async Task DeleteSkill_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAsync();
        
        var createDto = new CreateSkillDto
        {
            Name = "Skill to Delete",
            ProficiencyLevel = 2,
            TargetLevel = 5
        };

        var createResponse = await _client.PostAsJsonAsync("/api/skills", createDto);
        var createdSkill = await createResponse.Content.ReadFromJsonAsync<SkillDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/skills/{createdSkill!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/skills/{createdSkill.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private class AuthResponseWrapper
    {
        public string Message { get; set; } = string.Empty;
        public TokenResponse? Token { get; set; }
    }

    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}