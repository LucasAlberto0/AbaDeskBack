# Arquitetura do Backend (Ambiente Dev) — AbaDesk

Este documento descreve a arquitetura técnica, padrões de design e componentes da API RESTful do **AbaDesk** (.NET 9) na branch `dev`.

---

## 1. Padrão Arquitetural: Service Layer Pattern

A API adota o padrão **Service Layer**, mantendo os Controllers focados no protocolo HTTP e concentrando a lógica de negócio na camada de serviços:

```text
Requisição HTTP (JSON / Multipart)
        │
        ▼
┌─────────────────────────────────┐
│     Controllers (Camada HTTP)   │  Validação de ModelState, extração de Claims de Usuário/Role
└─────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────┐
│   Services (Regras de Negócio)  │  Validações, regras de workflow, auditoria e transações
└─────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────┐
│ AppDbContext (EF Core / Npgsql) │  Mapeamento relacional, queries otimizadas e tracking
└─────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────┐
│   PostgreSQL Local (Porta 5432) │  Persistência relacional, constraints e índices
└─────────────────────────────────┘
```

---

## 2. Injeção de Dependências e Configuração no Dev

Configurada em [`Program.cs`](../Program.cs):

| Serviço | Implementação | Escopo | Finalidade no Ambiente Dev |
| :--- | :--- | :---: | :--- |
| `AppDbContext` | `AppDbContext` | **Scoped** | Contexto do Entity Framework conectado ao PostgreSQL local. |
| `IAuthService` | `AuthService` | **Scoped** | Autenticação, emissão e validação de tokens JWT. |
| `ITicketService` | `TicketService` | **Scoped** | Regras de chamados, movimentação de status e auditoria. |
| `IDashboardService` | `DashboardService` | **Scoped** | Agregação de métricas no banco para os painéis. |
| `IUserService` | `UserService` | **Scoped** | Operações administrativas de contas de usuários. |
| `INotificationService` | `NotificationService` | **Scoped** | Gravação e envio de notificações via WebSocket. |
| `IAiService` | `AiService` | **Scoped** | Cliente HTTP tipado conectado à API da OpenAI. |
| `IPasswordHasher<User>`| `PasswordHasher<User>` | **Scoped** | Hashing com PBKDF2 e salt aleatório. |
| `DbSeeder` | `DbSeeder` | **Transient** | Carga inicial e migração de anexos na inicialização. |

---

## 3. Política de CORS no Ambiente Dev

Para facilitar os testes locais com o frontend rodando no Vite (`http://localhost:5173` ou outras portas locais), a política de CORS na branch `dev` é configurada como:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.SetIsOriginAllowed(origin => true)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
```

Isso garante que chamadas autenticadas com cookies/tokens e conexões WebSocket do SignalR funcionem perfeitamente sem bloqueios de origem cruzada.

---

## 4. Hub SignalR Local (`/hubs/ticket`)

O hub WebSocket permite comunicação instantânea:
- Mapeado na rota `/hubs/ticket`.
- Suporte a token JWT passado via query string `?access_token=<token>` para contornar limitações nativas do handshake de WebSockets nos navegadores.
- Emissão de eventos `ReceiveNotification`, `ReceiveComment` e `TicketUpdated`.

---

## 5. Armazenamento Binário de Anexos no Banco

Na branch `dev`, os arquivos de imagem são persistidos diretamente como `byte[]` na tabela `Tickets`:
- Elimina a dependência de pastas locais no disco rígido ou de buckets de nuvem no ambiente de desenvolvimento.
- O endpoint `GET /api/tickets/{id}/attachment` permite testar o download e visualização de anexos de forma desacoplada de serviços externos.

