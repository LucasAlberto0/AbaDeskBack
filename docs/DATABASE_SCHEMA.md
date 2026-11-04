# Esquema do Banco de Dados (Ambiente Dev) — AbaDesk

Este documento detalha o modelo relacional, tabelas, colunas, índices e migrações do banco de dados **PostgreSQL** utilizado no ambiente de desenvolvimento do **AbaDesk**.

---

## 1. Diagrama Entidade-Relacionamento (ERD)

```mermaid
erDiagram
    USERS ||--o{ TICKETS_CREATED : "cria"
    USERS ||--o{ TICKETS_ASSIGNED : "atendido por"
    USERS ||--o{ TICKET_COMMENTS : "comenta"
    USERS ||--o{ TICKET_HISTORIES : "registra ação"
    USERS ||--o{ NOTIFICATIONS : "recebe"

    TICKETS ||--o{ TICKET_COMMENTS : "possui"
    TICKETS ||--o{ TICKET_HISTORIES : "trilha"
    TICKETS ||--o{ NOTIFICATIONS : "origina"

    USERS {
        uuid Id PK
        string Name
        string Email UK
        string PasswordHash
        int Role
        string CompanyUnit
        string JobTitle
        boolean IsActive
        timestamptz CreatedAt
        timestamptz UpdatedAt
        timestamptz LastLoginAt
    }

    TICKETS {
        uuid Id PK
        string ProtocolNumber
        string Title
        string Description
        string CompanyUnit
        string Department
        string SystemName
        string AttachmentUrl
        bytea AttachmentData
        string AttachmentContentType
        string AttachmentFileName
        string Solution
        int Category
        int Priority
        int Status
        uuid CreatedByUserId FK
        uuid AssignedToUserId FK
        timestamptz CreatedAt
        timestamptz UpdatedAt
        timestamptz ResolvedAt
    }

    TICKET_COMMENTS {
        uuid Id PK
        uuid TicketId FK
        uuid UserId FK
        string Content
        timestamptz CreatedAt
        timestamptz UpdatedAt
    }

    TICKET_HISTORIES {
        uuid Id PK
        uuid TicketId FK
        uuid UserId FK
        string Action
        int FromStatus
        int ToStatus
        string Description
        timestamptz CreatedAt
    }

    NOTIFICATIONS {
        uuid Id PK
        uuid UserId FK
        uuid TicketId FK
        string Title
        string Message
        int Type
        boolean IsRead
        timestamptz CreatedAt
    }
```

---

## 2. Tabelas Mapeadas no EF Core

Configuradas em [`AppDbContext.cs`](../Data/AppDbContext.cs):

### `Users`
- `Id`: `uuid` (Primary Key).
- `Name`: `text` (Nome do colaborador).
- `Email`: `text` (Índice Único `IX_Users_Email`).
- `PasswordHash`: `text` (Hash PBKDF2).
- `Role`: `integer` (`0: User`, `1: Attendant`, `2: Admin`).
- `CompanyUnit`: `text` (Unidade da empresa).
- `JobTitle`: `text` (Cargo funcional).
- `IsActive`: `boolean` (Permissão de login).
- `CreatedAt`, `UpdatedAt`, `LastLoginAt`: `timestamptz`.

### `Tickets`
- `Id`: `uuid` (Primary Key).
- `ProtocolNumber`: `text` (Índice `IX_Tickets_ProtocolNumber`).
- `Title`, `Description`, `CompanyUnit`, `Department`, `SystemName`: `text`.
- `AttachmentData`: `bytea` (Bytes binários da imagem de evidência).
- `AttachmentContentType`: `text` (MIME type).
- `AttachmentFileName`: `text` (Nome original do arquivo).
- `Solution`: `text` (Solução técnica registrada no encerramento).
- `Category`: `integer` (Bug, Improvement, NewFeature, Question, Access).
- `Priority`: `integer` (Low, Medium, High, Critical).
- `Status`: `integer` (Open, InAnalysis, InProgress, WaitingUser, Resolved).
- `CreatedByUserId`: `uuid` (FK `Users.Id`, `DeleteBehavior.Restrict`).
- `AssignedToUserId`: `uuid` (FK `Users.Id`, `DeleteBehavior.SetNull`).

### `TicketHistories`
- `Id`: `uuid` (Primary Key).
- `TicketId`: `uuid` (FK `Tickets.Id`, `DeleteBehavior.Cascade`).
- `UserId`: `uuid` (FK `Users.Id`, `DeleteBehavior.Restrict`).
- `Action`: `text` (Descrição da ação executada).
- `FromStatus`, `ToStatus`: `integer` (Status anterior e novo).
- `Description`: `text` (Detalhe da mudança).

### `TicketComments`
- `Id`: `uuid` (Primary Key).
- `TicketId`: `uuid` (FK `Tickets.Id`, `DeleteBehavior.Cascade`).
- `UserId`: `uuid` (FK `Users.Id`, `DeleteBehavior.Restrict`).
- `Content`: `text` (Mensagem).

### `Notifications`
- `Id`: `uuid` (Primary Key).
- `UserId`: `uuid` (FK `Users.Id`, `DeleteBehavior.Cascade`).
- `TicketId`: `uuid` (FK `Tickets.Id`, `DeleteBehavior.Cascade`).
- `Title`, `Message`: `text`.
- `Type`: `integer` (StatusChanged, NewComment, etc.).
- `IsRead`: `boolean` (Índice `IX_Notifications_IsRead`).

---

## 3. Comandos Úteis do Entity Framework Core

```bash
# Aplica todas as migrações pendentes no PostgreSQL local
dotnet ef database update

# Gera uma nova migração ao alterar classes da pasta Entities
dotnet ef migrations add NomeDaAlteracao

# Remove a última migração se ainda não aplicada
dotnet ef migrations remove
```

