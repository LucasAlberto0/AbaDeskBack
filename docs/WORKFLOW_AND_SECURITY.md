# Workflow e Segurança — AbaDesk API

Este documento detalha a máquina de estados, regras de negócio de transição e os mecanismos de segurança (autenticação, autorização e proteção de dados sensíveis) implementados no **AbaDesk**.

---

## 1. Máquina de Estados dos Chamados

O status de um chamado não pode ser alterado de maneira arbitrária. A camada de serviços (`TicketService`) valida a legitimidade de cada mudança de estado.

### 1.1 Tabela de Transições Válidas

| Status Atual | Próximos Status Permitidos | Ação / Endpoint Correspondente | Permissão Mínima |
| :--- | :--- | :--- | :--- |
| **Open (Novo)** | `InAnalysis` | `POST /api/tickets/{id}/analyze` | Attendant, Admin |
| **Open (Novo)** | `InProgress` | `POST /api/tickets/{id}/start-progress` | Attendant, Admin |
| **InAnalysis (Em Análise)** | `InProgress` | `POST /api/tickets/{id}/start-progress` | Attendant, Admin |
| **InAnalysis (Em Análise)** | `WaitingUser` | `POST /api/tickets/{id}/wait-for-user` | Attendant, Admin |
| **InAnalysis (Em Análise)** | `Resolved` | `POST /api/tickets/{id}/resolve` (com solução) | Attendant, Admin |
| **InProgress (Em Atendimento)** | `WaitingUser` | `POST /api/tickets/{id}/wait-for-user` | Attendant, Admin |
| **InProgress (Em Atendimento)** | `Resolved` | `POST /api/tickets/{id}/resolve` (com solução) | Attendant, Admin |
| **WaitingUser (Aguardando Usuário)** | `InProgress` | `POST /api/tickets/{id}/start-progress` | Attendant, Admin |
| **WaitingUser (Aguardando Usuário)** | `Resolved` | `POST /api/tickets/{id}/resolve` (com solução) | Attendant, Admin |
| **Resolved (Resolvido)** | `Open` | `POST /api/tickets/{id}/reopen` (com justificativa) | Solicitante, Admin |

> **Imutabilidade e Regras Rígidas:**
> - Um chamado no status `Resolved` não aceita novos comentários técnicos nem alterações diretas de status — ele deve ser formalmente **reaberto** (`reopen`), gerando um novo registro de auditoria justificando a reabertura.
> - A transição para `Resolved` exige obrigatoriamente a descrição do campo `Solution`. Requisições com solução vazia são rejeitadas com `400 Bad Request`.

---

## 2. Matriz de Controle de Acesso Baseado em Papéis (RBAC)

O AbaDesk opera com três perfis formais de usuário (`Enums/Role.cs`):

| Funcionalidade / Recurso | Usuário (`User`) | Atendente (`Attendant`) | Administrador (`Admin`) |
| :--- | :---: | :---: | :---: |
| Autenticar e consultar próprio perfil | Sim | Sim | Sim |
| Abrir novo chamado | Sim | Sim | Sim |
| Visualizar chamados criados por si próprio | Sim | Sim | Sim |
| Visualizar chamados de outros colaboradores | ❌ Não | Sim | Sim |
| Visualizar quadro Kanban completo | ❌ Não | Sim | Sim |
| Assumir / Iniciar análise de chamado | ❌ Não | Sim | Sim |
| Atribuir chamado a outro técnico | ❌ Não | Sim | Sim |
| Resolver chamado com solução técnica | ❌ Não | Sim | Sim |
| Reabrir chamado resolvido | Sim (apenas próprios) | ❌ Não | Sim (qualquer) |
| Excluir chamado | Sim (se recém-criado) | ❌ Não | Sim |
| Listar usuários do sistema | ❌ Não | ❌ Não | Sim |
| Cadastrar, editar e inativar usuários | ❌ Não | ❌ Não | Sim |
| Promover ou alterar papéis de usuários | ❌ Não | ❌ Não | Sim |
| Acessar métricas globais corporativas no Dashboard | ❌ Não | Sim | Sim |
| Interagir com a assistente virtual ABA Assist | Sim | Sim | Sim |

---

## 3. Segurança e Criptografia de Credenciais

### 3.1 Hashing de Senhas com `IPasswordHasher<User>`
- Nenhuma senha é armazenada em texto plano.
- Utiliza a implementação oficial do ASP.NET Core Identity baseada em **PBKDF2** (*Password-Based Key Derivation Function 2*) com salt criptográfico aleatório gerado por hash e múltiplas iterações SHA-256.
- A verificação de senha compara os hashes sem risco de ataques de temporização (*timing attacks*).

### 3.2 Autenticação Stateless via JWT (JSON Web Tokens)
- **Algoritmo:** HMAC-SHA256 (`SecurityAlgorithms.HmacSha256Signature`).
- **Validade do Token:** 1.440 minutos (24 horas) por padrão, configurável via `Jwt:ExpirationMinutes`.
- **Zero Vazamento de Dados Sensíveis:** O payload do token carrega apenas claims públicas necessárias para autorização (`sub/NameIdentifier`, `email`, `role`, `name`), jamais incluindo senhas ou segredos de infraestrutura.

---

## 4. Política de Proteção a Dados Sensíveis

Para garantir conformidade de segurança e prevenção contra vazamento de segredos:

1. **Repositório Livre de Segredos:**
   - O arquivo `appsettings.json` versionado no controle de código contém apenas valores genéricos para desenvolvimento local.
   - Nenhuma chave de produção, credencial de banco PostgreSQL de nuvem ou chave da API OpenAI é mantida em arquivos versionados.
2. **Uso de Variáveis de Ambiente e User Secrets:**
   - Em desenvolvimento local: o time utiliza `dotnet user-secrets` para sobrescrever chaves de teste.
   - Em produção (Render / Cloud): os segredos são injetados diretamente como variáveis de ambiente pelo provedor de hospedagem:
     - `ConnectionStrings__DefaultConnection`
     - `Jwt__Key`
     - `Jwt__Issuer`
     - `Jwt__Audience`
     - `OpenAI__ApiKey`
     - `OpenAI__AgentId`
3. **Proteção contra Injeção de SQL:**
   - Todas as operações de banco utilizam o Entity Framework Core com consultas parametrizadas internamente pelo Npgsql.
   - Não há concatenação dinâmica de strings em comandos SQL em nenhuma parte da camada de serviços.
4. **Política CORS (Cross-Origin Resource Sharing):**
   - Configurada para autorizar requisições provenientes da aplicação cliente com credenciais e cabeçalhos autorizados.

