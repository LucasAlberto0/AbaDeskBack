# AbaDesk — API RESTful (Ambiente Dev)

[![Ambiente](https://img.shields.io/badge/Ambiente-Desenvolvimento_(dev)-yellow?style=for-the-badge)](https://github.com/)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Local-4169E1?style=for-the-badge&logo=postgresql)](https://www.postgresql.org/)
[![Entity Framework Core](https://img.shields.io/badge/EF_Core-9.0-68217A?style=for-the-badge&logo=dotnet)](https://learn.microsoft.com/ef/core/)
[![SignalR](https://img.shields.io/badge/SignalR-LocalHub-512BD4?style=for-the-badge&logo=dotnet)](https://learn.microsoft.com/aspnet/core/signalr)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger)](https://swagger.io/)
[![xUnit](https://img.shields.io/badge/Tests-xUnit_|_Moq-007ACC?style=for-the-badge)](https://xunit.net/)

> **Branch de Desenvolvimento (`dev`):** Núcleo de serviços e regras de negócio do **AbaDesk**, desenvolvido como um **projeto de teste técnico para a empresa Grupo ABA**. Configurado para desenvolvimento local ágil, testes e iteração de novas features.

---

## 📌 Sumário

- [Visão Geral do Ambiente Dev](#-visão-geral-do-ambiente-dev)
- [Funcionalidades e Módulos Ativos](#-funcionalidades-e-módulos-ativos)
- [Configuração de Ambiente Local](#-configuração-de-ambiente-local)
- [Execução Local Passo a Passo](#-execução-local-passo-a-passo)
- [Swagger UI & Teste de Endpoints](#-swagger-ui--teste-de-endpoints)
- [Banco de Dados e Carga Inicial (Seeder)](#-banco-de-dados-e-carga-inicial-seeder)
- [Credenciais Padrão para Testes](#-credenciais-padrão-para-testes)
- [Executando Testes Automatizados](#-executando-testes-automatizados)
- [Documentação Técnica da Branch Dev](#-documentação-técnica-da-branch-dev)

---

## 🛠️ Visão Geral do Ambiente Dev

Na branch `dev`, o backend foi estruturado para fornecer o máximo de produtividade ao desenvolvedor local:

- **Swagger / OpenAPI Sempre Ativo em Desenvolvimento:** Documentação viva interativa exposta em `http://localhost:5017/swagger`.
- **CORS Permissivo (`AllowAll`):** Habilita qualquer origem local (incluindo Vite em `localhost:5173` ou portas alternativas) com suporte completo a credenciais e cabeçalhos.
- **SignalR Hub Local (`/hubs/ticket`):** Conexão WebSocket local imediata com autenticação de token via query string.
- **Seeder Automático na Inicialização:** Popula usuários de teste, chamados em diferentes fases, comentários e histórico sempre que o banco de dados estiver zerado.
- **Migração Transparente de Anexos:** Serviço `MigrateDiskAttachmentsAsync` que converte arquivos legados em bytes no PostgreSQL automaticamente.

---

## ✨ Funcionalidades e Módulos Ativos

1. **Autenticação e Perfis (JWT + Identity Core):**
   - Endpoints para Login, Registro, Consulta de perfil (`/api/auth/me`) e Atualização de dados.
   - Hashing seguro via `IPasswordHasher<User>`.
2. **Motor de Workflow e Gestão de Chamados:**
   - CRUD completo com paginação e busca textual.
   - Transições de estado controladas (*Open → InAnalysis → InProgress → WaitingUser → Resolved*).
   - Ação de reabertura com justificativa obrigatória.
   - Download de evidências e anexos binários direto do banco (`/api/tickets/{id}/attachment`).
3. **Auditoria e Colaboração:**
   - Trilha de histórico imutável (Audit Trail).
   - Comentários em tempo real sincronizados via SignalR.
4. **Central de Notificações Internas:**
   - Persistência de notificações no banco e emissão via WebSocket para os usuários destinatários.
5. **Inteligência Artificial (ABA Assist):**
   - Integração com agentes OpenAI via `HttpClient` resiliente (`/api/ai/chat`), com sessões contínuas.
6. **Métricas de Gestão (Dashboard):**
   - Totalizadores agregados diretamente no PostgreSQL, isolados por nível de privilégio.

---

## 🔐 Configuração de Ambiente Local

No ambiente `dev`, as configurações locais devem residir em `appsettings.Development.json` ou serem injetadas via **User Secrets**:

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

> **Aviso de Segurança:** Nunca versione senhas de produção ou chaves privadas reais no Git. Utilize `dotnet user-secrets` para sobrescrever chaves de teste localmente.

---

## 💻 Execução Local Passo a Passo

### Pré-requisitos
- **.NET 9 SDK** instalado
- Instância do **PostgreSQL** ativa localmente (porta 5432)

### Comandos de Inicialização

```bash
# 1. Navegue até a pasta do backend
cd AbaDeskBack

# 2. Restaure os pacotes NuGet
dotnet restore

# 3. Aplique as migrações no PostgreSQL local
dotnet ef database update

# 4. Inicie o servidor
dotnet run
```

O servidor iniciará escutando por padrão em `http://localhost:5017`.

---

## 🌐 Swagger UI & Teste de Endpoints

Com a API rodando, acesse no navegador:
```text
http://localhost:5017/swagger
```

Pelo Swagger, você pode:
1. Executar `POST /api/auth/login` com uma das credenciais padrão para obter o token JWT.
2. Clicar no botão **Authorize** no topo da página e informar `Bearer <seu_token>`.
3. Executar e validar qualquer endpoint de chamados, comentários, notificações ou IA.

---

## 🗄️ Banco de Dados e Carga Inicial (Seeder)

Ao iniciar com o banco recém-criado, o [`DbSeeder.cs`](./Data/DbSeeder.cs) cria automaticamente a base de testes:

| Perfil | E-mail | Senha Padrão | Escopo de Teste |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin@abadesk.local` | `senha123` | Gerenciamento de usuários e visão global |
| **Atendente 1** | `atendente1@abadesk.local` | `senha123` | Fila de atendimento e movimentação de status |
| **Atendente 2** | `atendente2@abadesk.local` | `senha123` | Atendimento simultâneo de chamados |
| **Usuário 1** | `usuario1@abadesk.local` | `senha123` | Abertura de chamados e homologação |
| **Usuário 2** | `usuario2@abadesk.local` | `senha123` | Chamados de outras unidades |

---

## 🧪 Executando Testes Automatizados

A suíte na branch `dev` contempla testes unitários e de integração que não dependem do banco de dados físico (utilizam banco InMemory e mocks):

```bash
# Executa todos os testes da solução
dotnet test AbaDesk.sln

# Executa apenas os testes unitários da Service Layer
dotnet test tests/AbaDesk.UnitTests

# Executa apenas os testes de integração dos Endpoints HTTP e Segurança
dotnet test tests/AbaDesk.IntegrationTests
```

---

## 📚 Documentação Técnica da Branch Dev

Consulte a documentação aprofundada na pasta [`docs/`](./docs/):

- 📖 [**Sobre o Projeto e Regras de Negócio (Dev)** (`docs/ABOUT_PROJECT.md`)](./docs/ABOUT_PROJECT.md)
- 📐 [**Arquitetura do Backend e Serviços (Dev)** (`docs/ARCHITECTURE.md`)](./docs/ARCHITECTURE.md)
- 🔌 [**Catálogo de Endpoints REST e SignalR (Dev)** (`docs/API_ENDPOINTS.md`)](./docs/API_ENDPOINTS.md)
- 🗃️ [**Esquema do Banco de Dados e Migrations (Dev)** (`docs/DATABASE_SCHEMA.md`)](./docs/DATABASE_SCHEMA.md)
- 🔐 [**Workflow, RBAC e Segurança (Dev)** (`docs/WORKFLOW_AND_SECURITY.md`)](./docs/WORKFLOW_AND_SECURITY.md)
- 🧪 [**Guia de Testes Automatizados (Dev)** (`docs/TESTS.md`)](./docs/TESTS.md)
