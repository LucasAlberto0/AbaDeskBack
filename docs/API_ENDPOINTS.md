# Catálogo de Endpoints REST e SignalR (Ambiente Dev) — AbaDesk

Esta documentação provê a referência de todos os endpoints HTTP e eventos do WebSocket disponíveis na branch `dev` do **AbaDesk Backend**.

---

## 📌 Sumário

1. [Autenticação (`/api/auth`)](#1-autenticação-apiauth)
2. [Chamados (`/api/tickets`)](#2-chamados-apitickets)
3. [Comentários e Histórico](#3-comentários-e-histórico)
4. [Ações do Fluxo de Trabalho](#4-ações-do-fluxo-de-trabalho)
5. [Notificações (`/api/notifications`)](#5-notificações-apinotifications)
6. [Dashboard (`/api/dashboard`)](#6-dashboard-apidashboard)
7. [Administração de Usuários (`/api/users`)](#7-administração-de-usuários-apiusers)
8. [Inteligência Artificial (`/api/ai`)](#8-inteligência-artificial-apiai)
9. [SignalR WebSocket Hub (`/hubs/ticket`)](#9-signalr-websocket-hub-hubsticket)

---

## 1. Autenticação (`/api/auth`)

- `POST /api/auth/login`: Autentica usuário com e-mail e senha. Retorna token JWT e dados do usuário. (Público)
- `POST /api/auth/register`: Registra um novo colaborador corporativo. (Público)
- `GET /api/auth/me`: Retorna os dados do colaborador autenticado com base no token JWT. (Autenticado)
- `PATCH /api/auth/profile`: Atualiza dados básicos e regenera o token de sessão. (Autenticado)

---

## 2. Chamados (`/api/tickets`)

- `GET /api/tickets`: Lista chamados paginados com múltiplos filtros (`status`, `priority`, `category`, `systemName`, `department`, `assignedToUserId`, `createdByUserId`, `search`, `page`, `pageSize`).
- `GET /api/tickets/my`: Lista apenas os chamados associados ao usuário autenticado (criados por ele ou atribuídos a ele).
- `GET /api/tickets/{id}`: Detalhes completos do chamado com comentários e histórico.
- `POST /api/tickets`: Criação de novo chamado com suporte a dados de anexo opcionais.
- `DELETE /api/tickets/{id}`: Exclusão física de chamado. (Admin ou criador)
- `GET /api/tickets/{id}/attachment`: Retorna o arquivo binário da evidência direto do banco de dados (`AllowAnonymous`).

---

## 3. Comentários e Histórico

- `POST /api/tickets/{id}/comments`: Adiciona um comentário e dispara evento no SignalR.
- `GET /api/tickets/{id}/comments`: Recupera toda a conversa do chamado.
- `GET /api/tickets/{id}/history`: Recupera a trilha de auditoria completa (quem alterou, de qual status para qual status e quando).

---

## 4. Ações do Fluxo de Trabalho

| Rota | Permissão Mínima | Ação Realizada |
| :--- | :--- | :--- |
| `POST /api/tickets/{id}/analyze` | Attendant, Admin | Transita de `Open` para `InAnalysis`. |
| `POST /api/tickets/{id}/assign` | Attendant, Admin | Atribui a responsabilidade a um usuário específico. |
| `POST /api/tickets/{id}/start-progress`| Attendant, Admin | Transita para `InProgress` e vincula o técnico responsável. |
| `POST /api/tickets/{id}/wait-for-user` | Attendant, Admin | Transita para `WaitingUser` aguardando retorno do solicitante. |
| `POST /api/tickets/{id}/resolve` | Attendant, Admin | Transita para `Resolved`. Exige `{ "solution": "texto" }`. |
| `POST /api/tickets/{id}/reopen` | Solicitante, Admin | Reabre um chamado resolvido voltando para `Open`. Exige `{ "justification": "texto" }`. |

---

## 5. Notificações (`/api/notifications`)

- `GET /api/notifications`: Lista as notificações recentes do usuário.
- `DELETE /api/notifications/{id}`: Remove uma notificação específica.
- `DELETE /api/notifications`: Limpa todas as notificações do usuário logado.

---

## 6. Dashboard (`/api/dashboard`)

- `GET /api/dashboard/summary`: Retorna totais de chamados abertos, em análise, em atendimento, aguardando e resolvidos, filtrados automaticamente pelo perfil do usuário.

---

## 7. Administração de Usuários (`/api/users`)

*Apenas colaboradores com perfil `Admin`:*
- `GET /api/users`: Lista de colaboradores cadastrados com paginação e busca.
- `GET /api/users/{id}`: Detalhes de um colaborador específico.
- `POST /api/users`: Cadastro manual de usuário com definição de papel inicial.
- `PUT /api/users/{id}`: Atualização cadastral e de privilégios (*User, Attendant, Admin*).
- `DELETE /api/users/{id}`: Inativação da conta do usuário.

---

## 8. Inteligência Artificial (`/api/ai`)

- `POST /api/ai/chat`:
  - Envia mensagens para o agente de IA corporativo (ABA Assist) e retorna a resposta formatada em Markdown com suporte a histórico por `SessionId`.

---

## 9. SignalR WebSocket Hub (`/hubs/ticket`)

- **Conexão:** `ws://localhost:5017/hubs/ticket?access_token=<jwt>`
- **Eventos:**
  - `ReceiveNotification`: Notifica novos alertas ao usuário.
  - `ReceiveComment`: Transmite novo comentário em tempo real.
  - `TicketUpdated`: Notifica alterações de status para atualização de quadros Kanban.

