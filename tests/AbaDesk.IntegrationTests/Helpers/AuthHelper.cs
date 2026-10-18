using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AbaDesk.IntegrationTests.Fixtures;
using AbaDeskBack.DTOs.Auth;

namespace AbaDesk.IntegrationTests.Helpers;

public static class AuthHelper
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<AuthResponse> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return authResponse!;
    }

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(AbaDeskWebApplicationFactory factory, string email, string password)
    {
        var client = factory.CreateClient();
        var auth = await LoginAsync(client, email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }
}
