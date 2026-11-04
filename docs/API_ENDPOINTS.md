# Catálogo de Endpoints da API — AbaDesk

Esta documentação provê a referência completa de todos os endpoints HTTP RESTful expostos pela API do **AbaDesk**, detalhando métodos, rotas, níveis de autorização, parâmetros e formatos de resposta.

---

## 📌 Sumário

1. [Autenticação e Perfil (`/api/auth`)](#1-autenticação-e-perfil-apiauth)
2. [Gestão de Chamados (`/api/tickets`)](#2-gestão-de-chamados-apitickets)
3. [Comentários e Histórico (`/api/tickets/{id}/...`)](#3-comentários-e-histórico)
4. [Ações do Workflow de Chamados](#4-ações-do-workflow-de-chamados)
5. [Notificações (`/api/notifications`)](#5-notificações-apinotifications)
6. [Dashboard e Métricas (`/api/dashboard`)](#6-dashboard-e-métricas-apidashboard)
7. [Administração de Usuários (`/api/users`)](#7-administração-de-usuários-apiusers)
8. [Inteligência Artificial (`/api/ai`)](#8-inteligência-artificial-apiai)
9. [Hub em Tempo Real (`/hubs/ticket`)](#9-hub-em-tempo-real-hubsticket)

---

## 1. Autenticação e Perfil (`/api/auth`)

### 1.1 `POST /api/auth/login`
- **Descrição:** Autentica um usuário com e-mail corporativo e senha.
- **Autorização:** Pública (Anônimo).
- **Corpo da Requisição (JSON):**
  ```json
  {
    "email": "usuario@abadesk.local",
    "password": "sua_senha"
  }
  ```
- **Respostas:**
  - `200 OK`: Retorna `accessToken`, `expiresAt` e o objeto `user` (sem o hash de senha).
  - `400 Bad Request`: Payload inválido.
  - `401 Unauthorized`: Credenciais incorretas ou conta inativa.

### 1.2 `POST /api/auth/register`
- **Descrição:** Registra um novo usuário no sistema e retorna um token de acesso imediato.
- **Autorização:** Pública (Anônimo).
- **Corpo da Requisição (JSON):**
  ```json
  {
    "name": "Nome Completo",
    "email": "colaborador@empresa.com.br",
    "password": "senha_segura",
    "role": "User",
    "jobTitle": "Analista de Operações",
    "companyUnit": "ABA Infra"
  }
  ```
- **Respostas:**
  - `200 OK`: Conta criada com sucesso e token JWT emitido.
  - `400 Bad Request`: E-mail já cadastrado ou validação pendente.

### 1.3 `GET /api/auth/me`
- **Descrição:** Retorna os dados essenciais do usuário autenticado no token JWT enviado.
- **Autorização:** Requer Autenticação (`Bearer <token>`).
- **Respostas:**
  - `200 OK`: `{ "id": "guid", "name": "...", "email": "...", "role": "...", "isActive": true }`.
  - `401 Unauthorized`: Token ausente, inválido ou expirado.

### 1.4 `PATCH /api/auth/profile`
- **Descrição:** Atualiza os dados cadastrais (nome, empresa) do usuário logado e regenera o token JWT.
- **Autorização:** Requer Autenticação (`Bearer <token>`).

---

## 2. Gestão de Chamados (`/api/tickets`)

### 2.1 `GET /api/tickets`
- **Descrição:** Lista chamados com filtros avançados e paginação. Administradores e Atendentes visualizam todos os chamados; Usuários comuns visualizam apenas seus próprios chamados.
- **Autorização:** Requer Autenticação.
- **Parâmetros de Consulta (Query Strings):**
  - `status`: Filtro por enum de status (*Open, InAnalysis, InProgress, WaitingUser, Resolved*).
  - `priority`: Filtro por prioridade (*Low, Medium, High, Critical*).
  - `category`: Filtro por categoria (*Bug, Improvement, NewFeature, Question, Access*).
  - `systemName`: Nome do sistema impactado.
  - `department`: Departamento solicitante.
  - `assignedToUserId`: Filtrar por responsável técnico (`Guid`).
  - `createdByUserId`: Filtrar por autor do chamado (`Guid`).
  - `search`: Busca textual simultânea em título, protocolo e descrição.
  - `page`: Número da página (Padrão: `1`).
  - `pageSize`: Quantidade de registros por página (Padrão: `20`).
- **Respostas:**
  - `200 OK`: Retorna objeto paginado `{ "items": [...], "totalCount": 42, "page": 1, "pageSize": 20, "totalPages": 3 }`.

### 2.2 `GET /api/tickets/my`
- **Descrição:** Atalho para recuperar apenas chamados do usuário logado (criados por ele ou atribuídos a ele).
- **Autorização:** Requer Autenticação.

### 2.3 `GET /api/tickets/{id}`
- **Descrição:** Recupera os detalhes completos de um chamado, incluindo histórico de auditoria e comentários.
- **Autorização:** Requer Autenticação (usuários comuns só podem acessar chamados próprios).
- **Respostas:**
  - `200 OK`: Objeto detalhado do chamado.
  - `403 Forbidden`: Usuário sem permissão para acessar este chamado.
  - `404 Not Found`: Chamado inexistente.

### 2.4 `POST /api/tickets`
- **Descrição:** Criação de um novo chamado no sistema.
- **Autorização:** Requer Autenticação.
- **Corpo da Requisição (JSON):**
  ```json
  {
    "title": "Erro ao emitir nota fiscal",
    "description": "O sistema trava ao clicar em emitir no pedido 1042.",
    "companyUnit": "ABA Infra",
    "department": "Faturamento",
    "systemName": "ERP Corporativo",
    "category": "Bug",
    "priority": "High",
    "attachmentData": "base64_opcional",
    "attachmentFileName": "evidencia.png",
    "attachmentContentType": "image/png"
  }
  ```
- **Respostas:**
  - `201 Created`: Chamado criado com número de protocolo gerado e `Location` no header.
  - `400 Bad Request`: Dados obrigatórios ausentes.

### 2.5 `DELETE /api/tickets/{id}`
- **Descrição:** Exclui permanentemente um chamado e seus anexos.
- **Autorização:** Administradores ou criador do chamado (enquanto no status inicial).
- **Respostas:**
  - `204 NoContent`: Exclusão concluída.
  - `403 Forbidden`: Sem privilégios de exclusão.
  - `404 Not Found`: Não localizado.

### 2.6 `GET /api/tickets/{id}/attachment`
- **Descrição:** Retorna o fluxo de bytes do arquivo de evidência anexado ao chamado diretamente do banco de dados.
- **Autorização:** Pública / Acesso direto (`AllowAnonymous`).
- **Respostas:**
  - `200 OK`: Stream binário do arquivo com headers `Content-Type` e `Content-Disposition`.
  - `404 Not Found`: Chamado sem anexo cadastrado.

---

## 3. Comentários e Histórico

### 3.1 `POST /api/tickets/{id}/comments`
- **Descrição:** Adiciona uma mensagem na thread de comentários do chamado e dispara evento em tempo real no SignalR.
- **Autorização:** Requer Autenticação.
- **Corpo:** `{ "content": "Adicionei a evidência solicitada no chamado." }`
- **Respostas:** `200 OK` com o comentário criado.

### 3.2 `GET /api/tickets/{id}/comments`
- **Descrição:** Lista todos os comentários do chamado em ordem cronológica.
- **Autorização:** Requer Autenticação.

### 3.3 `GET /api/tickets/{id}/history`
- **Descrição:** Retorna a trilha de auditoria completa (quem alterou, quando alterou, transições de status e ações executadas).
- **Autorização:** Requer Autenticação.

---

## 4. Ações do Workflow de Chamados

Todas as ações de workflow são executadas via requisições `POST` semânticas:

| Endpoint | Permissão | Ação e Validações |
| :--- | :--- | :--- |
| `POST /api/tickets/{id}/analyze` | Admin, Attendant | Transita de `Open` para `InAnalysis`. |
| `POST /api/tickets/{id}/assign` | Admin, Attendant | Atribui a responsabilidade técnica do chamado a um usuário (`{ "userId": "guid" }`). |
| `POST /api/tickets/{id}/start-progress` | Admin, Attendant | Transita para `InProgress` e vincula o técnico responsável. |
| `POST /api/tickets/{id}/wait-for-user` | Admin, Attendant | Transita para `WaitingUser` aguardando retorno externo. |
| `POST /api/tickets/{id}/resolve` | Admin, Attendant | Transita para `Resolved`. Exige payload com a solução técnica: `{ "solution": "Descrição técnica da correção" }`. |
| `POST /api/tickets/{id}/reopen` | Solicitante, Admin | Reabre um chamado `Resolved` voltando para `Open`. Exige justificativa: `{ "justification": "O problema voltou a ocorrer..." }`. |

---

## 5. Notificações (`/api/notifications`)

- `GET /api/notifications`: Retorna a lista de notificações recentes do usuário logado.
- `DELETE /api/notifications/{id}`: Exclui uma notificação específica.
- `DELETE /api/notifications`: Limpa todo o histórico de notificações do usuário.

---

## 6. Dashboard e Métricas (`/api/dashboard`)

- `GET /api/dashboard/summary`:
  - Retorna totalizadores agrupados por status, chamados recentes e métricas de desempenho.
  - **Filtro Automático por Perfil:** Solicitantes recebem dados apenas das suas solicitações; Atendentes e Admins recebem consolidados corporativos.

---

## 7. Administração de Usuários (`/api/users`)

*Restrito exclusivamente a colaboradores com perfil `Admin` (`[Authorize(Roles = "Admin")]`)*:

- `GET /api/users`: Lista usuários paginados com filtro por nome e e-mail.
- `GET /api/users/{id}`: Detalhes de um usuário específico.
- `POST /api/users`: Criação manual de novo usuário com definição de papel inicial.
- `PUT /api/users/{id}`: Edição de dados (nome, e-mail, cargo, empresa, role).
- `DELETE /api/users/{id}`: Alterna o status do usuário para inativo, bloqueando acessos futuros.

---

## 8. Inteligência Artificial (`/api/ai`)

### `POST /api/ai/chat`
- **Descrição:** Envia mensagens para o agente de IA corporativo (ABA Assist) e recebe a resposta formatada.
- **Autorização:** Requer Autenticação.
- **Corpo:**
  ```json
  {
    "message": "Como abrir um chamado de suporte para o sistema financeiro?",
    "sessionId": "guid_ou_string_opcional_para_manter_contexto"
  }
  ```
- **Respostas:**
  - `200 OK`: `{ "response": "Texto formatado em Markdown...", "sessionId": "id_da_sessao" }`.
  - `400 Bad Request`: Mensagem vazia ou excedendo 4.000 caracteres.
  - `504 Gateway Timeout`: Tempo limite esgotado na comunicação com o LLM.

---

## 9. Hub em Tempo Real (`/hubs/ticket`)

Conexão WebSocket com autenticação por token query param (`?access_token=<jwt>`):

- **Grupos:** O cliente pode ingressar no grupo de um chamado específico para escutar atualizações em tempo real (`Ticket_{id}`).
- **Eventos Emitidos pelo Servidor:**
  - `ReceiveNotification`: Notifica novos alertas ao usuário conectado.
  - `ReceiveComment`: Transmite novo comentário inserido no chamado.
  - `TicketUpdated`: Notifica alterações de status para atualização de quadros Kanban.

