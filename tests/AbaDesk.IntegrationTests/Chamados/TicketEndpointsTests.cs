using System.Net;
using System.Net.Http.Json;
using AbaDesk.IntegrationTests.Fixtures;
using AbaDesk.IntegrationTests.Helpers;
using AbaDeskBack.DTOs.Tickets;
using AbaDeskBack.Enums;
using FluentAssertions;
using Xunit;

namespace AbaDesk.IntegrationTests.Chamados;

public class TicketEndpointsTests : IClassFixture<AbaDeskWebApplicationFactory>
{
    private readonly AbaDeskWebApplicationFactory _factory;

    public TicketEndpointsTests(AbaDeskWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeveCriarChamadoComSucessoRetornando201Created()
    {
        var userClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "usuario1@abadesk.local", "senha123");

        var request = new CreateTicketRequest
        {
            Title = "Lentidao no sistema interno",
            Description = "O sistema esta demorando para carregar as telas",
            Category = Category.Bug,
            Priority = Priority.Medium,
            CompanyUnit = "Matriz",
            Department = "Financeiro",
            SystemName = "ERP",
            AttachmentBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
        };

        var response = await userClient.PostAsJsonAsync("/api/tickets", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var ticket = await response.Content.ReadFromJsonAsync<TicketResponse>(AuthHelper.JsonOptions);
        ticket.Should().NotBeNull();
        ticket!.Id.Should().NotBeEmpty();
        ticket.Title.Should().Be("Lentidao no sistema interno");
        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.ProtocolNumber.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task DeveCriarChamadoSemAnexoComSucessoRetornando201Created()
    {
        var userClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "usuario1@abadesk.local", "senha123");

        var request = new CreateTicketRequest
        {
            Title = "Dúvida sobre relatório",
            Description = "Como emitir o relatório mensal no sistema",
            Category = Category.Question,
            Priority = Priority.Low,
            CompanyUnit = "Matriz",
            Department = "Financeiro",
            SystemName = "ERP",
            AttachmentBase64 = null
        };

        var response = await userClient.PostAsJsonAsync("/api/tickets", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var ticket = await response.Content.ReadFromJsonAsync<TicketResponse>(AuthHelper.JsonOptions);
        ticket.Should().NotBeNull();
        ticket!.Id.Should().NotBeEmpty();
        ticket.Title.Should().Be("Dúvida sobre relatório");
        ticket.Status.Should().Be(TicketStatus.Open);
    }

    [Fact]
    public async Task DeveConsultarDetalhesDoChamadoCriadoComSucesso()
    {
        var userClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "usuario1@abadesk.local", "senha123");

        var createResponse = await userClient.PostAsJsonAsync("/api/tickets", new CreateTicketRequest
        {
            Title = "Consulta de chamado",
            Description = "Verificacao de retorno detalhado",
            Category = Category.Question,
            Priority = Priority.Low,
            CompanyUnit = "Matriz",
            Department = "Operacoes",
            SystemName = "Portal",
            AttachmentBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TicketResponse>(AuthHelper.JsonOptions);

        var getResponse = await userClient.GetAsync($"/api/tickets/{created!.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var details = await getResponse.Content.ReadFromJsonAsync<TicketResponse>(AuthHelper.JsonOptions);
        details.Should().NotBeNull();
        details!.Id.Should().Be(created.Id);
        details.Title.Should().Be("Consulta de chamado");
    }

    [Fact]
    public async Task DevePermitirSuporteIniciarAnaliseDoChamadoComSucesso()
    {
        var userClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "usuario1@abadesk.local", "senha123");
        var attendantClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "atendente1@abadesk.local", "senha123");

        var createResponse = await userClient.PostAsJsonAsync("/api/tickets", new CreateTicketRequest
        {
            Title = "Chamado para analise tecnica",
            Description = "Aguardando atendimento da equipe",
            Category = Category.Improvement,
            Priority = Priority.High,
            CompanyUnit = "Matriz",
            Department = "Logistica",
            SystemName = "WMS",
            AttachmentBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TicketResponse>(AuthHelper.JsonOptions);

        var analyzeResponse = await attendantClient.PostAsync($"/api/tickets/{created!.Id}/analyze", null);

        analyzeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await attendantClient.GetAsync($"/api/tickets/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<TicketResponse>(AuthHelper.JsonOptions);
        updated!.Status.Should().Be(TicketStatus.InAnalysis);
    }

    [Fact]
    public async Task DeveRetornarAnexoBinarioDoChamadoCriadoDiretoDoBanco()
    {
        var userClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "usuario1@abadesk.local", "senha123");
        const string base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

        var createResponse = await userClient.PostAsJsonAsync("/api/tickets", new CreateTicketRequest
        {
            Title = "Chamado com anexo no banco",
            Description = "Validando extracao de arquivo binario",
            Category = Category.Bug,
            Priority = Priority.High,
            CompanyUnit = "Matriz",
            Department = "TI",
            SystemName = "Portal",
            AttachmentBase64 = $"data:image/png;base64,{base64Data}"
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TicketResponse>(AuthHelper.JsonOptions);

        var anonymousClient = _factory.CreateClient();
        var attachmentResponse = await anonymousClient.GetAsync($"/api/tickets/{created!.Id}/attachment");

        attachmentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        attachmentResponse.Content.Headers.ContentType!.MediaType.Should().Be("image/png");

        var bytes = await attachmentResponse.Content.ReadAsByteArrayAsync();
        bytes.Should().Equal(Convert.FromBase64String(base64Data));
    }
}
