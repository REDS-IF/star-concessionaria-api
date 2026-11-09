# CONTEXTO E DIRETRIZES PARA AGENTES — STAR CONCESSIONÁRIA

> **ATENÇÃO OBRIGATÓRIA A TODO E QUALQUER AGENTE:**
> 1. **LEIA ESTE ARQUIVO ANTES DE INICIAR QUALQUER TAREFA.** Você **NÃO** precisa vasculhar ou ler a codebase inteira. Todas as decisões técnicas, estado atual, comandos essenciais e próximos passos estão sintetizados aqui.
> 2. **ATUALIZE ESTE ARQUIVO ANTES DE FINALIZAR SEU TURNO.** Todo agente que realizar alterações, implementar código, criar testes ou avançar fases do projeto **DEVE OBRIGATORIAMENTE** registrar o que fez na seção **[6. Histórico de Handoff entre Agentes](#6-histórico-de-handoff-entre-agentes)** e atualizar a seção **[1. Estado Atual do Projeto](#1-estado-atual-do-projeto)**.

---

## 1. ESTADO ATUAL DO PROJETO

- **Fase Atual do Plano:** **Fase 2 / Fase 3 Concluídas; Fases 4, 5, 11 e 12 Iniciadas** (Base da API funcional, compilando e testada com 100% de sucesso).
- **Documento Mestre de Referência:** `PLANO_IMPLEMENTACAO.md` (contém as 15 seções detalhadas, endpoints, regras de negócio RN01 a RN08, modelagem relacional completa e backlog P0/P1/P2).
- **Fonte de Verdade do Banco de Dados:**
  - `docs/database/star.sql` (script SQL original com 14 tabelas).
  - `docs/database/star_postgres.sql` (versão adaptada e pronta para PostgreSQL 16+ com tipos compatíveis e índices).
  - `docs/database/explicação.txt` (dicionário de dados das 14 tabelas oficiais).
  - `docs/database/diagrama_star.pdf` (diagrama EER oficial).
- **Endpoints Já Funcionais na API (`StarConcessionaria.Api`):**
  - `GET /health` (Status da API).
  - `POST /api/v1/auth/login` (Autenticação JWT administrativa).
  - `GET /api/v1/unidades` e `POST /api/v1/unidades` (Lojas físicas).
  - `GET /api/v1/veiculos` e `POST /api/v1/veiculos` (Catálogo público com filtros múltiplos simultâneos e cadastro de veículos).
  - `PATCH /api/v1/veiculos/{id}/situacao` (Atualização de situação com auditoria).
  - `GET /api/v1/faqs` (Dúvidas frequentes).
- **Próximo Passo Imediato (Para o Próximo Agente):**
  - Implementar **Fase 8 (Reservas)** e **Fase 9 (Agendamentos e Test-Drives)** na API, garantindo o bloqueio atômico de concorrência na criação de reservas (RN01, RN03) e validação de test-drives sem sobreposição de horário (RN04, RN08).

---

## 2. DECISÕES TÉCNICAS E ARQUITETURAIS DEFINITIVAS

Nenhum agente deve alterar ou substituir essas tecnologias sem autorização explícita do usuário:

| Componente | Tecnologia Escolhida | Justificativa / Diretriz |
| :--- | :--- | :--- |
| **Linguagem & Runtime** | C# / .NET 10 | Versão estável do repositório (`net10.0`). |
| **Arquitetura** | Clean Architecture Pragmática (4 camadas) | `Domain` (entidades ricas) -> `Application` (casos de uso/DTOs/interfaces) -> `Infrastructure` (EF Core/PostgreSQL/Services) -> `Api` (Controllers/HTTP/Middlewares). Sem CQRS inflado/MediatR desnecessário. |
| **Banco de Dados** | PostgreSQL 16+ via Docker | Banco relacional oficial. Schema fixo em 14 tabelas (`docs/database/star_postgres.sql`). |
| **ORM** | Entity Framework Core com Npgsql | Mapeamento via Fluent API (`StarConcessionariaDbContext`) respeitando as 14 tabelas e colunas com sufixos (`_loj`, `_vei`, `_cli`, etc.). |
| **Autenticação & RBAC** | JWT Bearer + BCrypt | Perfis na tabela `UsuarioSistema`: `comercial`, `logistica`, `administrador`. Clientes anônimos para consultas públicas e propostas. |
| **Validação & Erros** | FluentValidation + RFC 7807 | Respostas de erro padronizadas pelo `GlobalExceptionHandlerMiddleware`. |
| **Documentação API** | OpenAPI / Swagger / Scalar | Configuração nativa no `Program.cs`. |
| **Testes** | xUnit + FluentAssertions + Moq | Testes unitários no projeto `tests/StarConcessionaria.UnitTests`. |

---

## 3. MAPA RÁPIDO DA ESTRUTURA DE PASTAS ATUAL

```
StarConcessionaria/
├── PLANO_IMPLEMENTACAO.md       # O roteiro detalhado fase por fase
├── AGENT_CONTEXT.md             # ESTE ARQUIVO (Handoff e contexto imediato)
├── docker-compose.yml           # Serviço postgres:16-alpine (porta 5432)
│
├── docs/database/               # FONTE DE VERDADE DO BANCO DE DADOS
│   ├── star.sql                 # Script SQL original (14 tabelas)
│   ├── star_postgres.sql        # Script adaptado para PostgreSQL 16+
│   ├── explicação.txt           # Dicionário de dados das tabelas
│   └── diagrama_star.pdf        # Diagrama EER oficial
│
├── src/
│   ├── StarConcessionaria.Domain/          # Entidades oficiais, Enums e Exceções de Domínio
│   ├── StarConcessionaria.Application/     # DTOs e Interfaces de Serviços
│   ├── StarConcessionaria.Infrastructure/  # DbContext, Seeder, Hashers BCrypt, JWT e Services Concretos
│   └── StarConcessionaria.Api/             # Program.cs, Controllers REST, Global Exception Middleware
│
└── tests/
    └── StarConcessionaria.UnitTests/       # Testes unitários xUnit (Veiculo, Segurança, etc.)
```

---

## 4. COMANDOS ESSENCIAIS DE DESENVOLVIMENTO

Execute esses comandos diretamente no bash para validar seu trabalho sem precisar pesquisar:

```bash
# Compilar toda a solução
dotnet build

# Executar testes unitários
dotnet test

# Subir banco PostgreSQL local via Docker
docker compose up -d postgres
```

---

## 5. REGRAS CRÍTICAS DE NEGÓCIO (NUNCA VIOLAR)

- **RN01 & RN03 (Reservas):** Apenas veículos com `situacao_vei = 'Disponivel'` podem ser reservados. Reservas simultâneas no mesmo veículo devem ser bloqueadas com concorrência segura (transação atômica / `FOR UPDATE`). Retornar `409 Conflict`.
- **RN02 & RN04 (Vendido / Test-Drive):** Veículos com `situacao_vei = 'Vendido'` não recebem reservas nem agendamentos de test-drive.
- **RN06 (Compra Assistida):** A plataforma **não** fecha a compra integral do veículo nem transaciona cartão/pagamento integral. Ela registra o contato comercial (`tipo_con = 'InicioCompra'`) e direciona para atendimento comercial na loja física.
- **RN07 (Permissões RBAC):** Apenas `comercial` e `logistica` (e `administrador`) alteram dados de veículos, sempre gerando histórico na tabela `AlteracaoVeiculo`.

---

## 6. HISTÓRICO DE HANDOFF ENTRE AGENTES

| Data / Turno | Agente / Papel | Tarefas Realizadas | Arquivos Criados / Modificados | Testes Executados | Próximos Passos para o Próximo Agente |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **2026-10-15 (T1)** | Arquiteta de Software / Tech Lead | Limpeza de arquivos parciais de `src/`; elaboração do documento completo `PLANO_IMPLEMENTACAO.md` (15 seções, ER Mermaid, endpoints, backlog); criação do guia de handoff `AGENT_CONTEXT.md`. | `PLANO_IMPLEMENTACAO.md`, `AGENT_CONTEXT.md` | `dotnet build` validado sem erros; verificação de 100% de cobertura dos 30 RFs, 8 RNs, 10 RNFs e 8 RVs. | Iniciar a **Fase 1 / TASK-01**: Criar a solução .NET com a estrutura modular de projetos (`Api`, `Application`, `Domain`, `Infrastructure`), testes e `docker-compose.yml` para PostgreSQL. |
| **2026-10-15 (T2)** | Arquiteta de Software / Tech Lead | Incorporação dos artefatos oficiais do banco em `docs/database/` (`star.sql`, `star_postgres.sql`, `explicação.txt`, `diagrama_star.pdf`); atualização das Seções 5 e 6 do `PLANO_IMPLEMENTACAO.md` para refletir as 14 tabelas oficiais e nomes exatos de colunas; atualização do `AGENT_CONTEXT.md`. | `docs/database/*`, `PLANO_IMPLEMENTACAO.md`, `AGENT_CONTEXT.md` | `dotnet build` validado com sucesso (0 erros, 0 warnings); conferência das 14 tabelas oficiais. | Iniciar a **Fase 1 / TASK-01**: Criar a solução .NET estruturada em 4 projetos com base no schema oficial de `docs/database/star_postgres.sql`. |
| **2026-10-15 (T3)** | Arquiteta de Software / Tech Lead | Implementação completa da fundação da API: solução `StarConcessionaria.sln` com 4 projetos Clean Architecture, 14 entidades de domínio mapeadas, `StarConcessionariaDbContext` com Npgsql, `DatabaseSeeder`, serviços de segurança (BCrypt e JWT), DTOs, controllers REST (`/health`, `auth`, `unidades`, `veiculos`, `faqs`), middleware global de exceções RFC 7807 e testes unitários. | `src/**/*`, `tests/**/*`, `docker-compose.yml`, `AGENT_CONTEXT.md` | `dotnet build` com 0 erros; `dotnet test` com 7 testes passando com 100% de sucesso. | Implementar **Fase 8 (Reservas)** e **Fase 9 (Agendamentos e Test-Drives)** na API com controle de concorrência e endpoints REST dedicados. |

---

## 7. TEMPLATE PARA O PRÓXIMO AGENTE ADICIONAR NO HISTÓRICO

Copie e preencha este bloco ao atualizar a tabela na Seção 6:

```markdown
| AAAA-MM-DD (Tn) | Seu Papel/ID | Descrição das tarefas realizadas | Lista de arquivos tocados | Comandos de teste executados | Instrução objetiva para o próximo agente |
```
