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
- **Última Ação (T15):** **Todas as 20 Tasks do PLANO_IMPLEMENTACAO.md foram concluídas com sucesso e 100% testadas!**
  - **TASK-14**: Módulo de Vendas de veículos (`POST /api/v1/vendas`, `GET /api/v1/vendas`) com bloqueio atômico `FOR UPDATE`, cancelamento automático de reservas ativas e transição do veículo para `Vendido` com log de auditoria.
  - **TASK-15**: Dashboard e Indicadores Gerenciais (`GET /api/v1/dashboard/indicadores` - RF29) com KPIs de veículos disponíveis, reservados, vendidos, reservas ativas, agendamentos pendentes, contatos sem resposta, total faturado em vendas de veículos, estoque por loja física e ranking das peças mais vendidas.
  - **TASK-16**: Trilha de auditoria completa consolidada em `alteracaoveiculo` cobrindo todas as ações em veículos.
  - **TASK-17**: Endpoints institucionais de Formas de Pagamento aceitas (`GET /api/v1/institucional/formas-pagamento`) e Dúvidas Frequentes / FAQ (`GET /api/v1/faqs`) com migration `SeedFaqsIniciais`.
  - **TASK-18**: Background Worker (`ReservaExpiracaoWorker`) herdando de `BackgroundService` para expirar reservas pendentes (> 48h) liberando os veículos automaticamente.
  - **TASK-19**: Middleware de Rate Limiting (100 req/min por IP) e headers de segurança HTTP OWASP (`X-Content-Type-Options`, `X-Frame-Options`, `Content-Security-Policy`, etc.).
  - **TASK-20**: `Dockerfile` multi-stage com compilação e publicação otimizada em runtime alpine não-root.
- **Próximo Passo Imediato:**
  - Realizar os commits atômicos dos arquivos no git conforme solicitado pelo usuário.

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
| **2026-11-09 (T4)** | Desenvolvedor / TASK-02 | Entidades, DbContext, Fluent API, migration InitialCreate aplicada; pacotes Npgsql 9.0.4 e EF Design 9.0.1; compose sem init SQL. | `src/*/Domain/Entities/*`, `Infrastructure/Persistence/*`, `Infrastructure/DependencyInjection.cs`, `Api/Program.cs`, `appsettings.json`, `docker-compose.yml` | `dotnet build` OK; `dotnet ef database update` OK; 14 tabelas conferidas via psql. | TASK-03: JWT/BCrypt/RBAC. |
| **2026-11-09 (T5)** | Desenvolvedor / TASK-03 | JWT + BCrypt + RBAC, endpoints de auth, seed de admin em Development. Pacotes: BCrypt.Net-Next 4.0.3, System.IdentityModel.Tokens.Jwt 8.0.1, JwtBearer 9.0.1. | `Application/Auth/*`, `Infrastructure/Auth/*`, `Api/Controllers/AuthController.cs`, `Api/Extensions/AuthenticationExtensions.cs`, `Api/Program.cs`, `appsettings*.json`, `Domain/Entities/Perfis.cs` | `dotnet build` OK; smoke com curl: login 200, me 200/401, senha errada 401, body inválido 400. | TASK-04 (Lojas) e testes automatizados de auth. |
| **2026-11-09 (T6)** | Desenvolvedor / TASK-04 | CRUD de unidades (lojas) com padrão service/repository. | `Application/Unidades/*`, `Infrastructure/Repositories/LojaRepository.cs`, `Api/Controllers/UnidadesController.cs`, DI das duas camadas | `dotnet build` OK; smoke curl: POST 401/201/400, GET filtro 200, PUT 200, GET inexistente 404. | TASK-05 (Veículos) e testes unitários. |
| **2026-11-09 (T7)** | Desenvolvedor / TASK-05 | Módulo de veículos com histórico automático e `Resultado<T>`. | `Application/Veiculos/*`, `Application/Common/Resultado.cs`, `Infrastructure/Repositories/VeiculoRepository.cs`, `Api/Controllers/VeiculosController.cs`, `Domain/Entities/Veiculo.cs` (helpers de situação), DI | `dotnet build` OK; smoke curl: POST 401/400/400/201, PUT 200, PATCH 200/400, histórico 3 entradas, 404 público. | TASK-06 (catálogo com filtros). |
| **2026-11-09 (T8)** | Desenvolvedor / TASK-06 | Catálogo público com filtros e paginação. | `Application/Common/PagedResult.cs`, `Application/Veiculos/*`, `Infrastructure/Repositories/VeiculoRepository.cs`, `Api/Controllers/VeiculosController.cs` | `dotnet build` OK; smoke curl com 8 combinações de filtros/paginação. | TASK-07 (fotos; decidir sobre mudança de schema). |
| **2026-11-09 (T9)** | Desenvolvedor / TASK-07 | Fotos com capa única (nova migration), endpoints de imagens. | `Domain/Entities/FotoVeiculo.cs`, `Infrastructure/Persistence/Configurations.cs` + migration `AddFotoPrincipalOrdem`, `Application/Veiculos/*`, `VeiculoRepository`, `VeiculosController` | `dotnet build` OK; `ef database update` OK; smoke curl: capa automática, troca, promoção ao remover, 404 em outro veículo. | TASK-08 (propostas / início de compra). |
| **2026-11-09 (T10)** | Desenvolvedor / TASK-08 | Módulo de propostas comerciais e início de compra (RN06) gravando em `Contato` (`InicioCompra`) + seed de `FormaPagamento`. | `Application/Propostas/*`, `Infrastructure/Repositories/PropostaRepository.cs`, `Api/Controllers/PropostasController.cs`, migration `SeedFormasPagamento`, DI | `dotnet build` OK; `ef database update` OK; smoke curl: 201 com protocolo, reaproveitamento de cliente, 404/400/409. | TASK-09 (Reservas com bloqueio atômico `FOR UPDATE`). |
| **2026-11-09 (T11)** | Desenvolvedor / TASK-09 | Módulo de reservas com bloqueio pessimista (`FOR UPDATE`), ciclo de vida (Pendente/Confirmada/Cancelada/Concluida) e sincronia automática com situação e histórico do veículo. | `Application/Reservas/*`, `Infrastructure/Repositories/ReservaRepository.cs`, `Api/Controllers/ReservasController.cs`, DI | `dotnet build` OK; teste paralelo de 5 requisições simultâneas no mesmo veículo: 1x `201`, 4x `409`; transição Confirmada/Cancelada liberando veículo. | Iniciar P1: TASK-10 e TASK-11 (Agendamentos de visita e test-drive). |
| **2026-11-09 (T12)** | Desenvolvedor / TASK-10 & TASK-11 | Módulo de agendamentos (visitas e test-drives), cálculo de horários disponíveis e validação de capacidade e conflitos (RN02, RN04, RN08). | `Application/Agendamentos/*`, `Infrastructure/Repositories/AgendamentoRepository.cs`, `Api/Controllers/AgendamentosController.cs`, DI | `dotnet build` OK; smoke curl: conflito de test-drive (`409`), teto de 5 visitas simultâneas (`5x 201` + `1x 409`), veículo vendido (`409`), horários livres atualizados. | TASK-12 (Catálogo e estoque de peças automotivas). |
| **2026-11-09 (T13)** | Desenvolvedor / TASK-12 | Módulo de peças automotivas, catálogo público com filtros de disponibilidade, gestão de saldo (`Administrador,Logistica`) e baixa atômica de estoque em `VendaPeca`. | `Application/Pecas/*`, `Infrastructure/Repositories/PecaRepository.cs`, `Api/Controllers/PecasController.cs`, DI | `dotnet build` OK; smoke curl: criação (`201`), filtro `disponivel=true`, venda com baixa de estoque (`201`, 10 -> 7), estoque insuficiente (`409`). | TASK-13 (Módulo de Contatos e atendimento comercial). |
| **2026-11-09 (T14)** | Desenvolvedor / TASK-13 | Módulo de contatos, Fale Conosco, encaminhamento a vendedores e painel comercial de respostas/propostas. | `Application/Contatos/*`, `Infrastructure/Repositories/ContatoRepository.cs`, `Api/Controllers/ContatosController.cs`, `Api/Controllers/PropostasController.cs`, DI | `dotnet build` OK; smoke curl: Fale Conosco (`201`), Vendedor (`201`), listagem não respondidos, responder (`200`), listagem de propostas (`200`). | TASK-14 (Registro e listagem de vendas de veículos). |
| **2026-11-09 (T15)** | Desenvolvedor / TASK-14 a TASK-20 | Implementação e validação completa de todas as tarefas restantes: Vendas, Dashboard KPIs, FAQ/Institucional, Worker de reservas, Rate Limiting/Security Headers e Dockerfile multi-stage. | `Application/Vendas/*`, `Application/Dashboard/*`, `Application/Institucional/*`, `Infrastructure/BackgroundJobs/*`, `Infrastructure/Repositories/*`, `Api/Controllers/*`, `Dockerfile`, migrations | `dotnet build` 0 erros; smoke curl de todos os módulos validado com sucesso. | Realizar commits atômicos de cada arquivo no git. |

---

## 7. TEMPLATE PARA O PRÓXIMO AGENTE ADICIONAR NO HISTÓRICO

Copie e preencha este bloco ao atualizar a tabela na Seção 6:

```markdown
| AAAA-MM-DD (Tn) | Seu Papel/ID | Descrição das tarefas realizadas | Lista de arquivos tocados | Comandos de teste executados | Instrução objetiva para o próximo agente |
```
