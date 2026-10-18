using System.Net;
using System.Net.Http.Json;
using AbaDesk.IntegrationTests.Fixtures;
using AbaDesk.IntegrationTests.Helpers;
using AbaDeskBack.DTOs.Tickets;
using AbaDeskBack.Enums;
using FluentAssertions;
using Xunit;

namespace AbaDesk.IntegrationTests.Permissoes;

public class AuthorizationTests : IClassFixture<AbaDeskWebApplicationFactory>
{
    private readonly AbaDeskWebApplicationFactory _factory;
    private readonly HttpClient _anonymousClient;

    public AuthorizationTests(AbaDeskWebApplicationFactory factory)
    {
        _factory = factory;
        _anonymousClient = factory.CreateClient();
    }

    [Fact]
    public async Task DeveRetornarUnauthorizedAoAcessarEndpointProtegidoSemToken()
    {
        var response = await _anonymousClient.GetAsync("/api/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeveRetornarForbiddenQuandoUsuarioComumTentarAcessarGestaoDeUsuarios()
    {
        var userClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "usuario1@abadesk.local", "senha123");

        var response = await userClient.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeveRetornarForbiddenQuandoUsuarioComumTentarIniciarAnaliseDeChamado()
    {
        var userClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "usuario1@abadesk.local", "senha123");

        var createResponse = await userClient.PostAsJsonAsync("/api/tickets", new CreateTicketRequest
        {
            Title = "Problema de teste de permissao",
            Description = "Descricao de teste",
            Category = Category.Bug,
            Priority = Priority.Low,
            CompanyUnit = "Matriz",
            Department = "RH",
            SystemName = "Portal",
            AttachmentBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TicketResponse>(AuthHelper.JsonOptions);

        var analyzeResponse = await userClient.PostAsync($"/api/tickets/{created!.Id}/analyze", null);

        analyzeResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
