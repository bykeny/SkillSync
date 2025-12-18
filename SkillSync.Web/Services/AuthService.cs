using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using SkillSync.Web.Auth;
using SkillSync.Web.Models;

namespace SkillSync.Web.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string Message)> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<UserProfile?> GetProfileAsync();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = (CustomAuthStateProvider)authStateProvider;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (authResponse?.Token != null)
                {
                    await _localStorage.SetItemAsync("accessToken", authResponse.Token.AccessToken);
                    await _localStorage.SetItemAsync("refreshToken", authResponse.Token.RefreshToken);

                    _authStateProvider.NotifyUserAuthentication(authResponse.Token.AccessToken);
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token.AccessToken);
                }

                return (true, authResponse?.Message ?? "Registration successful");
            }

            var errorResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return (false, errorResponse?.Message ?? "Registration failed");
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (authResponse?.Token != null)
                {
                    await _localStorage.SetItemAsync("accessToken", authResponse.Token.AccessToken);
                    await _localStorage.SetItemAsync("refreshToken", authResponse.Token.RefreshToken);

                    _authStateProvider.NotifyUserAuthentication(authResponse.Token.AccessToken);
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token.AccessToken);
                }

                return (true, authResponse?.Message ?? "Login successful");
            }

            var errorResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return (false, errorResponse?.Message ?? "Login failed");
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("accessToken");
        await _localStorage.RemoveItemAsync("refreshToken");

        _authStateProvider.NotifyUserLogout();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<UserProfile?> GetProfileAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("accessToken");
            if (string.IsNullOrEmpty(token))
                return null;

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.GetFromJsonAsync<UserProfile>("api/auth/profile");
        }
        catch
        {
            return null;
        }
    }
}