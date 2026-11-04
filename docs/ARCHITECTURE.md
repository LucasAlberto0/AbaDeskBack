# Arquitetura do Backend — AbaDesk API

Este documento detalha a arquitetura técnica, os padrões de software, o ciclo de vida de dependências e os componentes centrais da API RESTful do **AbaDesk** (.NET 9).

---

## 1. Padrão Arquitetural: Service Layer Pattern

A API foi projetada segundo o padrão **Service Layer**, estabelecendo uma fronteira clara e coesa entre o transporte HTTP e as regras de negócio:

```text
Requisição HTTP (JSON / Multipart)
        │
        ▼
┌─────────────────────────────────┐
│     Controllers (Camada HTTP)   │  Validação de ModelState, extração de Claims (Usuário/Role)
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
│       Banco PostgreSQL          │  Tabelas, índices, constraints e integridade relacional
└─────────────────────────────────┘
```

### Por que não utilizamos Repository Pattern Genérico?
O **Entity Framework Core** já implementa internamente os padrões *Repository* (`DbSet<T>`) e *Unit of Work* (`DbContext`). A criação de uma camada extra de repositório introduziria abstração desnecessária (*leaky abstraction*) e reduziria o aproveitamento dos recursos avançados do LINQ e do EF Core (como projeções com `.Select()`, paginação e `.AsNoTracking()`). A camada de serviço comunica-se diretamente com o `AppDbContext`, mantendo o código limpo, testável e manutenível.

---

## 2. Injeção de Dependências e Ciclo de Vida dos Serviços

Os serviços são registrados no container nativo do ASP.NET Core em [`Program.cs`](../Program.cs):

| Interface | Implementação | Ciclo de Vida | Finalidade |
| :--- | :--- | :---: | :--- |
| `AppDbContext` | `AppDbContext` | **Scoped** | Instância única por requisição HTTP, garantindo transações atômicas com o banco. |
| `IAuthService` | `AuthService` | **Scoped** | Login, registro, validação de credenciais e emissão de tokens JWT. |
| `ITicketService` | `TicketService` | **Scoped** | CRUD, máquina de estados, histórico de auditoria e manipulação de anexos. |
| `IDashboardService` | `DashboardService` | **Scoped** | Agregações de banco de dados e totalizadores para os painéis. |
| `IUserService` | `UserService` | **Scoped** | Gestão administrativa de usuários, inativação e perfis. |
| `INotificationService` | `NotificationService` | **Scoped** | Persistência e disparo de notificações aos usuários. |
| `IAiService` | `AiService` | **Scoped** | Comunicação com o agente de IA via cliente HTTP tipado. |
| `IPasswordHasher<User>` | `PasswordHasher<User>` | **Scoped** | Algoritmo PBKDF2 com salt para hashing seguro de senhas. |
| `DbSeeder` | `DbSeeder` | **Transient** | Executado na inicialização para semeadura e migração de dados. |

---

## 3. Comunicação em Tempo Real com SignalR (`TicketHub`)

O hub de WebSockets [`TicketHub.cs`](../Hubs/TicketHub.cs) permite comunicação bidirecional de baixa latência:

```text
Cliente (Browser / App) <─────── WebSocket / SSE / LongPolling ───────> TicketHub (/hubs/ticket)
```

### 3.1 Autenticação no Handshake do WebSocket
Navegadores não permitem adicionar cabeçalhos customizados na criação de instâncias nativas de `WebSocket`. Para viabilizar a autenticação via JWT, o ASP.NET Core intercepta o token na query string `?access_token=<token>`:

```csharp
x.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/ticket"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
};
```

### 3.2 Grupos e Broadcasting
- `TicketHub` mapeia conexões por usuário autenticado.
- O serviço `NotificationService` injeta `IHubContext<TicketHub>` para notificar o usuário quando um chamado de seu interesse for atualizado.

---

## 4. Integração com Inteligência Artificial (`AiService`)

A integração com o agente de IA é desacoplada através de um **Typed HttpClient**:

```csharp
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddHttpClient<IAiService, AiService>((serviceProvider, client) =>
{
    var options = builder.Configuration.GetSection("OpenAI").Get<OpenAiOptions>() ?? new OpenAiOptions();
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 60);
});
```

- **Resiliência:** Tratamento explícito de `TimeoutException` e `HttpRequestException`.
- **Sessões Isoladas:** Cada conversa mantém um identificador de sessão (`SessionId`), permitindo continuidade de contexto entre mensagens sucessivas do mesmo usuário.

---

## 5. Armazenamento de Anexos no Banco de Dados

Para garantir integridade transacional e portabilidade em ambientes conteinerizados ou de nuvem sem persistência de disco local, os arquivos de imagem/evidência são armazenados como colunas binárias na tabela `Tickets`:

- `AttachmentData` (`byte[]`): Conteúdo binário original do arquivo.
- `AttachmentContentType` (`string`): MIME type (ex.: `image/png`, `image/jpeg`).
- `AttachmentFileName` (`string`): Nome original do arquivo anexado.

### Endpoint Dedicado de Download
O endpoint `GET /api/tickets/{id}/attachment` lê os bytes diretamente do banco e responde como stream HTTP binário com cabeçalho de tipo apropriado (`File(attachment.Data, attachment.ContentType, attachment.FileName)`).

---

## 6. Autenticação, Tokens JWT e Segurança

A camada de segurança utiliza o middleware oficial `Microsoft.AspNetCore.Authentication.JwtBearer`:

- **Chave de Assinatura Simétrica:** Baseada em segredo com no mínimo 256 bits.
- **Validações Estritas Ativas:**
  - `ValidateIssuer = true` (Emissor oficial da API)
  - `ValidateAudience = true` (Audiência restrita a clientes autorizados)
  - `ValidateLifetime = true` (Verificação de expiração sem tolerância de atraso: `ClockSkew = TimeSpan.Zero`)
  - `ValidateIssuerSigningKey = true` (Validação criptográfica da assinatura)
- **Claims Embutidas no Token:**
  - `ClaimTypes.NameIdentifier`: ID único do usuário (`Guid`)
  - `ClaimTypes.Email`: E-mail corporativo
  - `ClaimTypes.Role`: Papel no sistema (*User*, *Attendant*, *Admin*)
  - `name`: Nome completo do colaborador

---

## 7. Tratamento de Exceções e Respostas Padronizadas

As exceções de negócio lançadas na camada de serviço são traduzidas em códigos de status HTTP semânticos pelos Controllers:

| Tipo de Exceção | Código HTTP | Significado |
| :--- | :---: | :--- |
| `KeyNotFoundException` | **404 Not Found** | O recurso solicitado (chamado, usuário ou anexo) não foi localizado. |
| `UnauthorizedAccessException` | **403 Forbidden** | O usuário autenticado não possui permissão para executar a ação no recurso. |
| `InvalidOperationException` | **400 Bad Request** | Violação de regra de negócio ou transição de status inválida no workflow. |
| Erros de `ModelState` | **400 Bad Request** | Falha de validação dos dados de entrada (campos obrigatórios, tamanhos, formatos). |
| Exceções não tratadas | **500 Internal Server Error** | Falhas inesperadas de infraestrutura (capturadas sem expor stack traces sensíveis). |

