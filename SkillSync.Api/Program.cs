using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using SkillSync.Api;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.Services.Activities;
using SkillSync.Api.Services.Auth;
using SkillSync.Api.Services.Skills;
using SkillSync.Api.Services.Milestones;
using SkillSync.Api.Services.AI;
using SkillSync.Api.Services.GitHub;
using SkillSync.Api.Services.Analytics;
using SkillSync.Api.Validators;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/skillsync-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateSkillDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Add OpenAPI (needed for Scalar)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "https://localhost:7001" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Configure OpenAI Settings
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));

// Configure GitHub Settings
builder.Services.Configure<GitHubSettings>(builder.Configuration.GetSection("GitHub"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? throw new InvalidOperationException("JWT Secret not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// Register Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IMilestoneService, MilestoneService>();
builder.Services.AddScoped<IAIRecommendationService, AIRecommendationService>();
builder.Services.AddHttpClient<IGitHubService, GitHubService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Map OpenAPI JSON
    app.MapOpenApi();

    // Map Scalar UI (API docs)
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseCors("AllowBlazorApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Log.Information("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating the database");
    }
}

Log.Information("Starting SkillSync API");

app.Run();

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            // Add the security scheme at the document level
            var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = securitySchemes;

            // Apply it as a requirement for all operations
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security ??= [];
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            }
        }
    }
}