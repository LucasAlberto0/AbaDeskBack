# AbaDesk - Backend

Backend do projeto AbaDesk, desenvolvido em .NET 9.0 com Entity Framework Core e PostgreSQL.

## Funcionalidades Implementadas
- **Autenticação:** JWT com hash de senhas (Argon2/Identity).
- **Usuários:** CRUD completo para administração e permissionamento por `Role` (Admin, Attendant, User).
- **Chamados (Tickets):** Abertura, listagem paginada, filtros avançados e timeline/histórico de transições.
- **Workflow Rígido:** Transições controladas (`NEW -> ANALYZING -> IN_DEVELOPMENT -> IN_TEST -> WAITING_HOMOLOGATION -> RESOLVED`).
- **Comentários:** Atendentes e usuários podem trocar mensagens no chamado.
- **Testes (QA):** Registro de casos de teste atrelados a um chamado e execução (aprovação/reprovação).
- **Homologação:** O usuário final valida a entrega para aprovar (RESOLVED) ou reprovar (volta para IN_DEVELOPMENT).
- **Dashboard:** Métricas gerais e alertas de pendências para o usuário logado (`PendingMyAction`).

## Como rodar

1. Certifique-se de que o PostgreSQL está rodando.
2. Atualize a connection string em `appsettings.json` (ou `appsettings.Development.json`).
3. Execute as migrations:
   ```bash
   dotnet ef database update
   ```
4. Rode a aplicação (O seeder inicializará o banco automaticamente na primeira execução):
   ```bash
   dotnet run
   ```

**Credenciais Administrativas Padrão:**
- **Email:** admin@abadesk.local
- **Senha:** senha123
