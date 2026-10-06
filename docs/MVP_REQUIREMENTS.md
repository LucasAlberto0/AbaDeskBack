# ABA Desk - Back-end MVP

Você será o **desenvolvedor Back-end responsável pelo ABA Desk**, uma plataforma corporativa de atendimento e gestão do ciclo de vida de chamados internos.

Seu objetivo é implementar o **back-end completo e funcional do MVP**, utilizando o projeto .NET 9. O projeto será consumido posteriormente por um front-end React.

---

## 1. CONTEXTO DO SISTEMA
O **ABA Desk** centraliza solicitações relacionadas a sistemas corporativos, controlando o ciclo completo da demanda:
`Usuário abre chamado → Análise → Desenvolvimento → Testes → Homologação → Resolução`

### Tipos de Conta
- **USER (Usuário/Solicitante):** Cria e acompanha os próprios chamados. Não vê chamados de outros, nem altera status arbitrariamente.
- **ATTENDANT (Atendente):** Faz triagem, assume chamados, encaminha para desenvolvimento, cria testes, homologa e resolve. Pode atuar como desenvolvedor (identificado pelo `AssignedToUserId`).
- **ADMIN (Administrador):** Possui as permissões de Atendente + gerencia usuários e visualiza métricas globais.

---

## 2. STACK OBRIGATÓRIA
- .NET 9, ASP.NET Core Web API, C#
- Entity Framework Core, PostgreSQL, Npgsql
- JWT Bearer Authentication
- Swagger/OpenAPI
- Service Layer Pattern
- **Hashing de Senha:** `PasswordHasher<TUser>`

---

## 3. DESIGN PATTERN
- Utilizar **Service Layer Pattern** (Sem Repository Pattern).
- Regra: `Controller → Service → DbContext / EF Core → PostgreSQL`
- **Controllers** lidam apenas com a camada HTTP.
- **Services** contém as regras de negócio, transições de status e validações.

---

## 4. ESTRUTURA DE PASTAS (Sugestão)
```text
ABA.Desk.Api
├── Controllers
├── Services (Interfaces / Implementations)
├── Data (AppDbContext.cs)
├── Entities
├── DTOs (Auth, Users, Tickets, Comments, etc)
├── Enums
├── Exceptions
├── Middleware
├── Authentication
├── Mappings
├── Validators
└── Migrations
```

---

## 5. ENTIDADES PRINCIPAIS

### User
Campos: `Id`, `Name`, `Email`, `PasswordHash`, `Role` (Enum: User, Attendant, Admin), `IsActive`, `CreatedAt`, `UpdatedAt`, `LastLoginAt`

### Ticket (Chamado)
Campos: `Id`, `ProtocolNumber` (ex: 1042), `Title`, `Description`, `CompanyUnit`, `Department`, `SystemName`, `Category`, `Priority`, `Status`, `CreatedByUserId`, `AssignedToUserId`, `HomologationResponsibleUserId`, `CreatedAt`, `UpdatedAt`, `ResolvedAt`

### TicketHistory (Histórico)
Campos: `Id`, `TicketId`, `UserId`, `Action`, `FromStatus`, `ToStatus`, `Description`, `CreatedAt`

### TicketComment (Comentários)
Campos: `Id`, `TicketId`, `UserId`, `Content`, `CreatedAt`, `UpdatedAt`

### TestCase (Casos de Teste)
Campos: `Id`, `TicketId`, `Title`, `Description`, `ExpectedResult`, `ActualResult`, `Status` (Pending, Passed, Failed), `CreatedByUserId`, `CreatedAt`, `UpdatedAt`

### Homologation (Homologação)
Campos: `Id`, `TicketId`, `RequestedByUserId`, `ResponsibleUserId`, `Status` (Pending, Approved, Rejected), `Comment`, `RequestedAt`, `DecidedAt`, `DecidedByUserId`

---

## 6. ENUMS DE DOMÍNIO
- **Category:** Bug, Improvement, NewFeature, Question, Access
- **Priority:** Low, Medium, High, Critical
- **Status:** NEW, ANALYZING, IN_DEVELOPMENT, IN_TEST, WAITING_HOMOLOGATION, RESOLVED

---

## 7. FLUXO DE STATUS E REGRAS
Fluxo oficial: `NEW → ANALYZING → IN_DEVELOPMENT → IN_TEST → WAITING_HOMOLOGATION → RESOLVED`
Fluxo de rejeição: `WAITING_HOMOLOGATION → IN_DEVELOPMENT`

**Regras Essenciais:**
- **IN_TEST para WAITING_HOMOLOGATION:** Só permite se houver pelo menos 1 teste E todos estiverem aprovados.
- **WAITING_HOMOLOGATION para RESOLVED / IN_DEVELOPMENT:** Somente o `HomologationResponsibleUserId` pode aprovar ou rejeitar. Rejeição exige comentário obrigatório.
- **RESOLVED:** Chamado encerrado, nenhuma alteração de fluxo permitida.

---

## 8. AUTENTICAÇÃO E AUTORIZAÇÃO
- **JWT Bearer Authentication.** Payload de resposta deve ter `accessToken`, `expiresAt` e os dados básicos do usuário (nunca retornar `PasswordHash`).
- **Endpoint Current User:** `GET /api/auth/me`.
- **Autorização de Recurso:** A camada de Service deve validar a posse da entidade (ex: USER só acessa e comenta em seus próprios chamados).

---

## 9. ENDPOINTS (Controllers Rest)
- **Auth:** `/api/auth/login`, `/api/auth/me`
- **Tickets:** `/api/tickets` (CRUD e Filtros), `/api/tickets/my`, Ações (`/analyze`, `/assign`, `/start-development`, `/send-to-test`, `/send-to-homologation`, `/resolve`)
- **Comments / History:** `/api/tickets/{id}/comments`, `/api/tickets/{id}/history`
- **Tests:** `/api/tickets/{id}/tests`, `/api/tests/{testId}` (CRUD), `/api/tests/{testId}/execute`
- **Homologations:** `/api/homologations/pending`, `/api/homologations/{id}/approve`, `/api/homologations/{id}/reject`
- **Users:** `/api/users` (Somente ADMIN)
- **Dashboard:** `/api/dashboard` (Retornar totalizadores via banco de dados)

---

## 10. REGRAS GERAIS DE ARQUITETURA E QUALIDADE
- **DTOs e Validações:** Não retornar/receber entidades diretamente. Validar todos os inputs via DataAnnotations ou FluentValidation.
- **Tratamento de Erros:** Middleware global para exceções com responses padronizados (400, 401, 403, 404, 500).
- **Paginação:** Todos os retornos de lista devem conter paginação (`items`, `page`, `pageSize`, `totalItems`, `totalPages`).
- **EF Core e Banco de Dados:** Modelar as foreign keys corretamente, configurar índices úteis (ex: `User.Email` único, index em `Ticket.Status`), salvar tudo em formato compatível com UTC.
- **Transações:** Ações que alteram múltiplas tabelas (ex: transição de status) devem ser transacionais no EF Core.
- **Segurança:** Nunca concatenar SQL (usar EF parametrizado), não expor secrets no código (usar user secrets ou env vars).
- **Seed:** Criar usuários (Admin, Attendant, User) e chamados fictícios no `AppDbContext` para testar o MVP logo de cara.
- **Swagger:** Configurar o JWT Bearer Auth no Swagger para facilitar os testes.
- **Fora do escopo (MVP):** Refresh token, Login social, IA, Notificações, Arquiteturas distribuídas ou Redis.

---

## 11. ORDEM DE IMPLEMENTAÇÃO E CRITÉRIO DE ACEITAÇÃO
1. **Infraestrutura:** EF Core, PostgreSQL, Entidades, DbContext, Migrations.
2. **Autenticação:** JWT, PasswordHash, Login.
3. **Chamados (Base):** CRUD, Filtros, Comentários, Histórico.
4. **Workflow:** Transições de Status e Validações do Service Layer.
5. **Testes e Homologação:** Gerenciamento de testes e regras de aprovação/reprovação.
6. **Dashboard e Users:** Estatísticas e ADM.
7. **Seed e QA:** População de dados base e compilação do fluxo total.

**Critério Final:** O back-end só será considerado concluído quando todo o fluxo da criação do chamado até a sua homologação e resolução puder ser executado e validado via Swagger, com o banco real respondendo às regras e permissões.

*(Consulte a especificação completa para detalhes finos e validações)*.
