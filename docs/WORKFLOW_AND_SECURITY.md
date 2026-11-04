# Workflow e Segurança (Ambiente Dev) — AbaDesk

Este documento orienta os desenvolvedores sobre a máquina de estados, regras de permissões RBAC e práticas de segurança na branch `dev`.

---

## 1. Regras de Transição de Status dos Chamados

A máquina de estados implementada em `TicketService` impede alterações inválidas no fluxo de vida do chamado:

```text
       ┌──────────┐
       │   Open   │
       └────┬─────┘
            │
            ▼
     ┌──────────────┐
     │  InAnalysis  │
     └──────┬───────┘
            │
            ▼
     ┌──────────────┐      Informação Pendente      ┌─────────────┐
     │  InProgress  │ ────────────────────────────> │ WaitingUser │
     └──────┬───────┘ <──────────────────────────── └──────┬──────┘
            │               Retorno do Usuário             │
            ▼                                              │ Resolução
     ┌──────────────┐ <────────────────────────────────────┘
     │   Resolved   │
     └──────┬───────┘
            │ Reabertura com justificativa
            ▼
       ┌──────────┐
       │   Open   │
       └──────────┘
```

- **Resolução (`Resolved`):** O campo `Solution` é obrigatório. Requisições com solução em branco são rejeitadas com erro `400 Bad Request`.
- **Reabertura (`Open`):** Chamados finalizados só voltam a ficar abertos através do endpoint `/api/tickets/{id}/reopen`, que exige justificativa obrigatória e cria um novo registro de auditoria.

---

## 2. Matriz de Autorização RBAC

| Ação no Sistema | Usuário (`User`) | Atendente (`Attendant`) | Administrador (`Admin`) |
| :--- | :---: | :---: | :---: |
| Autenticar e buscar dados de perfil | Sim | Sim | Sim |
| Criar chamados | Sim | Sim | Sim |
| Visualizar chamados próprios | Sim | Sim | Sim |
| Visualizar chamados de terceiros | ❌ Não | Sim | Sim |
| Operar no quadro Kanban | ❌ Não | Sim | Sim |
| Iniciar análise de chamado | ❌ Não | Sim | Sim |
| Resolver chamado com solução | ❌ Não | Sim | Sim |
| Reabrir chamado resolvido | Sim (apenas próprios) | ❌ Não | Sim |
| Excluir chamado | Sim (se aberto por si) | ❌ Não | Sim |
| Gerenciar contas de usuários | ❌ Não | ❌ Não | Sim |

---

## 3. Segurança de Dados no Desenvolvimento Local

Para evitar incidentes de segurança:

1. **User Secrets no Desenvolvimento:**
   - Utilize a ferramenta de linha de comando para armazenar chaves locais sem versionar:
     ```bash
     dotnet user-secrets set "Jwt:Key" "uma-chave-longa-e-segura-com-pelo-menos-32-caracteres"
     dotnet user-secrets set "OpenAI:ApiKey" "sua-chave-de-teste-local"
     ```
2. **PostgreSQL Parametrizado:**
   - Todas as queries utilizam EF Core com parâmetros nativos, eliminando vulnerabilidades de SQL Injection.
3. **Hashing de Senha:**
   - O algoritmo PBKDF2 com salt aleatório garante proteção contra ataques de dicionário e tabelas rainbow.

