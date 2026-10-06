using AbaDeskBack.Entities;
using AbaDeskBack.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AbaDeskBack.Data;

public class DbSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public DbSeeder(AppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            return; // Already seeded
        }

        // Seed Users
        var users = new List<User>
        {
            new User { Name = "Admin User", Email = "admin@abadesk.local", Role = Role.Admin, IsActive = true },
            new User { Name = "Atendente 1", Email = "atendente1@abadesk.local", Role = Role.Attendant, IsActive = true },
            new User { Name = "Atendente 2", Email = "atendente2@abadesk.local", Role = Role.Attendant, IsActive = true },
            new User { Name = "Usuário 1", Email = "usuario1@abadesk.local", Role = Role.User, IsActive = true },
            new User { Name = "Usuário 2", Email = "usuario2@abadesk.local", Role = Role.User, IsActive = true }
        };

        foreach (var user in users)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, "senha123");
        }

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Seed Tickets
        var admin = users.First(u => u.Role == Role.Admin);
        var attendant1 = users.First(u => u.Email == "atendente1@abadesk.local");
        var user1 = users.First(u => u.Email == "usuario1@abadesk.local");

        var tickets = new List<Ticket>
        {
            // Ticket 1 - NEW
            new Ticket
            {
                ProtocolNumber = "1001", Title = "Falha no login do ERP", Description = "Não consigo logar no ERP com minha senha.",
                CompanyUnit = "Matriz", Department = "Financeiro", SystemName = "ERP", Category = Category.Bug, Priority = Priority.High,
                Status = TicketStatus.NEW, CreatedByUserId = user1.Id, HomologationResponsibleUserId = user1.Id
            },
            // Ticket 2 - ANALYZING
            new Ticket
            {
                ProtocolNumber = "1002", Title = "Criar relatório de vendas", Description = "Precisamos de um relatório com as vendas mensais consolidadas.",
                CompanyUnit = "Filial SP", Department = "Comercial", SystemName = "CRM", Category = Category.NewFeature, Priority = Priority.Medium,
                Status = TicketStatus.ANALYZING, CreatedByUserId = user1.Id, AssignedToUserId = attendant1.Id, HomologationResponsibleUserId = user1.Id
            },
            // Ticket 3 - IN_DEVELOPMENT
            new Ticket
            {
                ProtocolNumber = "1003", Title = "Lentidão no sistema de RH", Description = "A tela de folha de pagamento demora muito para carregar.",
                CompanyUnit = "Matriz", Department = "RH", SystemName = "RH System", Category = Category.Question, Priority = Priority.Low,
                Status = TicketStatus.IN_DEVELOPMENT, CreatedByUserId = admin.Id, AssignedToUserId = attendant1.Id, HomologationResponsibleUserId = admin.Id
            },
            // Ticket 4 - RESOLVED
            new Ticket
            {
                ProtocolNumber = "1004", Title = "Atualizar permissão de usuário", Description = "Gostaria de solicitar acesso à pasta do servidor.",
                CompanyUnit = "Matriz", Department = "TI", SystemName = "FileServer", Category = Category.Access, Priority = Priority.Medium,
                Status = TicketStatus.RESOLVED, CreatedByUserId = user1.Id, AssignedToUserId = admin.Id, HomologationResponsibleUserId = user1.Id,
                ResolvedAt = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };

        await _context.Tickets.AddRangeAsync(tickets);
        await _context.SaveChangesAsync();

        // Seed Histories for Ticket 4
        var resolvedTicket = tickets.Last();
        _context.TicketHistories.Add(new TicketHistory { TicketId = resolvedTicket.Id, UserId = user1.Id, Action = "Chamado criado", Description = "Abertura" });
        _context.TicketHistories.Add(new TicketHistory { TicketId = resolvedTicket.Id, UserId = admin.Id, Action = "Chamado resolvido", FromStatus = TicketStatus.NEW, ToStatus = TicketStatus.RESOLVED, Description = "Acesso concedido." });
        
        // Seed Comment
        _context.TicketComments.Add(new TicketComment { TicketId = resolvedTicket.Id, UserId = admin.Id, Content = "O acesso já foi liberado. Pode testar?" });

        await _context.SaveChangesAsync();
    }
}
