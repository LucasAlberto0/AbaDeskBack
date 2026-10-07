# AbaDesk - API RESTful (Backend)

O AbaDesk é uma plataforma avançada de chamados (Help Desk / Service Desk) projetada para controlar todo o ciclo de vida de uma solicitação, desde a abertura pelo usuário até a execução de testes de qualidade, resolução e homologação final. Este repositório contém todo o motor lógico (API Backend) que sustenta o sistema.

## Visão Geral do Projeto

A API do AbaDesk atua como a central inteligente de regras de negócio, mantendo um controle rigoroso sobre o fluxo de trabalho dos chamados e isolando responsabilidades através de níveis hierárquicos de acesso. O sistema assegura que cada solicitação passe por fases estritamente delimitadas, eliminando gargalos processuais e fornecendo uma trilha de auditoria completa em cada etapa.

## Tecnologias e Stack

O backend foi arquitetado com base em boas práticas e padrões consolidados, utilizando o seguinte ecossistema tecnológico:

- .NET 9.0: Framework principal e motor de alta performance.
- C#: Linguagem base para toda a arquitetura.
- Entity Framework Core: ORM (Object-Relational Mapper) robusto para persistência e abstração de banco de dados.
- PostgreSQL: Banco de dados relacional para armazenamento confiável e estruturado.
- ASP.NET Core Identity / JWT: Geração e validação de Json Web Tokens para sessões seguras e stateless, aliados a um sistema de hash forte para senhas.
- Swagger / OpenAPI: Documentação viva e interativa dos endpoints.

## Domínios e Funcionalidades

- Autenticação e Autorização baseada em Roles: Níveis de acesso bem definidos (Administrador, Suporte/Atendente e Usuário Final).
- Gestão Integral de Chamados (Tickets): Motor de CRUD de chamados com capacidade de paginação, busca avançada e manipulação de estado.
- Motor de Workflow Rígido: Transições engessadas que evitam saltos indevidos (Novo -> Em Análise -> Em Desenvolvimento -> Em Teste -> Aguardando Homologação -> Resolvido).
- Controle de Qualidade (QA): Funcionalidade que atrela Casos de Teste (Test Cases) a chamados específicos, permitindo que a equipe de QA avalie critérios técnicos antes do repasse ao cliente.
- Trilha de Interações (Timeline): Linha do tempo imutável que registra qualquer comentário, aprovação, rejeição ou alteração de status dentro de um chamado.
- Dados de Dashboard Dinâmicos: Geração de estatísticas consolidadas (taxa de resolução, pendências da conta e volumes de atividade).

## Instruções de Instalação e Execução

### Pré-requisitos
- .NET 9.0 SDK instalado.
- Instância do PostgreSQL rodando localmente ou de forma remota.

### Inicializando a Aplicação

1. Clone o repositório em seu ambiente local.
2. Navegue até o diretório raiz e edite o arquivo `appsettings.Development.json` (ou `appsettings.json`). Insira as credenciais do seu PostgreSQL na seção `DefaultConnection`.
3. Execute as migrations para que o Entity Framework construa o banco de dados:
   ```bash
   dotnet ef database update
   ```
4. Suba o servidor:
   ```bash
   dotnet run
   ```

Na primeira inicialização, um Seeder automático criará tabelas fundamentais e inserirá os primeiros registros no banco de dados para facilitar a experiência e os testes iniciais.

### Credenciais Administrativas Padrão (Seeder)
- Email: admin@abadesk.local
- Senha: senha123
