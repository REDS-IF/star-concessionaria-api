# CONTEXTO E DIRETRIZES PARA AGENTES — STAR CONCESSIONÁRIA

> **ATENÇÃO OBRIGATÓRIA A TODO E QUALQUER AGENTE:**
> 1. **LEIA ESTE ARQUIVO ANTES DE INICIAR QUALQUER TAREFA.** Você **NÃO** precisa vasculhar ou ler a codebase inteira. Todas as decisões técnicas, estado atual, comandos essenciais e próximos passos estão sintetizados aqui.
> 2. **ATUALIZE ESTE ARQUIVO ANTES DE FINALIZAR SEU TURNO.** Todo agente que realizar alterações, implementar código, criar testes ou avançar fases do projeto **DEVE OBRIGATORIAMENTE** registrar o que fez na seção **[6. Histórico de Handoff entre Agentes](#6-histórico-de-handoff-entre-agentes)** e atualizar a seção **[1. Estado Atual do Projeto](#1-estado-atual-do-projeto)**.

---

## 1. ESTADO ATUAL DO PROJETO

- **Fase Atual do Plano:** **Fase 1: Preparação do Ambiente e Criação da Solução** (pronto para início da implementação da infraestrutura da solução).
- **Documento Mestre de Referência:** `PLANO_IMPLEMENTACAO.md` (contém as 15 seções detalhadas, endpoints, regras de negócio RN01 a RN08, modelagem relacional completa e backlog P0/P1/P2).
- **Especificação de Requisitos Original:** `Requisitos_FullCycle.pdf` (30 RFs, 8 RNs, 10 RNFs, 8 RVs).
- **Última Ação Realizada:** Elaboração e aprovação do Plano Completo de Implementação e criação deste guia de handoff `AGENT_CONTEXT.md`.
- **Próximo Passo Imediato (Para o Próximo Agente):**
  - Iniciar a **Fase 1 / TASK-01**: Estruturar a solução em 4 projetos (`StarConcessionaria.Api`, `StarConcessionaria.Application`, `StarConcessionaria.Domain`, `StarConcessionaria.Infrastructure`) e criar os projetos de teste (`StarConcessionaria.UnitTests`, `StarConcessionaria.IntegrationTests`), configurando `docker-compose.yml` com PostgreSQL.

---

## 2. DECISÕES TÉCNICAS E ARQUITETURAIS DEFINITIVAS

Nenhum agente deve alterar ou substituir essas tecnologias sem autorização explícita do usuário:

| Componente | Tecnologia Escolhida | Justificativa / Diretriz |
| :--- | :--- | :--- |
| **Linguagem & Runtime** | C# / .NET 10 | Versão estável do repositório (`net10.0`). |
| **Arquitetura** | Clean Architecture Pragmática (4 camadas) | `Domain` (regras puras) -> `Application` (casos de uso/DTOs) -> `Infrastructure` (EF Core/PostgreSQL) -> `Api` (Controllers/HTTP). Sem CQRS inflado/MediatR desnecessário. |
| **Banco de Dados** | PostgreSQL 16+ via Docker | Banco relacional com transações ACID e suporte a concorrência otimista e pessimista. |
| **ORM** | Entity Framework Core com Npgsql | Mapeamento via Fluent API (`IEntityTypeConfiguration<T>`). |
| **Autenticação & RBAC** | JWT Bearer + BCrypt | Perfis autorizados: `Administrador`, `Comercial`, `Logistica`. Clientes anônimos para consultas públicas e propostas. |
| **Validação** | FluentValidation | Regras de entrada desacopladas dos controllers. |
| **Documentação API** | OpenAPI / Swagger / Scalar | Endpoints documentados em `/openapi/v1.json` ou UI Swagger. |
| **Testes** | xUnit + FluentAssertions + Moq + Testcontainers | Testes de concorrência com PostgreSQL real e testes unitários no domínio. |

---

## 3. MAPA RÁPIDO DA ESTRUTURA DE PASTAS ESPERADA

```
StarConcessionaria/
├── PLANO_IMPLEMENTACAO.md       # O roteiro detalhado fase por fase
├── AGENT_CONTEXT.md             # ESTE ARQUIVO (Handoff e contexto imediato)
├── docker-compose.yml           # Serviço postgres:16-alpine (porta 5432)
├── Dockerfile                   # Build e deploy multi-stage
│
├── src/
│   ├── StarConcessionaria.Domain/          # Entidades (Veiculo, Unidade, Reserva, etc.), Enums, Regras
│   ├── StarConcessionaria.Application/     # Services, DTOs, Validators, Interfaces
│   ├── StarConcessionaria.Infrastructure/  # DbContext, Configurations, Migrations, Repositories
│   └── StarConcessionaria.Api/             # Controllers, Middlewares, Program.cs
│
└── tests/
    ├── StarConcessionaria.UnitTests/       # Testes unitários de regras de negócio
    └── StarConcessionaria.IntegrationTests/# Testes com Testcontainers PostgreSQL
```

---

## 4. COMANDOS ESSENCIAIS DE DESENVOLVIMENTO

Execute esses comandos diretamente no bash para validar seu trabalho sem precisar pesquisar:

```bash
# Compilar toda a solução
dotnet build

# Executar testes unitários e de integração
dotnet test

# Subir banco PostgreSQL local via Docker
docker compose up -d postgres

# Gerar nova migration no EF Core
dotnet ef migrations add <NomeDaMigration> --project src/StarConcessionaria.Infrastructure --startup-project src/StarConcessionaria.Api

# Aplicar migrations no banco
dotnet ef database update --project src/StarConcessionaria.Infrastructure --startup-project src/StarConcessionaria.Api
```

---

## 5. REGRAS CRÍTICAS DE NEGÓCIO (NUNCA VIOLAR)

- **RN01 & RN03 (Reservas):** Apenas veículos `Disponivel` podem ser reservados. Reservas simultâneas no mesmo veículo devem ser bloqueadas com concorrência segura (transação atômica / `FOR UPDATE` ou versão). Retornar `409 Conflict`.
- **RN02 & RN04 (Vendido / Test-Drive):** Veículos `Vendido` não recebem reservas nem agendamentos de test-drive.
- **RN06 (Compra Assistida):** A plataforma **não** fecha a compra integral do veículo nem transaciona cartão/pagamento integral. Ela registra o lead qualificado (proposta comercial) e direciona para atendimento comercial na unidade física.
- **RN07 (Permissões RBAC):** Apenas `Comercial` e `Logistica` (e `Administrador`) alteram dados de veículos, sempre gerando histórico na auditoria.

---

## 6. HISTÓRICO DE HANDOFF ENTRE AGENTES

> **Instruções para o Agente:**
> Sempre que você concluir um turno com alterações no repositório, adicione uma nova linha na tabela abaixo descrevendo resumidamente o que foi feito, arquivos alterados, testes executados e a instrução clara para o próximo agente.

| Data / Turno | Agente / Papel | Tarefas Realizadas | Arquivos Criados / Modificados | Testes Executados | Próximos Passos para o Próximo Agente |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **2026-10-15 (T1)** | Arquiteta de Software / Tech Lead | Limpeza de arquivos parciais de `src/`; elaboração do documento completo `PLANO_IMPLEMENTACAO.md` (15 seções, ER Mermaid, endpoints, backlog); criação do guia de handoff `AGENT_CONTEXT.md`. | `PLANO_IMPLEMENTACAO.md`, `AGENT_CONTEXT.md` | `dotnet build` validado sem erros; verificação de 100% de cobertura dos 30 RFs, 8 RNs, 10 RNFs e 8 RVs. | Iniciar a **Fase 1 / TASK-01**: Criar a solução .NET com a estrutura modular de projetos (`Api`, `Application`, `Domain`, `Infrastructure`), testes e `docker-compose.yml` para PostgreSQL. |

---

## 7. TEMPLATE PARA O PRÓXIMO AGENTE ADICIONAR NO HISTÓRICO

Copie e preencha este bloco ao atualizar a tabela na Seção 6:

```markdown
| AAAA-MM-DD (Tn) | Seu Papel/ID | Descrição das tarefas realizadas | Lista de arquivos tocados | Comandos de teste executados | Instrução objetiva para o próximo agente |
```
