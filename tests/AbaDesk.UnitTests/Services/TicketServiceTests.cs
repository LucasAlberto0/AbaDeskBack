using AbaDeskBack.Data;
using AbaDeskBack.DTOs.Tickets;
using AbaDeskBack.Entities;
using AbaDeskBack.Enums;
using AbaDeskBack.Services.Implementations;
using AbaDeskBack.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AbaDesk.UnitTests.Services;

public class TicketServiceTests
{
    private readonly Mock<INotificationService> _notificationServiceMock;

    public TicketServiceTests()
    {
        _notificationServiceMock = new Mock<INotificationService>();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task DeveCriarChamadoComStatusAbertoEHistoricoQuandoDadosForemValidos()
    {
        using var context = CreateDbContext();
        var user = new User { Id = Guid.NewGuid(), Name = "Usuario Teste", Email = "user@teste.com", Role = Role.User, IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new TicketService(context, _notificationServiceMock.Object);
        var request = new CreateTicketRequest
        {
            Title = "Problema com impressora",
            Description = "A impressora nao conecta na rede",
            Category = Category.Bug,
            Priority = Priority.High,
            CompanyUnit = "Matriz",
            Department = "TI",
            SystemName = "Hardware"
        };

        var result = await service.CreateTicketAsync(request, user.Id, Role.User);

        result.Should().NotBeNull();
        result.Title.Should().Be("Problema com impressora");
        result.Status.Should().Be(TicketStatus.Open);
        result.ProtocolNumber.Should().NotBeNullOrWhiteSpace();

        var ticketInDb = await context.Tickets.Include(t => t.History).FirstOrDefaultAsync(t => t.Id == result.Id);
        ticketInDb.Should().NotBeNull();
        ticketInDb!.History.Should().ContainSingle(h => h.Action == "Chamado criado");
    }

    [Fact]
    public async Task DeveImpedirSuporteTecnicoDeCriarChamado()
    {
        using var context = CreateDbContext();
        var service = new TicketService(context, _notificationServiceMock.Object);
        var request = new CreateTicketRequest
        {
            Title = "Chamado por suporte",
            Description = "Tentativa de criacao",
            Category = Category.Bug,
            Priority = Priority.Medium
        };

        var act = async () => await service.CreateTicketAsync(request, Guid.NewGuid(), Role.Attendant);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Suporte técnico não tem permissão para criar chamados.");
    }

    [Fact]
    public async Task DeveImpedirUsuarioComumDeVisualizarChamadoDeOutroUsuario()
    {
        using var context = CreateDbContext();
        var creator = new User { Id = Guid.NewGuid(), Name = "Criador", Email = "criador@teste.com", Role = Role.User, IsActive = true };
        var otherUser = new User { Id = Guid.NewGuid(), Name = "Outro", Email = "outro@teste.com", Role = Role.User, IsActive = true };
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ProtocolNumber = "1001",
            Title = "Chamado confidencial",
            Description = "Descricao confidencial",
            CreatedByUserId = creator.Id,
            Status = TicketStatus.Open
        };

        context.Users.AddRange(creator, otherUser);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var service = new TicketService(context, _notificationServiceMock.Object);

        var act = async () => await service.GetTicketDetailsAsync(ticket.Id, otherUser.Id, Role.User);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Você não tem permissão para acessar este chamado.");
    }

    [Fact]
    public async Task DevePermitirSuporteIniciarAnaliseDeChamadoAberto()
    {
        using var context = CreateDbContext();
        var creator = new User { Id = Guid.NewGuid(), Name = "Criador", Email = "criador@teste.com", Role = Role.User, IsActive = true };
        var attendant = new User { Id = Guid.NewGuid(), Name = "Suporte", Email = "suporte@teste.com", Role = Role.Attendant, IsActive = true };
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ProtocolNumber = "1002",
            Title = "Ajuda com email",
            Description = "Outlook travando",
            CreatedByUserId = creator.Id,
            Status = TicketStatus.Open
        };

        context.Users.AddRange(creator, attendant);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var service = new TicketService(context, _notificationServiceMock.Object);

        await service.StartAnalysisAsync(ticket.Id, attendant.Id, Role.Attendant);

        var updatedTicket = await context.Tickets.FindAsync(ticket.Id);
        updatedTicket!.Status.Should().Be(TicketStatus.InAnalysis);
        updatedTicket.AssignedToUserId.Should().Be(attendant.Id);

        _notificationServiceMock.Verify(n => n.CreateNotificationAsync(
            creator.Id,
            ticket.Id,
            It.IsAny<string>(),
            It.IsAny<string>(),
            NotificationType.StatusChanged), Times.Once);
    }

    [Fact]
    public async Task DeveImpedirUsuarioComumDeIniciarAnaliseDeChamado()
    {
        using var context = CreateDbContext();
        var user = new User { Id = Guid.NewGuid(), Name = "Usuario Comum", Email = "user@teste.com", Role = Role.User, IsActive = true };
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ProtocolNumber = "1003",
            Title = "Problema",
            Description = "Descricao",
            CreatedByUserId = user.Id,
            Status = TicketStatus.Open
        };

        context.Users.Add(user);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var service = new TicketService(context, _notificationServiceMock.Object);

        var act = async () => await service.StartAnalysisAsync(ticket.Id, user.Id, Role.User);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Usuários não possuem permissão para iniciar análise de chamados.");
    }

    [Fact]
    public async Task DeveImpedirIniciarAnaliseQuandoStatusNaoForAberto()
    {
        using var context = CreateDbContext();
        var attendant = new User { Id = Guid.NewGuid(), Name = "Suporte", Email = "suporte@teste.com", Role = Role.Attendant, IsActive = true };
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ProtocolNumber = "1004",
            Title = "Problema em andamento",
            Description = "Ja em atendimento",
            CreatedByUserId = Guid.NewGuid(),
            Status = TicketStatus.InProgress
        };

        context.Users.Add(attendant);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var service = new TicketService(context, _notificationServiceMock.Object);

        var act = async () => await service.StartAnalysisAsync(ticket.Id, attendant.Id, Role.Attendant);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*apenas chamados com status Aberto podem entrar em análise*");
    }

    [Fact]
    public async Task DeveExigirSolucaoAoResolverChamado()
    {
        using var context = CreateDbContext();
        var attendant = new User { Id = Guid.NewGuid(), Name = "Suporte", Email = "suporte@teste.com", Role = Role.Attendant, IsActive = true };
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ProtocolNumber = "1005",
            Title = "Chamado para resolver",
            Description = "Descricao",
            CreatedByUserId = Guid.NewGuid(),
            AssignedToUserId = attendant.Id,
            Status = TicketStatus.InProgress
        };

        context.Users.Add(attendant);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var service = new TicketService(context, _notificationServiceMock.Object);
        var request = new ResolveTicketRequest { Solution = "   " };

        var act = async () => await service.ResolveTicketAsync(ticket.Id, request, attendant.Id, Role.Attendant);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("É obrigatório registrar a solução aplicada para resolver o chamado.");
    }

    [Fact]
    public async Task DeveImpedirExclusaoDeChamadoPeloSuporteQuandoNaoEstiverResolvido()
    {
        using var context = CreateDbContext();
        var attendant = new User { Id = Guid.NewGuid(), Name = "Suporte", Email = "suporte@teste.com", Role = Role.Attendant, IsActive = true };
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ProtocolNumber = "1006",
            Title = "Chamado aberto",
            Description = "Nao resolvido ainda",
            CreatedByUserId = Guid.NewGuid(),
            Status = TicketStatus.Open
        };

        context.Users.Add(attendant);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var service = new TicketService(context, _notificationServiceMock.Object);

        var act = async () => await service.DeleteTicketAsync(ticket.Id, attendant.Id, Role.Attendant);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("O suporte técnico só possui permissão para excluir chamados após serem solucionados/resolvidos.");
    }

    [Fact]
    public async Task DevePermitirSuporteExcluirChamadoQuandoEstiverComStatusResolvido()
    {
        using var context = CreateDbContext();
        var attendant = new User { Id = Guid.NewGuid(), Name = "Suporte", Email = "suporte@teste.com", Role = Role.Attendant, IsActive = true };
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ProtocolNumber = "1007",
            Title = "Chamado resolvido",
            Description = "Descricao do chamado",
            Solution = "Problema corrigido com sucesso",
            CreatedByUserId = Guid.NewGuid(),
            AssignedToUserId = attendant.Id,
            Status = TicketStatus.Resolved
        };

        context.Users.Add(attendant);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var service = new TicketService(context, _notificationServiceMock.Object);

        await service.DeleteTicketAsync(ticket.Id, attendant.Id, Role.Attendant);

        var deletedTicket = await context.Tickets.FindAsync(ticket.Id);
        deletedTicket.Should().BeNull();
    }

    [Fact]
    public async Task DeveImpedirUsuarioComumDeExcluirChamadoDeOutroUsuario()
    {
        using var context = CreateDbContext();
        var creator = new User { Id = Guid.NewGuid(), Name = "Criador", Email = "criador@teste.com", Role = Role.User, IsActive = true };
        var otherUser = new User { Id = Guid.NewGuid(), Name = "Outro", Email = "outro@teste.com", Role = Role.User, IsActive = true };
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ProtocolNumber = "1008",
            Title = "Chamado do criador",
            Description = "Descricao",
            CreatedByUserId = creator.Id,
            Status = TicketStatus.Open
        };

        context.Users.AddRange(creator, otherUser);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var service = new TicketService(context, _notificationServiceMock.Object);

        var act = async () => await service.DeleteTicketAsync(ticket.Id, otherUser.Id, Role.User);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Usuários só podem deletar os próprios chamados.");
    }
}
