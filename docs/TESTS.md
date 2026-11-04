# Testes Automatizados no Ambiente Dev — AbaDesk Backend

Este documento orienta sobre a execução, manutenção e criação de novos testes automatizados na branch `dev` do backend.

---

## 1. Estrutura da Solução de Testes

A solução conta com dois projetos de teste automatizados sob a pasta `tests/`:

- **`AbaDesk.UnitTests`:**
  - Testa as regras de negócio isoladas na `TicketService` usando mocks de dependências com **Moq** e asserções com **FluentAssertions**.
  - Execução instantânea em milissegundos sem dependência de banco de dados ou rede.
- **`AbaDesk.IntegrationTests`:**
  - Testa a aplicação de ponta a ponta em memória usando `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) e o provedor `Microsoft.EntityFrameworkCore.InMemory`.
  - Valida contratos HTTP reais, autorização baseada em tokens JWT, respostas de status codes e proteção RBAC de endpoints.

---

## 2. Como Executar os Testes no Dev

No terminal, na pasta raiz do backend (`AbaDeskBack/`):

```bash
# Executa todos os testes da solução
dotnet test AbaDesk.sln

# Executa com saída detalhada
dotnet test AbaDesk.sln --verbosity normal

# Executa apenas a suíte unitária
dotnet test tests/AbaDesk.UnitTests

# Executa apenas a suíte de integração
dotnet test tests/AbaDesk.IntegrationTests
```

---

## 3. Cobertura de Cenários no Dev

### 3.1 Testes Unitários (`TicketServiceTests.cs`)
- Criação de chamado com protocolo sequencial.
- Validação de obrigatoriedade de título e descrição.
- Validação de unidade da empresa.
- Transição de status para `InAnalysis`.
- Regra de bloqueio impedindo usuário comum (`User`) de assumir análise de chamado.
- Resolução de chamado com preenchimento da solução técnica e bloqueio quando solução estiver em branco.

### 3.2 Testes de Integração
- **Autenticação (`AuthEndpointsTests.cs`):** Login com sucesso (`200 OK`) gerando JWT e rejeição de credenciais inválidas (`401 Unauthorized`).
- **Autorização RBAC (`AuthorizationTests.cs`):** Bloqueio de rotas sem token (`401`) e bloqueio de usuários comuns em rotas exclusivas do suporte (`403 Forbidden`).
- **Contratos de Chamados (`TicketEndpointsTests.cs`):** Criação com `201 Created`, validação de campos obrigatórios (`400 Bad Request`), consulta por ID e download binário do anexo do banco.

