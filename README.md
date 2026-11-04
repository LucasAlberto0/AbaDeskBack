# AbaDesk — API RESTful (Backend)

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql)](https://www.postgresql.org/)
[![Entity Framework Core](https://img.shields.io/badge/EF_Core-9.0-68217A?style=for-the-badge&logo=dotnet)](https://learn.microsoft.com/ef/core/)
[![SignalR](https://img.shields.io/badge/SignalR-RealTime-512BD4?style=for-the-badge&logo=dotnet)](https://learn.microsoft.com/aspnet/core/signalr)
[![Swagger](https://img.shields.io/badge/OpenAPI-Swagger-85EA2D?style=for-the-badge&logo=swagger)](https://swagger.io/)
[![xUnit](https://img.shields.io/badge/Tests-xUnit_|_Moq-007ACC?style=for-the-badge)](https://xunit.net/)

> **Motor de Regras de Negócio e Serviços em Tempo Real da Plataforma ABA Desk**  
> **Aviso:** Este projeto é um **teste técnico desenvolvido para a empresa Grupo ABA Infra**.

Central inteligente e robusta de serviços desenvolvida em **.NET 9 (C#)** para orquestrar o ciclo completo de atendimento, análise, desenvolvimento, testes de qualidade (QA), homologação e resolução de chamados de tecnologia do **Grupo ABA Infra**.

---

## 📌 Sumário

- [Visão Geral](#-visão-geral)
- [Principais Capacidades](#-principais-capacidades)
- [Stack Tecnológica](#-stack-tecnológica)
- [Estrutura da Solução](#-estrutura-da-solução)
- [Configuração de Ambiente e Segurança](#-configuração-de-ambiente-e-segurança)
- [Instalação e Execução Local](#-instalação-e-execução-local)
- [Banco de Dados e Seeder](#-banco-de-dados-e-seeder)
- [Credenciais Padrão de Demonstração](#-credenciais-padrão-de-demonstração)
- [Suíte de Testes Automatizados](#-suíte-de-testes-automatizados)
- [Documentação Técnica Completa](#-documentação-técnica-completa)

---

## 🌐 Visão Geral

A API do **AbaDesk** é projetada seguindo o padrão **Service Layer Pattern**, isolando a camada de transporte HTTP das regras estritas de negócio, transições de estado e segurança corporativa.

O sistema atua como árbitro confiável do ciclo de vida das demandas corporativas:
- Garante transições de status válidas e auditadas em linha do tempo imutável.
- Impede saltos indevidos de etapa no fluxo técnico.
- Provê canal em tempo real via **SignalR WebSockets** para notificações instantâneas.
- Integra assistente inteligente (**ABA Assist**) via OpenAI para diagnósticos técnicos.
- Armazena anexos e evidências binárias de forma segura e transacional no PostgreSQL.

---

## 🚀 Principais Capacidades

- **Autenticação & Autorização RBAC:** Tokens JWT assinados com claims de papel (*User*, *Attendant*, *Admin*) e hash seguro com `IPasswordHasher<User>`.
- **Motor de Workflow Rígido:** Máquina de estados controlando o ciclo: *Open → InAnalysis → InProgress → WaitingUser → Resolved*, com suporte a reabertura formal.
- **Auditoria Completa (Audit Trail):** Registro automático de quem alterou o chamado, quando alterou, status anterior e novo status.
- **Central de Notificações em Tempo Real:** Broadcasting de eventos via WebSocket com persistência de notificações no banco.
- **Inteligência Artificial Operacional (ABA Assist):** Endpoint integrado com streaming/resiliência para diagnóstico de incidentes.
- **Anexos Binários no Banco:** Armazenamento direto de evidências em `byte[]` com `ContentType` e `FileName`, eliminando dependências externas de storage no MVP.

---

## 🛠️ Stack Tecnológica

| Camada / Função | Tecnologia | Versão / Detalhes |
| :--- | :--- | :--- |
| **Plataforma Core** | .NET SDK | 9.0+ |
| **Linguagem** | C# | 13.0 |
| **Framework Web** | ASP.NET Core Web API | 9.0 |
| **ORM / Persistência** | Entity Framework Core (Npgsql) | 9.0.4 |
| **Banco de Dados** | PostgreSQL | 15+ / 16 |
| **Autenticação** | JWT Bearer & ASP.NET Identity Core | 9.0 |
| **Tempo Real (WebSockets)** | ASP.NET Core SignalR | 9.0 |
| **Documentação da API** | Swashbuckle Swagger & OpenAPI Annotations | 10.2+ |
| **IA / LLM Integration** | OpenAI SDK & HttpClient com timeout resiliente | 2.14+ |
| **Testes Unitários & Mocks** | xUnit, Moq, FluentAssertions | Últimas |
| **Testes de Integração** | WebApplicationFactory & EF Core InMemory | 9.0 |

---

## 📂 Estrutura da Solução

```text
AbaDeskBack/
├── Configuration/        # Classes fortemente tipadas de configuração (Options)
│   └── OpenAiOptions.cs  # Configurações do serviço de IA
├── Controllers/          # Endpoints HTTP REST da API
│   ├── AiController.cs            # Chat com o agente ABA Assist
│   ├── AuthController.cs          # Login, registro, perfil e me
│   ├── DashboardController.cs     # Totalizadores e métricas
│   ├── NotificationsController.cs # Leitura e exclusão de notificações
│   ├── TicketsController.cs       # CRUD, workflow, comentários, anexos
│   └── UsersController.cs         # Gestão administrativa de usuários
├── Data/                 # Persistência e sementes de banco
│   ├── AppDbContext.cs   # Mapeamento do EF Core, índices e chaves
│   └── DbSeeder.cs       # Carga inicial de usuários e chamados
├── DTOs/                 # Contratos de entrada e saída (Data Transfer Objects)
│   ├── Ai/ Auth/ Comments/ Common/ Dashboard/ History/ Notifications/ Tickets/ Users/
├── Entities/             # Modelos de domínio do banco relacional
│   ├── User.cs Ticket.cs TicketHistory.cs TicketComment.cs Notification.cs
├── Enums/                # Enumerações de domínio (Role, Status, Priority, Category)
├── Hubs/                 # Hub SignalR para WebSockets em tempo real
│   └── TicketHub.cs      # Conexões ativas e broadcast de notificações
├── Migrations/           # Migrações versionadas do Entity Framework Core
├── Services/             # Camada de Regras de Negócio (Service Layer)
│   ├── Interfaces/       # Contratos de serviços (IAuthService, ITicketService, ...)
│   └── Implementations/  # Implementações com validações e transações
├── tests/                # Suíte de testes automatizados
│   ├── AbaDesk.UnitTests/         # Testes unitários de regras de negócio
│   └── AbaDesk.IntegrationTests/  # Testes de integração de endpoints e segurança
├── Program.cs            # Configuração de serviços, injeção de dependência e pipeline HTTP
└── appsettings.json      # Configurações e parametrização do ambiente
```

---

## 🔐 Configuração de Ambiente e Segurança

As configurações da API são lidas do `appsettings.json`, de **User Secrets** (no desenvolvimento) ou de **Variáveis de Ambiente** (no deploy).

### Exemplo Seguro de Configuração (`appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=abadesk_db;Username=postgres;Password=your_password"
  },
  "Jwt": {
    "Key": "your-256-bit-secret-key-here-minimum-32-characters",
    "Issuer": "AbaDeskAPI",
    "Audience": "AbaDeskClients",
    "ExpirationMinutes": 1440
  },
  "OpenAI": {
    "ApiKey": "",
    "AgentId": "agent_your_agent_id",
    "BaseUrl": "https://api.openai.com/v1/",
    "TimeoutSeconds": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **Diretriz de Segurança Rigorosa:** Nenhuma chave de produção ou credencial confidencial é versionada no repositório. Em ambientes de produção (ex.: Render / Docker), utilize variáveis de ambiente do sistema com a convenção do ASP.NET Core (ex.: `ConnectionStrings__DefaultConnection` e `Jwt__Key`).

---

## 💻 Instalação e Execução Local

### Pré-requisitos
- **.NET 9 SDK** instalado
- **PostgreSQL** ativo localmente ou via container Docker
- Ferramenta de linha de comando `dotnet-ef` (opcional, para novas migrações)

### Passo a Passo

1. Navegue até o diretório do backend:
   ```bash
   cd AbaDeskBack
   ```

2. Restaure as dependências do projeto:
   ```bash
   dotnet restore
   ```

3. Configure a string de conexão no `appsettings.Development.json` apontando para sua instância do PostgreSQL.

4. Aplique as migrações no banco de dados:
   ```bash
   dotnet ef database update
   ```

5. Execute a aplicação:
   ```bash
   dotnet run
   ```

6. Acesse a documentação interativa do Swagger:
   ```text
   http://localhost:5017/swagger
   ```

---

## 🗄️ Banco de Dados e Seeder

Na primeira inicialização, o serviço [`DbSeeder.cs`](./Data/DbSeeder.cs) verifica se já existem usuários cadastrados. Caso o banco esteja vazio:
- Cria automaticamente contas para **Admin**, **Atendentes** e **Usuários**.
- Gera chamados de exemplo em diferentes estágios do workflow (*Open*, *InAnalysis*, *InProgress*, *Resolved*).
- Popula comentários e históricos para permitir validação imediata da interface.

### Credenciais Padrão do Seeder

| Perfil | E-mail | Senha Padrão |
| :--- | :--- | :--- |
| **Administrador** | `admin@abadesk.local` | `senha123` |
| **Atendente 1** | `atendente1@abadesk.local` | `senha123` |
| **Atendente 2** | `atendente2@abadesk.local` | `senha123` |
| **Usuário 1** | `usuario1@abadesk.local` | `senha123` |
| **Usuário 2** | `usuario2@abadesk.local` | `senha123` |

---

## 🧪 Suíte de Testes Automatizados

O backend conta com duas suítes automatizadas executadas via **xUnit**:

```bash
# Executa todos os testes da solução (Unitários e de Integração)
dotnet test AbaDesk.sln

# Executa apenas os testes unitários
dotnet test tests/AbaDesk.UnitTests

# Executa apenas os testes de integração
dotnet test tests/AbaDesk.IntegrationTests
```

- **AbaDesk.UnitTests:** Valida regras de negócio puras do `TicketService` com mocks de dependências.
- **AbaDesk.IntegrationTests:** Valida contratos HTTP reais, autenticação com JWT, proteção de rotas com RBAC e respostas da API utilizando servidor in-memory (`WebApplicationFactory`) e banco isolado `Microsoft.EntityFrameworkCore.InMemory`.

---

## 📚 Documentação Técnica Completa

Para aprofundamento em cada subsistema do backend, consulte os documentos em [`docs/`](./docs/):

- 📖 [**Sobre o Projeto e Regras de Negócio** (`docs/ABOUT_PROJECT.md`)](./docs/ABOUT_PROJECT.md): Visão corporativa, fluxos e máquina de estados.
- 📐 [**Arquitetura do Backend** (`docs/ARCHITECTURE.md`)](./docs/ARCHITECTURE.md): Service Layer, injeção de dependência, SignalR e IA.
- 🔌 [**Catálogo de Endpoints da API** (`docs/API_ENDPOINTS.md`)](./docs/API_ENDPOINTS.md): Especificação completa de todas as rotas REST e eventos do WebSocket.
- 🗃️ [**Esquema do Banco de Dados** (`docs/DATABASE_SCHEMA.md`)](./docs/DATABASE_SCHEMA.md): Diagrama ERD, tabelas, campos, índices e relacionamentos.
- 🔐 [**Workflow e Segurança** (`docs/WORKFLOW_AND_SECURITY.md`)](./docs/WORKFLOW_AND_SECURITY.md): Quality gates, matriz de permissões RBAC e políticas de autenticação.
- 🧪 [**Testes Automatizados** (`docs/TESTS.md`)](./docs/TESTS.md): Estrutura de testes unitários e de integração, asserções e relatórios.
