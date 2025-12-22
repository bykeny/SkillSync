using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SkillSync.Web;
using SkillSync.Web.Auth;
using SkillSync.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Get API base address from configuration
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:7234";

// Add HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

// Add MudBlazor
builder.Services.AddMudServices();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add Authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());

// Add Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAuthService>(provider => provider.GetRequiredService<AuthService>());
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IGitHubService, GitHubService>();

await builder.Build().RunAsync();