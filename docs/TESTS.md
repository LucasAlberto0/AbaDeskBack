# Testes Automatizados — AbaDesk Backend

Este documento descreve a arquitetura, convenções, ferramentas e procedimentos de execução da suíte de testes automatizados do backend (**AbaDesk.UnitTests** e **AbaDesk.IntegrationTests**).

---

## 1. Estratégia de Testes

A suíte do backend foi estruturada seguindo a **Pirâmide de Testes**, com foco em alta confiabilidade, velocidade de execução e independência de infraestrutura física externa:

```text
       ┌────────────────────────┐
       │   Integration Tests    │  9 testes: Contratos HTTP, WebApplicationFactory,
       │ (AbaDesk.Integration)  │  JWT Bearer e banco InMemory isolado.
       └───────────┬────────────┘
                   │
       ┌───────────▼────────────┐
       │       Unit Tests       │  10 testes: Regras de negócio do TicketService,
       │   (AbaDesk.UnitTests)  │  validações de entrada e transições de status com Moq.
       └────────────────────────┘
```

---

## 2. Testes Unitários (`AbaDesk.UnitTests`)

Localizados em `tests/AbaDesk.UnitTests/Services/TicketServiceTests.cs`. Testam isoladamente as regras da camada de serviço utilizando **Moq** e **FluentAssertions**:

### 2.1 Cenários Cobertos:
1. `DeveCriarChamadoComSucessoQuandoDadosForemValidos`: Valida a geração do número de protocolo e a persistência inicial do chamado.
2. `DeveLancarExcecaoQuandoTituloForVazio`: Rejeita abertura de chamados sem título.
3. `DeveLancarExcecaoQuandoDescricaoForVazia`: Rejeita abertura sem descrição detalhada.
4. `DeveLancarExcecaoQuandoAnexoEstiverAusente`: Valida a obrigatoriedade de evidência quando a regra de negócio exigir.
5. `DeveLancarExcecaoQuandoUnidadeEmpresaForInvalida`: Valida a seleção obrigatória da unidade corporativa.
6. `DeveIniciarAnaliseComSucessoQuandoChamadoEstiverAberto`: Testa a transição de estado de `Open` para `InAnalysis`.
7. `DeveLancarExcecaoAoIniciarAnaliseSeChamadoNaoEstiverAberto`: Impede análise em chamados já encerrados ou em outro status incompatível.
8. `DeveResolverChamadoComSucessoQuandoSolucaoForInformada`: Valida o encerramento do chamado com preenchimento da solução técnica.
9. `DeveLancarExcecaoAoResolverChamadoSemSolucao`: Rejeita resolução sem justificativa técnica.
10. `DeveImpedirUsuarioComumDeIniciarAnaliseDeChamado`: Garante que solicitantes comuns (`User`) não possam assumir tarefas restritas à equipe de suporte.

---

## 3. Testes de Integração (`AbaDesk.IntegrationTests`)

Localizados em `tests/AbaDesk.IntegrationTests/`. Utilizam o `WebApplicationFactory<Program>` para subir um servidor web in-memory real e banco `Microsoft.EntityFrameworkCore.InMemory`:

### 3.1 Autenticação (`Auth/AuthEndpointsTests.cs`)
- `DeveAutenticarComSucessoQuandoCredenciaisForemValidas`: Executa `POST /api/auth/login`, validando o retorno `200 OK` e a presença do token JWT.
- `DeveRetornarUnauthorizedQuandoCredenciaisForemInvalidas`: Valida retorno `401 Unauthorized` para senha incorreta.

### 3.2 Autorização e Perfis RBAC (`Permissoes/AuthorizationTests.cs`)
- `DeveRetornarUnauthorizedAoAcessarEndpointProtegidoSemToken`: Garante retorno `401 Unauthorized` para chamadas sem cabeçalho `Authorization`.
- `DeveRetornarForbiddenQuandoUsuarioComumAcessarEndpointExclusivoDoSuporte`: Garante retorno `403 Forbidden` quando um `User` tenta acionar rotas de suporte.
- `DevePermitirAoSuporteIniciarAnaliseDeChamado`: Garante retorno `200 OK` quando um `Attendant` aciona a mesma rota.

### 3.3 Endpoints de Chamados (`Chamados/TicketEndpointsTests.cs`)
- `DeveCriarChamadoComSucessoRetornandoCreated`: Valida `POST /api/tickets` retornando `201 Created` e protocolo gerado.
- `DeveRetornarBadRequestQuandoDadosObrigatoriosForemInvalidos`: Valida `400 Bad Request` na ausência de campos obrigatórios.
- `DeveBuscarChamadoPorIdComSucessoRetornandoOk`: Valida `GET /api/tickets/{id}` recuperando os dados persistidos.
- `DeveRetornarAnexoBinarioDoChamadoCriadoDiretoDoBanco`: Valida o endpoint `GET /api/tickets/{id}/attachment` consumindo diretamente os bytes do banco.

---

## 4. Instruções de Execução dos Testes

No terminal, a partir da raiz do repositório backend (`AbaDeskBack/`):

```bash
# Executa todos os testes da solução (Unitários e de Integração)
dotnet test AbaDesk.sln

# Executa com relatório detalhado no console
dotnet test AbaDesk.sln --verbosity normal

# Executa apenas a suíte de testes unitários
dotnet test tests/AbaDesk.UnitTests

# Executa apenas a suíte de testes de integração
dotnet test tests/AbaDesk.IntegrationTests
```

Todos os testes rodam de forma autônoma sem requerer um banco PostgreSQL físico ativo na máquina.
