using System.Net;
using System.Net.Http.Json;
using AbaDesk.IntegrationTests.Fixtures;
using AbaDesk.IntegrationTests.Helpers;
using AbaDeskBack.DTOs.Auth;
using FluentAssertions;
using Xunit;

namespace AbaDesk.IntegrationTests.Auth;

public class AuthEndpointsTests : IClassFixture<AbaDeskWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(AbaDeskWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveAutenticarComSucessoQuandoCredenciaisForemValidas()
    {
        var request = new LoginRequest
        {
            Email = "usuario1@abadesk.local",
            Password = "senha123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>(AuthHelper.JsonOptions);
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrWhiteSpace();
        content.User.Should().NotBeNull();
        content.User.Email.Should().Be("usuario1@abadesk.local");
    }

    [Fact]
    public async Task DeveRetornarUnauthorizedAoTentarLoginComSenhaIncorreta()
    {
        var request = new LoginRequest
        {
            Email = "usuario1@abadesk.local",
            Password = "senha_errada"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
