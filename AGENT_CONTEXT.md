# CONTEXTO E DIRETRIZES PARA AGENTES — STAR CONCESSIONÁRIA

> **ATENÇÃO OBRIGATÓRIA A TODO E QUALQUER AGENTE:**
> 1. **LEIA ESTE ARQUIVO ANTES DE INICIAR QUALQUER TAREFA.** Você **NÃO** precisa vasculhar ou ler a codebase inteira. Todas as decisões técnicas, estado atual, comandos essenciais e próximos passos estão sintetizados aqui.
> 2. **ATUALIZE ESTE ARQUIVO ANTES DE FINALIZAR SEU TURNO.** Todo agente que realizar alterações, implementar código, criar testes ou avançar fases do projeto **DEVE OBRIGATORIAMENTE** registrar o que fez na seção **[6. Histórico de Handoff entre Agentes](#6-histórico-de-handoff-entre-agentes)** e atualizar a seção **[1. Estado Atual do Projeto](#1-estado-atual-do-projeto)**.

---

## 1. ESTADO ATUAL DO PROJETO

- **Fase Atual do Plano:** **Fase 1: Preparação do Ambiente e Criação da Solução** (pronto para início da implementação da infraestrutura da solução).
- **Documento Mestre de Referência:** `PLANO_IMPLEMENTACAO.md` (contém as 15 seções detalhadas, endpoints, regras de negócio RN01 a RN08, modelagem relacional completa e backlog P0/P1/P2).
- **Especificação de Requisitos Original:** `Requisitos_FullCycle.pdf` (30 RFs, 8 RNs, 10 RNFs, 8 RVs).
- **Fonte de Verdade do Banco de Dados:**
  - `docs/database/star.sql` (script SQL original com 14 tabelas).
  - `docs/database/star_postgres.sql` (versão adaptada e pronta para PostgreSQL 16+ com tipos compatíveis e índices).
  - `docs/database/explicação.txt` (dicionário de dados e regras relacionais oficiais).
  - `docs/database/diagrama_star.pdf` (diagrama EER original).
- **Última Ação Realizada:** Incorporação do schema oficial do banco de dados (14 tabelas), criação do script PostgreSQL correspondente e atualização das seções 5 e 6 do plano.
- **Última Ação (T3):** TASK-01 concluída: solução com 4 projetos + 2 de testes, `Directory.Build.props` (TargetFramework central) e `docker-compose.yml`. **Atenção:** o SDK .NET 10 não está instalado (só 8 e 9), então por autorização do usuário o target está temporariamente em `net9.0` em `Directory.Build.props`; migrar para `net10.0` quando o SDK existir.
- **Próximo Passo Imediato (Para o Próximo Agente):**
  - Validar `docker compose up -d postgres` (não testado) e iniciar a **TASK-02**: `ApplicationDbContext`, Fluent API e migrations (adicionar pacotes EF Core/Npgsql).

---

## 2. DECISÕES TÉCNICAS E ARQUITETURAIS DEFINITIVAS

Nenhum agente deve alterar ou substituir essas tecnologias sem autorização explícita do usuário:

| Componente | Tecnologia Escolhida | Justificativa / Diretriz |
| :--- | :--- | :--- |
| **Linguagem & Runtime** | C# / .NET 10 | Versão estável do repositório (`net10.0`). |
| **Arquitetura** | Clean Architecture Pragmática (4 camadas) | `Domain` (entidades ricas) -> `Application` (casos de uso/DTOs) -> `Infrastructure` (EF Core/PostgreSQL) -> `Api` (Controllers/HTTP). Sem CQRS inflado/MediatR desnecessário. |
| **Banco de Dados** | PostgreSQL 16+ via Docker | Banco relacional oficial. Schema fixo em 14 tabelas (`docs/database/star_postgres.sql`). |
| **ORM** | Entity Framework Core com Npgsql | Mapeamento via Fluent API (`IEntityTypeConfiguration<T>`) respeitando as 14 tabelas e colunas com sufixos (`_loj`, `_vei`, `_cli`, etc.). |
| **Autenticação & RBAC** | JWT Bearer + BCrypt | Perfis na tabela `UsuarioSistema`: `comercial`, `logistica`, `administrador`. Clientes anônimos para consultas públicas e propostas. |
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
├── docs/database/               # FONTE DE VERDADE DO BANCO DE DADOS
│   ├── star.sql                 # Script SQL original (14 tabelas)
│   ├── star_postgres.sql        # Script adaptado para PostgreSQL 16+
│   ├── explicação.txt           # Dicionário de dados das tabelas
│   └── diagrama_star.pdf        # Diagrama EER oficial
│
├── src/
│   ├── StarConcessionaria.Domain/          # Entidades (Veiculo, Loja, Reserva, Agendamento, Peca, etc.)
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

- **RN01 & RN03 (Reservas):** Apenas veículos com `situacao_vei = 'Disponivel'` podem ser reservados. Reservas simultâneas no mesmo veículo devem ser bloqueadas com concorrência segura (transação atômica / `FOR UPDATE`). Retornar `409 Conflict`.
- **RN02 & RN04 (Vendido / Test-Drive):** Veículos com `situacao_vei = 'Vendido'` não recebem reservas nem agendamentos de test-drive.
- **RN06 (Compra Assistida):** A plataforma **não** fecha a compra integral do veículo nem transaciona cartão/pagamento integral. Ela registra o contato comercial (`tipo_con = 'InicioCompra'`) e direciona para atendimento comercial na loja física.
- **RN07 (Permissões RBAC):** Apenas `comercial` e `logistica` (e `administrador`) alteram dados de veículos, sempre gerando histórico na tabela `AlteracaoVeiculo`.

---

## 6. HISTÓRICO DE HANDOFF ENTRE AGENTES

> **Instruções para o Agente:**
> Sempre que você concluir um turno com alterações no repositório, adicione uma nova linha na tabela abaixo descrevendo resumidamente o que foi feito, arquivos alterados, testes executados e a instrução clara para o próximo agente.

| Data / Turno | Agente / Papel | Tarefas Realizadas | Arquivos Criados / Modificados | Testes Executados | Próximos Passos para o Próximo Agente |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **2026-10-15 (T1)** | Arquiteta de Software / Tech Lead | Limpeza de arquivos parciais de `src/`; elaboração do documento completo `PLANO_IMPLEMENTACAO.md` (15 seções, ER Mermaid, endpoints, backlog); criação do guia de handoff `AGENT_CONTEXT.md`. | `PLANO_IMPLEMENTACAO.md`, `AGENT_CONTEXT.md` | `dotnet build` validado sem erros; verificação de 100% de cobertura dos 30 RFs, 8 RNs, 10 RNFs e 8 RVs. | Iniciar a **Fase 1 / TASK-01**: Criar a solução .NET com a estrutura modular de projetos (`Api`, `Application`, `Domain`, `Infrastructure`), testes e `docker-compose.yml` para PostgreSQL. |
| **2026-10-15 (T2)** | Arquiteta de Software / Tech Lead | Incorporação dos artefatos oficiais do banco em `docs/database/` (`star.sql`, `star_postgres.sql`, `explicação.txt`, `diagrama_star.pdf`); atualização das Seções 5 e 6 do `PLANO_IMPLEMENTACAO.md` para refletir as 14 tabelas oficiais e nomes exatos de colunas; atualização do `AGENT_CONTEXT.md`. | `docs/database/*`, `PLANO_IMPLEMENTACAO.md`, `AGENT_CONTEXT.md` | `dotnet build` validado com sucesso (0 erros, 0 warnings); conferência das 14 tabelas oficiais. | Iniciar a **Fase 1 / TASK-01**: Criar a solução .NET estruturada em 4 projetos com base no schema oficial de `docs/database/star_postgres.sql`. |
| **2026-11-09 (T3)** | Desenvolvedor / TASK-01 | Criada solução .NET com Api (controllers), Application, Domain, Infrastructure, UnitTests e IntegrationTests, referências entre camadas e docker-compose com PostgreSQL 16 (init com `star_postgres.sql`). Target `net9.0` temporário (SDK 10 ausente). | `StarConcessionaria.sln`, `Directory.Build.props`, `docker-compose.yml`, `src/*`, `tests/*` | `dotnet build` OK (0 erros, 0 avisos); docker compose não executado. | TASK-02: DbContext, configurations e migrations; testar o container; migrar para net10.0 quando possível. |

---

## 7. TEMPLATE PARA O PRÓXIMO AGENTE ADICIONAR NO HISTÓRICO

Copie e preencha este bloco ao atualizar a tabela na Seção 6:

```markdown
| AAAA-MM-DD (Tn) | Seu Papel/ID | Descrição das tarefas realizadas | Lista de arquivos tocados | Comandos de teste executados | Instrução objetiva para o próximo agente |
```
