using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace SkillSync.Web.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationState _anonymous;

    public CustomAuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("accessToken");

        if (string.IsNullOrWhiteSpace(token))
            return _anonymous;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var claims = ParseClaimsFromJwt(token);
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwtAuthType"));

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwtAuthType"));
        var authState = Task.FromResult(new AuthenticationState(user));

        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(_anonymous);
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];

        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            keyValuePairs.TryGetValue(ClaimTypes.NameIdentifier, out var userId);
            keyValuePairs.TryGetValue(ClaimTypes.Email, out var email);
            keyValuePairs.TryGetValue(ClaimTypes.Name, out var name);

            if (userId != null)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()!));

            if (email != null)
                claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));

            if (name != null)
                claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}