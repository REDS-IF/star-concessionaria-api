# PLANO COMPLETO DE IMPLEMENTAÇÃO BACK-END — STAR CONCESSIONÁRIA

| Documento | Plano Técnico e Arquitetural de Implementação Back-End |
| :--- | :--- |
| **Projeto** | Plataforma Web — Star Concessionária |
| **Cliente** | Star Concessionária |
| **Responsável Técnico** | Arquiteta de Software Sênior & Tech Lead |
| **Tecnologias Core** | C# (.NET 10), ASP.NET Core Web API, Entity Framework Core, PostgreSQL, Docker |
| **Versão** | 1.0 — Definitiva |
| **Data** | Outubro de 2026 |

---

## 1. RESUMO EXECUTIVO

A **Star Concessionária** é uma rede automotiva que busca expandir sua presença digital por meio de uma plataforma web moderna, segura e escalável, conectando clientes, vendedores e suas unidades físicas.

O presente documento estabelece o **Plano Completo de Implementação do Back-End**, concebido a partir da rigorosa análise do Documento de Especificação de Requisitos da empresa (composto por 30 Requisitos Funcionais, 8 Regras de Negócio, 10 Requisitos Não Funcionais e 8 Requisitos a Validar).

### Principais Diretrizes da Solução
1. **Pragmatismo e Baixo Acoplamento:** Adoção de uma **Clean Architecture Pragmática em 4 Camadas** (API, Application, Domain, Infrastructure), evitando abstrações excessivas ou prematuras (como CQRS complexo com MediatR ou barramentos desnecessários), mantendo o domínio de negócio estritamente protegido e de fácil manutenção e testabilidade.
2. **Stack Tecnológica Sólida e Aberta:** C# com **ASP.NET Core Web API (.NET 10)**, ORM **Entity Framework Core (EF Core)** com provider **Npgsql**, banco de dados relacional **PostgreSQL**, conteinerização com **Docker/Docker Compose** e testes automatizados com **xUnit** e **Testcontainers**.
3. **Integridade de Negócio e Controle de Concorrência:** Tratamento rigoroso das transações de reserva e agendamento de test-drives, impedindo reservas simultâneas e conflitos de horários em veículos físicos por meio de bloqueios atômicos e validação dupla de disponibilidade (RN01, RN03, RN08, RF15).
4. **Jornada Comercial Assistida:** A compra de veículos na plataforma inicia a jornada e captura a intenção do cliente (lead qualificado), direcionando o atendimento para a equipe comercial e unidades físicas, onde o fechamento presencial ou assistido é concluído (RN06).
5. **Visibilidade Gerencial e Operacional:** Dashboards consolidados de veículos disponíveis, contatos recebidos, reservas ativas, localização por loja e peças com maior rotatividade (RF29).

Este plano fornece à equipe de engenharia um guia exaustivo, passo a passo, cobrindo modelagem de dados, catálogo completo de endpoints REST, regras de negócio detalhadas, matriz de segurança RBAC, fases sequenciais de implementação, estratégia de testes e backlog priorizado (P0/P1/P2).

---

## 2. ANÁLISE DO ESCOPO

O ecossistema da Star Concessionária atende a três grandes áreas funcionais, consumidas pela API:

```
+-----------------------------------------------------------------------------------+
|                           STAR CONCESSIONÁRIA BACK-END                            |
+-----------------------------------------------------------------------------------+
        |                                   |                                   |
        v                                   v                                   v
+------------------+             +--------------------+              +--------------------+
| Área do Cliente  |             | Área Administrativa|              |   Área Gerencial   |
| (Pública / Lead) |             |  (Operação Lojas)  |              |    (Diretoria)     |
+------------------+             +--------------------+              +--------------------+
| - Catálogo/Filtros             | - Gestão de Veículos              | - Visão Geral do   |
| - Detalhes/Imagens             | - Gestão de Unidades              |   Estoque Global   |
| - Solicitar Propostas          | - Gestão de Peças/Estoque         | - Leads e Contatos |
| - Solicitar Reservas           | - Gestão de Reservas              | - Reservas Ativas  |
| - Agendar Test-Drives          | - Gestão de Agendamentos          | - Vendas Concluídas|
| - Catálogo de Peças            | - Gestão de Propostas/Leads       | - Peças Mais       |
| - Lojas Físicas/Mapa           | - Controle de Acesso (RBAC)       |   Procuradas/Saídas|
| - FAQ / Formas Pagto           |                                   |                    |
+------------------+             +--------------------+              +--------------------+
```


### 2.2 Requisitos Não Funcionais (Conformidade Arquitetural)

| Requisito Não Funcional | Descrição | Abordagem Técnica Adotada no Back-End |
| :--- | :--- | :--- |
| **RNF01 — Responsividade** | Interface web responsiva para desktop, tablet e mobile | Endpoints REST fornecem respostas leves (JSON minificado) com paginação para renderização ágil em qualquer viewport. |
| **RNF02 — Usabilidade** | Navegação clara e intuitiva | APIs com contratos semânticos padronizados, mensagens de erro amigáveis e estruturadas em RFC 7807 (Problem Details). |
| **RNF03 — Desempenho** | Consultas, filtros e pesquisas rápidas | Índices compostos no PostgreSQL, consultas otimizadas sem tracking () no catálogo público e paginação obrigatória. |
| **RNF04 — Segurança** | Autenticação e controle de acesso administrativo | ASP.NET Core Identity / JWT Bearer com algoritmo HMAC-SHA256 e controle granular de perfis via RBAC (). |
| **RNF05 — Integridade dos Dados** | Preservação da consistência em veículos, reservas, peças e agendamentos | Transações ACID no PostgreSQL com nível de isolamento adequado e validação atômica contra duplicidade de reservas e horários. |
| **RNF06 — Compatibilidade** | Suporte aos navegadores modernos | API 100% agnóstica via protocolo HTTP/HTTPS padrão com suporte a CORS devidamente configurado para origens confiáveis. |
| **RNF07 — Escalabilidade** | Expansão de catálogo, usuários e lojas físicas | Arquitetura Stateless orientada a containers Docker, desacoplada e preparada para escalabilidade horizontal em orquestradores (Kubernetes/ECS). |
| **RNF08 — Manutenibilidade** | Código limpo, modular e fácil manutenção | Clean Architecture pragmática em 4 camadas, desacoplamento por injeção de dependência e aderência estrita aos princípios SOLID. |
| **RNF09 — Proteção de Informações** | Proteção de dados pessoais e conformidade LGPD | Criptografia de senhas em BCrypt, tráfego sob HTTPS (TLS 1.3), rate limiting contra abusos e acesso controlado a dados de contato. |
| **RNF10 — Disponibilidade** | Alta disponibilidade para clientes na internet | Healthchecks nativos (), conteinerização com Docker e estratégia de reinicialização automática (). |

### 2.1 Módulos Funcionais e Requisitos Atendidos

| Módulo | Escopo & Funcionalidades | Requisitos Cobertos |
| :--- | :--- | :--- |
| **Veículos** | Cadastro completo de novos e seminovos (marca, modelo, ano, preço, km, cor, fotos, histórico de alterações, unidade física). Catálogo público com múltiplos filtros simultâneos. Controle de situação: *Disponível*, *Reservado*, *Vendido*. | RF01, RF02, RF03, RF04, RF05, RF06, RF07, RF08, RF20, RN01, RN02, RN05, RN07 |
| **Clientes e Atendimento** | Registro de solicitações de informações, propostas comerciais, contatos diretos com vendedores e início do fluxo de aquisição de veículos. Registro de canais de contato preferenciais e formas de pagamento aceitas. | RF09, RF10, RF16, RF17, RF18, RN06 |
| **Reservas** | Solicitação de reserva por clientes, validação de disponibilidade, prevenção contra duplicidade e reservas simultâneas, ciclo de vida da reserva (Pendente, Confirmada, Cancelada, Expirada, Concluída). | RF11, RF15, RF26, RN01, RN03, RN05 |
| **Agendamentos** | Agendamento de visitas a unidades e agendamentos de test-drives. Validação rigorosa da disponibilidade do veículo e horários da loja, prevenindo choques de agenda. | RF12, RF13, RF14, RF15, RF27, RN02, RN04, RN08 |
| **Peças** | Cadastro e catalogação de peças e acessórios automotivos, categorias, controle de disponibilidade e quantidade em estoque. Estrutura preparada para evolução de vendas online. | RF21, RF22, RF23 |
| **Unidades Físicas** | Cadastro de lojas físicas, endereços completos, telefones, horário de funcionamento e associação direta dos veículos em estoque a cada loja. | RF19, RF20 |
| **Administração e Gestão** | Autenticação administrativa com JWT, perfis de acesso (RBAC), gestão operacional de contatos, aprovação de reservas/agendamentos, acompanhamento de vendas e indicadores gerenciais consolidados. | RF24, RF25, RF28, RF29, RN07, RNF04, RNF09 |
| **Institucional e Suporte** | Apresentação de formas de pagamento aceitas (à vista, consórcio, financiamento) e catálogo de Dúvidas Frequentes (FAQ). | RF18, RF30 |

---

## 3. ARQUITETURA RECOMENDADA

Para equilibrar **simplicidade, robustez, código limpo, baixo acoplamento e facilidade de evolução**, a arquitetura adotada é a **Clean Architecture Pragmática em 4 Camadas (Layered Clean Architecture)**.

### 3.1 Justificativa Técnica da Escolha

| Abordagem Avaliada | Vantagens | Desvantagens no Contexto | Decisão |
| :--- | :--- | :--- | :--- |
| **Monolito Tradicional (3 Camadas: Controller-Service-DAO)** | Extremamente rápido de iniciar. | Regras de negócio frequentemente vazam para controllers ou repositórios, acoplamento direto com o banco, difícil isolamento em testes unitários. | ❌ Rejeitado |
| **Clean Architecture Inflada (CQRS com MediatR, Event Sourcing, Domain Events)** | Alto desacoplamento para times gigantescos. | Complexidade acidental imensa, excesso de arquivos boilerplate (*Commands, Handlers, Queries, Events*), curva de aprendizado íngreme para um escopo de concessionária. | ❌ Rejeitado |
| **Clean Architecture Pragmática (4 Camadas Base: Api, Application, Domain, Infrastructure)** | **Isolamento total do Domínio**, regras testáveis sem banco ou HTTP, baixo acoplamento, injeção de dependência natural, clara separação de responsabilidades e altíssima produtividade. | Exige disciplina de dependências inversas (Dependency Inversion Principle). | **Aprovada (Solução Ideal)** |

### 3.2 Stack Tecnológica Escolhida e Justificada

- **Linguagem & Framework:** C# no **ASP.NET Core Web API (.NET 10)**. Oferece o estado da arte em performance HTTP (Kestrel), tipagem forte, recursos avançados de concorrência e suporte de longo prazo.
- **Banco de Dados Relacional:** **PostgreSQL 16+**. Excelente custo-benefício (open-source sem custo de licença), robustez com transações ACID estritas para reservas e bloqueios, tipos nativos ricos e facilidade de execução em containers Docker locais e em nuvem.
- **ORM:** **Entity Framework Core (EF Core 10)** com o provider `Npgsql.EntityFrameworkCore.PostgreSQL`. Mapeamento ORM produtivo via Fluent API (sem poluir entidades de domínio), suporte completo a Migrations versionadas, controle de transações e suporte nativo a concorrência otimista via tokens (`RowVersion` / `xmin`).
- **Autenticação & Segurança:** Tokens **JWT (JSON Web Token)** assinados com chave simétrica HMAC-SHA256, integrados ao mecanismo nativo de autorização baseada em Roles e Policies do ASP.NET Core (`[Authorize(Roles = "...")]`). Senhas criptografadas com `BCrypt.Net-Next`.
- **Validação de Entrada:** **FluentValidation**, permitindo validações declarativas, isoladas dos modelos, facilitando testes unitários das regras de payload.
- **Documentação de API:** **OpenAPI / Swagger** (`Microsoft.AspNetCore.OpenApi` + `Scalar` / `Swashbuckle`), gerando especificações interativas para o time de frontend.
- **Ambiente & Containerização:** **Docker** e **Docker Compose**, padronizando a infraestrutura de desenvolvimento (PostgreSQL + API) e garantindo paridade entre ambientes de desenvolvimento, homologação e produção.
- **Testes:** **xUnit**, **FluentAssertions**, **Moq** e **Testcontainers for .NET** (para testes de integração com banco de dados real em container descartável).

---

## 4. ESTRUTURA DE PASTAS E PROJETOS

A solução .NET é organizada em 4 projetos principais e 2 projetos de testes, respeitando estritamente a Regra de Dependência:

```
StarConcessionaria/
├── .dockerignore
├── .gitignore
├── docker-compose.yml
├── Dockerfile
├── StarConcessionaria.sln
├── README.md
├── PLANO_IMPLEMENTACAO.md
│
├── src/
│   ├── StarConcessionaria.Domain/                # Camada Central (Zero Dependências Externas)
│   │   ├── Common/                               # Entidade base, interfaces marcadoras
│   │   │   ├── BaseEntity.cs
│   │   │   └── IAggregateRoot.cs
│   │   ├── Entities/                             # Entidades de Domínio com Comportamento Rico
│   │   │   ├── Veiculo.cs
│   │   │   ├── VeiculoImagem.cs
│   │   │   ├── VeiculoHistoricoAlteracao.cs
│   │   │   ├── Unidade.cs
│   │   │   ├── Reserva.cs
│   │   │   ├── Agendamento.cs
│   │   │   ├── PropostaComercial.cs
│   │   │   ├── Contato.cs
│   │   │   ├── Peca.cs
│   │   │   ├── PecaCategoria.cs
│   │   │   ├── Venda.cs
│   │   │   ├── Faq.cs
│   │   │   └── Usuario.cs
│   │   ├── Enums/                                # Enumerações de Domínio
│   │   │   ├── SituacaoVeiculo.cs                # Disponivel, Reservado, Vendido
│   │   │   ├── TipoVeiculo.cs                    # Novo, Seminovo
│   │   │   ├── StatusReserva.cs                  # Pendente, Confirmada, Cancelada, Expirada, Concluida
│   │   │   ├── StatusAgendamento.cs              # Solicitado, Confirmado, Realizado, Cancelado
│   │   │   ├── TipoAgendamento.cs                # Visita, TestDrive
│   │   │   ├── StatusProposta.cs                 # Recebida, EmAtendimento, Concluida, Descartada
│   │   │   ├── FormaPagamentoInteresse.cs        # A Vista, Consorcio, Financiamento
│   │   │   └── PerfilUsuario.cs                  # Administrador, Comercial, Logistica
│   │   ├── ValueObjects/                         # Objetos de Valor Imutáveis
│   │   │   ├── Cpf.cs
│   │   │   ├── Email.cs
│   │   │   ├── Telefone.cs
│   │   │   └── PeriodoAgendamento.cs
│   │   ├── Exceptions/                           # Exceções Específicas de Domínio
│   │   │   ├── DomainException.cs
│   │   │   ├── VeiculoIndisponivelException.cs
│   │   │   ├── ConflitoAgendamentoException.cs
│   │   │   └── ReservaInvalidaException.cs
│   │   └── Repositories/                         # Interfaces de Persistência
│   │       ├── IUnitOfWork.cs
│   │       ├── IVeiculoRepository.cs
│   │       ├── IUnidadeRepository.cs
│   │       ├── IReservaRepository.cs
│   │       ├── IAgendamentoRepository.cs
│   │       ├── IPropostaComercialRepository.cs
│   │       ├── IContatoRepository.cs
│   │       ├── IPecaRepository.cs
│   │       ├── IVendaRepository.cs
│   │       ├── IFaqRepository.cs
│   │       └── IUsuarioRepository.cs
│   │
│   ├── StarConcessionaria.Application/           # Casos de Uso, Orquestração e DTOs
│   │   ├── Common/                               # Tipos Comuns de Resposta
│   │   │   ├── PagedResult.cs
│   │   │   └── ApiResponse.cs
│   │   ├── DTOs/                                 # Contratos de Transferência de Dados
│   │   │   ├── Veiculos/
│   │   │   │   ├── CriarVeiculoRequest.cs
│   │   │   │   ├── AtualizarVeiculoRequest.cs
│   │   │   │   ├── VeiculoDetalheResponse.cs
│   │   │   │   ├── VeiculoCatalogoResponse.cs
│   │   │   │   └── FiltroVeiculosParameters.cs
│   │   │   ├── Reservas/
│   │   │   │   ├── SolicitarReservaRequest.cs
│   │   │   │   ├── AtualizarStatusReservaRequest.cs
│   │   │   │   └── ReservaResponse.cs
│   │   │   ├── Agendamentos/
│   │   │   │   ├── AgendarVisitaRequest.cs
│   │   │   │   ├── AgendarTestDriveRequest.cs
│   │   │   │   ├── HorariosDisponiveisResponse.cs
│   │   │   │   └── AgendamentoResponse.cs
│   │   │   ├── Propostas/
│   │   │   │   ├── SolicitarPropostaRequest.cs
│   │   │   │   ├── SolicitarInformacaoRequest.cs
│   │   │   │   └── PropostaResponse.cs
│   │   │   ├── Pecas/
│   │   │   │   ├── SalvarPecaRequest.cs
│   │   │   │   └── PecaResponse.cs
│   │   │   ├── Unidades/
│   │   │   │   ├── SalvarUnidadeRequest.cs
│   │   │   │   └── UnidadeResponse.cs
│   │   │   ├── Auth/
│   │   │   │   ├── LoginRequest.cs
│   │   │   │   └── LoginResponse.cs
│   │   │   └── Dashboard/
│   │   │       └── IndicadoresGerenciaisResponse.cs
│   │   ├── Interfaces/                           # Contratos dos Serviços de Aplicação
│   │   │   ├── IVeiculoService.cs
│   │   │   ├── IReservaService.cs
│   │   │   ├── IAgendamentoService.cs
│   │   │   ├── IPropostaService.cs
│   │   │   ├── IContatoService.cs
│   │   │   ├── IPecaService.cs
│   │   │   ├── IUnidadeService.cs
│   │   │   ├── IAuthService.cs
│   │   │   ├── IDashboardService.cs
│   │   │   ├── IFaqService.cs
│   │   │   └── ITokenService.cs
│   │   ├── Services/                             # Implementação da Lógica de Aplicação
│   │   │   ├── VeiculoService.cs
│   │   │   ├── ReservaService.cs
│   │   │   ├── AgendamentoService.cs
│   │   │   ├── PropostaService.cs
│   │   │   ├── ContatoService.cs
│   │   │   ├── PecaService.cs
│   │   │   ├── UnidadeService.cs
│   │   │   ├── AuthService.cs
│   │   │   ├── DashboardService.cs
│   │   │   └── FaqService.cs
│   │   └── Validators/                           # Regras de Validação com FluentValidation
│   │       ├── CriarVeiculoRequestValidator.cs
│   │       ├── SolicitarReservaRequestValidator.cs
│   │       ├── AgendarTestDriveRequestValidator.cs
│   │       └── SolicitarPropostaRequestValidator.cs
│   │
│   ├── StarConcessionaria.Infrastructure/        # Persistência EF Core e Serviços de Infra
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/                   # Configuração Fluent API de Tabelas e Índices
│   │   │   │   ├── VeiculoConfiguration.cs
│   │   │   │   ├── VeiculoImagemConfiguration.cs
│   │   │   │   ├── VeiculoHistoricoConfiguration.cs
│   │   │   │   ├── UnidadeConfiguration.cs
│   │   │   │   ├── ReservaConfiguration.cs
│   │   │   │   ├── AgendamentoConfiguration.cs
│   │   │   │   ├── PropostaComercialConfiguration.cs
│   │   │   │   ├── PecaConfiguration.cs
│   │   │   │   ├── VendaConfiguration.cs
│   │   │   │   └── UsuarioConfiguration.cs
│   │   │   ├── Repositories/                     # Implementação dos Repositórios via EF Core
│   │   │   │   ├── BaseRepository.cs
│   │   │   │   ├── VeiculoRepository.cs
│   │   │   │   ├── ReservaRepository.cs
│   │   │   │   ├── AgendamentoRepository.cs
│   │   │   │   ├── UnidadeRepository.cs
│   │   │   │   ├── PecaRepository.cs
│   │   │   │   ├── VendaRepository.cs
│   │   │   │   └── UsuarioRepository.cs
│   │   │   ├── Seed/                             # Carga Inicial de Dados Essenciais
│   │   │   │   └── DatabaseSeeder.cs
│   │   │   └── Migrations/                       # Migrações Versionadas do Banco
│   │   └── Security/                             # Provedores de Token e Criptografia
│   │       ├── PasswordHasher.cs                 # BCrypt Hasher
│   │       └── TokenService.cs                   # Emissão de JWT Bearer
│   │
│   └── StarConcessionaria.Api/                   # Camada de Apresentação e Endpoints HTTP
│       ├── Controllers/                          # Controllers RESTful Tipados
│       │   ├── BaseApiController.cs
│       │   ├── VeiculosController.cs
│       │   ├── ReservasController.cs
│       │   ├── AgendamentosController.cs
│       │   ├── PropostasController.cs
│       │   ├── ContatosController.cs
│       │   ├── PecasController.cs
│       │   ├── UnidadesController.cs
│       │   ├── AuthController.cs
│       │   ├── DashboardController.cs
│       │   └── FaqsController.cs
│       ├── Middlewares/                          # Tratamento Global de Exceções e Respostas
│       │   └── GlobalExceptionHandlerMiddleware.cs
│       ├── Extensions/                           # Injeção de Dependências e Pipeline
│       │   ├── DependencyInjection.cs
│       │   └── AuthenticationExtensions.cs
│       ├── Program.cs                            # Configuração da Aplicação e Rotas
│       ├── appsettings.json
│       └── appsettings.Development.json
│
└── tests/
    ├── StarConcessionaria.UnitTests/             # Testes Rápidos sem I/O
    │   ├── Domain/
    │   │   ├── VeiculoTests.cs
    │   │   ├── ReservaTests.cs
    │   │   └── AgendamentoTests.cs
    │   └── Application/
    │       ├── VeiculoServiceTests.cs
    │       ├── ReservaServiceTests.cs
    │       └── AgendamentoServiceTests.cs
    └── StarConcessionaria.IntegrationTests/      # Testes E2E com Banco PostgreSQL Real
        ├── Fixtures/
        │   └── CustomWebApplicationFactory.cs    # Testcontainers PostgreSQL
        ├── Controllers/
        │   ├── VeiculosControllerTests.cs
        │   ├── ReservasConcurrencyTests.cs
        │   └── AuthControllerTests.cs
        └── Repositories/
            └── VeiculoRepositoryTests.cs
```

---

## 5. MODELAGEM DO BANCO DE DADOS (SCHEMA OFICIAL STAR CONCESSIONÁRIA)

A modelagem de dados foi rigorosamente alinhada ao schema oficial fornecido pela Star Concessionária (disponível em `docs/database/star.sql`, `docs/database/star_postgres.sql` e documentado em `docs/database/explicação.txt`). O schema é composto por **14 tabelas**, com chaves estrangeiras explícitas, campos geográficos para localização e controle de integridade relacional.

### 5.1 Especificação Detalhada das 14 Tabelas Oficiais

#### 1. `Loja` (Unidades Físicas)
- **Responsabilidade:** Guarda as concessionárias/unidades físicas da rede e seus endereços completos. As coordenadas de latitude e longitude auxiliam os clientes na identificação das lojas mais próximas (RF19, RF20).
- **Colunas:**
  - `id_loj` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador único da loja física.
  - `nome_loj` (VARCHAR(300), NOT NULL): Nome comercial da unidade (ex: "Star Concessionária - Matriz Campinas").
  - `telefone_loj` (VARCHAR(30)): Telefone de contato da loja.
  - `cep_loj` (VARCHAR(15)): Código postal.
  - `endereco_loj` (VARCHAR(300)): Logradouro (rua, avenida).
  - `numero_loj` (VARCHAR(30)): Número do imóvel.
  - `complemento_loj` (VARCHAR(100)): Informações complementares.
  - `bairro_loj` (VARCHAR(100)): Bairro.
  - `cidade_loj` (VARCHAR(100)): Município da loja.
  - `estado_loj` (CHAR(2)): Unidade federativa (ex: "SP").
  - `latitude_loj` (DECIMAL(10,7)): Latitude geográfica da loja.
  - `longitude_loj` (DECIMAL(10,7)): Longitude geográfica da loja.

#### 2. `UsuarioSistema` (Usuários Internos e Autenticação)
- **Responsabilidade:** Armazena os funcionários da concessionária e seus respectivos perfis administrativos e operacionais (RF24, RN07).
- **Colunas:**
  - `id_usu` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador do usuário interno.
  - `nome_usu` (VARCHAR(300), NOT NULL): Nome do colaborador.
  - `email_usu` (VARCHAR(300), NOT NULL, UNIQUE): E-mail institucional de acesso.
  - `senha_usu` (VARCHAR(300), NOT NULL): Hash seguro da senha (BCrypt, nunca armazenado em texto puro).
  - `perfil_usu` (VARCHAR(100), NOT NULL): Papel/perfil no sistema (`comercial`, `logistica`, `administrador`).

#### 3. `Cliente`
- **Responsabilidade:** Centraliza os dados de contato dos clientes que interagem na plataforma (propostas, dúvidas, reservas, agendamentos ou compras).
- **Colunas:**
  - `id_cli` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador único do cliente.
  - `nome_cli` (VARCHAR(300), NOT NULL): Nome completo do cliente.
  - `email_cli` (VARCHAR(300), NOT NULL): E-mail de contato.
  - `telefone_cli` (VARCHAR(30), NOT NULL): Telefone / WhatsApp com DDD.

#### 4. `FormaPagamento`
- **Responsabilidade:** Cadastra as formas de pagamento aceitas pela concessionária para aquisição de veículos (RF18).
- **Colunas:**
  - `id_fp` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador da forma de pagamento.
  - `nome_fp` (VARCHAR(100), NOT NULL, UNIQUE): Nome da modalidade (`À Vista`, `Consórcio`, `Financiamento`).

#### 5. `Veiculo` (Estoque Central)
- **Responsabilidade:** Armazena os dados dos veículos novos e seminovos em estoque e a respectiva loja onde cada veículo físico está localizado (RF01, RF02, RF04, RF20).
- **Colunas:**
  - `id_vei` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador do veículo.
  - `id_loj_fk` (INT, NOT NULL, FK -> `Loja.id_loj`): Loja física onde o veículo está alocado.
  - `tipo_vei` (VARCHAR(30), NOT NULL): Tipo (`Novo` ou `Seminovo`).
  - `marca_vei` (VARCHAR(100), NOT NULL): Fabricante / Marca (RF02).
  - `modelo_vei` (VARCHAR(100), NOT NULL): Modelo do automóvel.
  - `ano_vei` (INT, NOT NULL): Ano de fabricação / modelo.
  - `preco_vei` (DECIMAL(12,2), NOT NULL): Preço de venda anunciado.
  - `quilometragem_vei` (INT, NOT NULL): KM rodado (0 para novos).
  - `cor_vei` (VARCHAR(50), NOT NULL): Cor externa.
  - `descricao_vei` (TEXT): Descrição detalhada dos opcionais e acessórios.
  - `situacao_vei` (VARCHAR(30), NOT NULL, DEFAULT 'Disponivel'): Situação atual: `Disponivel`, `Reservado`, `Vendido` (RF04, RN01, RN02).

#### 6. `FotoVeiculo`
- **Responsabilidade:** Guarda a galeria de fotografias vinculadas a cada veículo (RF02, RF08).
- **Colunas:**
  - `id_fot` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador da foto.
  - `id_vei_fk` (INT, NOT NULL, FK -> `Veiculo.id_vei` ON DELETE CASCADE): Veículo associado.
  - `url_fot` (VARCHAR(500), NOT NULL): URL ou caminho da foto.

#### 7. `AlteracaoVeiculo` (Histórico e Auditoria)
- **Responsabilidade:** Registra alterações e modificações realizadas em um veículo (RF02, RN07).
- **Colunas:**
  - `id_alt` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador do registro de alteração.
  - `id_vei_fk` (INT, NOT NULL, FK -> `Veiculo.id_vei` ON DELETE CASCADE): Veículo alterado.
  - `descricao_alt` (TEXT, NOT NULL): Descrição do que foi modificado no veículo.

#### 8. `Contato` (Atendimento e Solicitação Comercial)
- **Responsabilidade:** Registra pedidos de informação, solicitações de propostas comerciais, início de compra e encaminhamentos a vendedores (RF09, RF10, RF16, RF17, RF25).
- **Colunas:**
  - `id_con` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador do contato.
  - `id_cli_fk` (INT, FK -> `Cliente.id_cli`): Cliente solicitante.
  - `id_vei_fk` (INT, FK -> `Veiculo.id_vei`): Veículo de interesse (se aplicável).
  - `id_loj_fk` (INT, FK -> `Loja.id_loj`): Loja de preferência do cliente.
  - `id_usu_fk` (INT, FK -> `UsuarioSistema.id_usu`): Vendedor responsável pelo atendimento.
  - `tipo_con` (VARCHAR(100), NOT NULL): Tipo do contato (`Informacao`, `Proposta`, `InicioCompra`, `ContatoVendedor`).
  - `mensagem_con` (TEXT, NOT NULL): Conteúdo da mensagem ou dados da proposta.
  - `data_con` (DATETIME / TIMESTAMPTZ, NOT NULL): Data e hora do registro.

#### 9. `Reserva`
- **Responsabilidade:** Registra a solicitação e o controle de reservas de veículos (RF11, RF15, RF26, RN01, RN03).
- **Colunas:**
  - `id_res` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador da reserva.
  - `id_cli_fk` (INT, NOT NULL, FK -> `Cliente.id_cli`): Cliente que solicitou a reserva.
  - `id_vei_fk` (INT, NOT NULL, FK -> `Veiculo.id_vei`): Veículo reservado.
  - `data_solicitacao_res` (DATETIME / TIMESTAMPTZ, NOT NULL): Data/hora da reserva.
  - `situacao_res` (VARCHAR(100), NOT NULL, DEFAULT 'Pendente'): Situação da reserva (`Pendente`, `Confirmada`, `Cancelada`, `Expirada`, `Concluida`).

#### 10. `Agendamento`
- **Responsabilidade:** Registra visitas presenciais a lojas e test-drives agendados pelos clientes (RF12, RF13, RF14, RF15, RF27, RN02, RN04, RN08).
- **Colunas:**
  - `id_age` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador do agendamento.
  - `id_cli_fk` (INT, NOT NULL, FK -> `Cliente.id_cli`): Cliente solicitante.
  - `id_loj_fk` (INT, NOT NULL, FK -> `Loja.id_loj`): Loja física onde ocorrerá a visita/test-drive.
  - `id_vei_fk` (INT, FK -> `Veiculo.id_vei`): Veículo associado (obrigatório para test-drive, opcional para visita geral).
  - `tipo_age` (VARCHAR(100), NOT NULL): Tipo (`Visita`, `TestDrive`).
  - `data_hora_inicio_age` (DATETIME / TIMESTAMPTZ, NOT NULL): Horário inicial do agendamento.
  - `data_hora_fim_age` (DATETIME / TIMESTAMPTZ, NOT NULL): Horário de término do agendamento.
  - `situacao_age` (VARCHAR(100), NOT NULL, DEFAULT 'Solicitado'): Situação (`Solicitado`, `Confirmado`, `Realizado`, `Cancelado`).

#### 11. `Peca` (Catálogo e Estoque de Peças)
- **Responsabilidade:** Guarda o catálogo de peças automotivas e a quantidade em estoque atual (RF21, RF22, RF23).
- **Colunas:**
  - `id_pec` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador da peça.
  - `nome_pec` (VARCHAR(300), NOT NULL): Nome comercial da peça.
  - `descricao_pec` (TEXT): Descrição técnica e compatibilidade veicular.
  - `preco_pec` (DECIMAL(10,2), NOT NULL): Preço unitário da peça.
  - `quantidade_estoque_pec` (INT, NOT NULL, DEFAULT 0): Saldo de estoque físico da rede.

#### 12. `VendaVeiculo` (Registro de Vendas de Veículos)
- **Responsabilidade:** Registra uma venda concluída e relaciona veículo, cliente, vendedor, loja e forma de pagamento utilizada (RF28, RF29).
- **Colunas:**
  - `id_vv` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador da venda de veículo.
  - `id_vei_fk` (INT, NOT NULL, UNIQUE, FK -> `Veiculo.id_vei`): Veículo comercializado.
  - `id_cli_fk` (INT, NOT NULL, FK -> `Cliente.id_cli`): Cliente comprador.
  - `id_vendedor_fk` (INT, NOT NULL, FK -> `UsuarioSistema.id_usu`): Vendedor responsável pelo fechamento.
  - `id_loj_fk` (INT, NOT NULL, FK -> `Loja.id_loj`): Loja física onde a venda foi finalizada.
  - `id_fp_fk` (INT, NOT NULL, FK -> `FormaPagamento.id_fp`): Forma de pagamento contratada.
  - `data_venda_vv` (DATETIME / TIMESTAMPTZ, NOT NULL): Data e hora da assinatura/conclusão da venda.
  - `valor_final_vv` (DECIMAL(12,2), NOT NULL): Valor final negociado.

#### 13. `VendaPeca` (Registro de Saída e Venda de Peças)
- **Responsabilidade:** Registra as peças vendidas, quantidade, valor unitário, data da venda e cliente (RF28, RF29 - Peças com maior saída).
- **Colunas:**
  - `id_vp` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador da venda de peça.
  - `id_pec_fk` (INT, NOT NULL, FK -> `Peca.id_pec`): Peça comercializada.
  - `id_cli_fk` (INT, FK -> `Cliente.id_cli`): Cliente comprador (se identificado).
  - `quantidade_vp` (INT, NOT NULL): Quantidade vendida.
  - `valor_unitario_vp` (DECIMAL(10,2), NOT NULL): Preço unitário praticado na venda.
  - `data_venda_vp` (DATETIME / TIMESTAMPTZ, NOT NULL): Data e hora da transação.

#### 14. `DuvidaFrequente` (FAQ Institucional)
- **Responsabilidade:** Guarda perguntas e respostas frequentes exibidas aos clientes no portal web (RF30). Não requer relacionamento com as demais tabelas.
- **Colunas:**
  - `id_df` (INT, PK, AUTO_INCREMENT / SERIAL): Identificador da dúvida frequente.
  - `pergunta_df` (TEXT, NOT NULL): Pergunta frequente.
  - `resposta_df` (TEXT, NOT NULL): Resposta oficial da concessionária.

---

## 6. DIAGRAMA ER (ENTITY RELATIONSHIP) — SCHEMA OFICIAL

O diagrama a seguir reflete fielmente as 14 tabelas oficiais da Star Concessionária e seus relacionamentos:

```mermaid
erDiagram
    Loja ||--o{ Veiculo : "possui no estoque (id_loj_fk)"
    Loja ||--o{ Agendamento : "sedia (id_loj_fk)"
    Loja ||--o{ Contato : "direcionado a (id_loj_fk)"
    Loja ||--o{ VendaVeiculo : "efetua (id_loj_fk)"

    Veiculo ||--o{ FotoVeiculo : "possui (id_vei_fk)"
    Veiculo ||--o{ AlteracaoVeiculo : "possui historico (id_vei_fk)"
    Veiculo ||--o{ Contato : "referencia (id_vei_fk)"
    Veiculo ||--o{ Reserva : "recebe (id_vei_fk)"
    Veiculo ||--o{ Agendamento : "usado em test-drive (id_vei_fk)"
    Veiculo ||--o| VendaVeiculo : "comercializado em (id_vei_fk)"

    Cliente ||--o{ Contato : "envia (id_cli_fk)"
    Cliente ||--o{ Reserva : "solicita (id_cli_fk)"
    Cliente ||--o{ Agendamento : "agenda (id_cli_fk)"
    Cliente ||--o{ VendaVeiculo : "compra (id_cli_fk)"
    Cliente ||--o{ VendaPeca : "adquire (id_cli_fk)"

    UsuarioSistema ||--o{ Contato : "atende (id_usu_fk)"
    UsuarioSistema ||--o{ VendaVeiculo : "vende (id_vendedor_fk)"

    FormaPagamento ||--o{ VendaVeiculo : "utilizada em (id_fp_fk)"

    Peca ||--o{ VendaPeca : "item vendido (id_pec_fk)"

    Loja {
        int id_loj PK
        string nome_loj
        string telefone_loj
        string cep_loj
        string endereco_loj
        string numero_loj
        string complemento_loj
        string bairro_loj
        string cidade_loj
        string estado_loj
        decimal latitude_loj
        decimal longitude_loj
    }

    UsuarioSistema {
        int id_usu PK
        string nome_usu
        string email_usu UK
        string senha_usu
        string perfil_usu "comercial | logistica | administrador"
    }

    Cliente {
        int id_cli PK
        string nome_cli
        string email_cli
        string telefone_cli
    }

    FormaPagamento {
        int id_fp PK
        string nome_fp UK
    }

    Veiculo {
        int id_vei PK
        int id_loj_fk FK
        string tipo_vei "Novo | Seminovo"
        string marca_vei
        string modelo_vei
        int ano_vei
        decimal preco_vei
        int quilometragem_vei
        string cor_vei
        text descricao_vei
        string situacao_vei "Disponivel | Reservado | Vendido"
    }

    FotoVeiculo {
        int id_fot PK
        int id_vei_fk FK
        string url_fot
    }

    AlteracaoVeiculo {
        int id_alt PK
        int id_vei_fk FK
        text descricao_alt
    }

    Contato {
        int id_con PK
        int id_cli_fk FK
        int id_vei_fk FK
        int id_loj_fk FK
        int id_usu_fk FK
        string tipo_con
        text mensagem_con
        datetime data_con
    }

    Reserva {
        int id_res PK
        int id_cli_fk FK
        int id_vei_fk FK
        datetime data_solicitacao_res
        string situacao_res "Pendente | Confirmada | Cancelada | Expirada | Concluida"
    }

    Agendamento {
        int id_age PK
        int id_cli_fk FK
        int id_loj_fk FK
        int id_vei_fk FK
        string tipo_age "Visita | TestDrive"
        datetime data_hora_inicio_age
        datetime data_hora_fim_age
        string situacao_age "Solicitado | Confirmado | Realizado | Cancelado"
    }

    Peca {
        int id_pec PK
        string nome_pec
        text descricao_pec
        decimal preco_pec
        int quantidade_estoque_pec
    }

    VendaVeiculo {
        int id_vv PK
        int id_vei_fk FK UK
        int id_cli_fk FK
        int id_vendedor_fk FK
        int id_loj_fk FK
        int id_fp_fk FK
        datetime data_venda_vv
        decimal valor_final_vv
    }

    VendaPeca {
        int id_vp PK
        int id_pec_fk FK
        int id_cli_fk FK
        int quantidade_vp
        decimal valor_unitario_vp
        datetime data_venda_vp
    }

    DuvidaFrequente {
        int id_df PK
        text pergunta_df
        text resposta_df
    }
```

---

## 7. CATÁLOGO DE ENDPOINTS DA API

A API segue os padrões arquiteturais RESTful, com rotas semânticas, substantivos no plural, verbos HTTP corretos, paginação padronizada, versionamento via rota (`/api/v1/...`) e códigos de status HTTP adequados.

### 7.1 Módulo: Catálogo e Veículos

| Método | Rota | Objetivo | Parâmetros / Body | Resposta | HTTP Status | Auth / Perfil |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/veiculos` | Listar catálogo público com filtros simultâneos (RF05, RF06, RF07) | Query: `marca`, `modelo`, `precoMin`, `precoMax`, `anoMin`, `anoMax`, `tipo`, `unidadeId`, `pagina`, `itensPorPagina` | PagedResult de `VeiculoCatalogoResponse` | `200 OK` | Anônimo (Público) |
| `GET` | `/api/v1/veiculos/{id}` | Obter detalhes completos do veículo (RF08, RF20) | Path: `id` (UUID) | `VeiculoDetalheResponse` (dados, fotos, loja, situação) | `200 OK` / `404 Not Found` | Anônimo (Público) |
| `POST` | `/api/v1/veiculos` | Cadastrar novo veículo (RF01, RF02) | Body: `CriarVeiculoRequest` | Objeto criado com `Id` | `201 Created` / `400 Bad Request` | `[Authorize(Roles = "Administrador,Comercial,Logistica")]` |
| `PUT` | `/api/v1/veiculos/{id}` | Atualizar dados cadastrais do veículo (RF03) | Path: `id`, Body: `AtualizarVeiculoRequest` | Objeto atualizado | `200 OK` / `404 Not Found` | `[Authorize(Roles = "Administrador,Comercial,Logistica")]` |
| `PATCH` | `/api/v1/veiculos/{id}/situacao` | Atualizar situação do veículo (RF04, RN01, RN02) | Path: `id`, Body: `{ "situacao": "Reservado" \| "Vendido" \| "Disponivel", "justificativa": "..." }` | Objeto atualizado | `200 OK` / `400 Bad Request` | `[Authorize(Roles = "Administrador,Comercial,Logistica")]` |
| `POST` | `/api/v1/veiculos/{id}/imagens` | Upload/vinculação de fotografias do veículo (RF02) | Path: `id`, Body: `MultipartFormData` ou `{ "url": "...", "principal": true, "ordem": 1 }` | `VeiculoImagemResponse` | `201 Created` | `[Authorize(Roles = "Administrador,Comercial,Logistica")]` |
| `DELETE` | `/api/v1/veiculos/{id}/imagens/{imagemId}` | Remover foto da galeria do veículo | Path: `id`, `imagemId` | Vazio | `204 No Content` | `[Authorize(Roles = "Administrador,Comercial,Logistica")]` |
| `GET` | `/api/v1/veiculos/{id}/historico` | Consultar auditoria de alterações do veículo (RF02) | Path: `id` | Lista de `VeiculoHistoricoResponse` | `200 OK` | `[Authorize(Roles = "Administrador,Comercial,Logistica")]` |

### 7.2 Módulo: Reservas de Veículos

| Método | Rota | Objetivo | Parâmetros / Body | Resposta | HTTP Status | Auth / Perfil |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/reservas` | Solicitar nova reserva de veículo (RF11, RF15, RN01, RN03) | Body: `SolicitarReservaRequest` (`veiculoId`, `nomeCliente`, `emailCliente`, `telefoneCliente`, `cpfCliente`, `observacoes`) | `ReservaResponse` (código da reserva, validade, status Pendente) | `201 Created` / `400 Bad Request` / `409 Conflict` | Anônimo (Público) |
| `GET` | `/api/v1/reservas/{id}` | Consultar detalhes de uma reserva específica | Path: `id` | `ReservaResponse` | `200 OK` / `404 Not Found` | `[Authorize(Roles = "Administrador,Comercial")]` |
| `GET` | `/api/v1/reservas` | Listar e gerenciar reservas com filtros (RF26) | Query: `status`, `veiculoId`, `cpfCliente`, `dataInicio`, `pagina`, `itensPorPagina` | PagedResult de `ReservaResponse` | `200 OK` | `[Authorize(Roles = "Administrador,Comercial")]` |
| `PATCH` | `/api/v1/reservas/{id}/status` | Confirmar, cancelar ou concluir reserva (RF26) | Path: `id`, Body: `{ "novoStatus": "Confirmada" \| "Cancelada" \| "Concluida", "motivo": "..." }` | `ReservaResponse` | `200 OK` / `400 Bad Request` | `[Authorize(Roles = "Administrador,Comercial")]` |

### 7.3 Módulo: Agendamentos e Test-Drives

| Método | Rota | Objetivo | Parâmetros / Body | Resposta | HTTP Status | Auth / Perfil |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/agendamentos/horarios-disponiveis` | Consultar horários livres para visita ou test-drive (RF14) | Query: `unidadeId`, `data`, `veiculoId` (opcional) | Lista de faixas de horários (`09:00`, `10:00`, etc.) | `200 OK` | Anônimo (Público) |
| `POST` | `/api/v1/agendamentos/visita` | Solicitar agendamento de visita presencial a uma unidade (RF12) | Body: `AgendarVisitaRequest` (`unidadeId`, `nomeCliente`, `emailCliente`, `telefoneCliente`, `dataHoraInicio`) | `AgendamentoResponse` | `201 Created` / `400 Bad Request` | Anônimo (Público) |
| `POST` | `/api/v1/agendamentos/test-drive` | Solicitar agendamento de test-drive (RF13, RF14, RF15, RN04, RN08) | Body: `AgendarTestDriveRequest` (`veiculoId`, `unidadeId`, `nomeCliente`, `emailCliente`, `telefoneCliente`, `cnhCliente`, `dataHoraInicio`) | `AgendamentoResponse` | `201 Created` / `400 Bad Request` / `409 Conflict` | Anônimo (Público) |
| `GET` | `/api/v1/agendamentos` | Gestão administrativa de agendamentos (RF27) | Query: `unidadeId`, `status`, `tipo`, `data`, `pagina`, `itensPorPagina` | PagedResult de `AgendamentoResponse` | `200 OK` | `[Authorize(Roles = "Administrador,Comercial,Logistica")]` |
| `PATCH` | `/api/v1/agendamentos/{id}/status` | Confirmar, concluir ou cancelar agendamento | Path: `id`, Body: `{ "status": "Confirmado" \| "Realizado" \| "Cancelado", "observacoes": "..." }` | `AgendamentoResponse` | `200 OK` / `400 Bad Request` | `[Authorize(Roles = "Administrador,Comercial")]` |

### 7.4 Módulo: Propostas Comerciais, Contatos e Atendimento

| Método | Rota | Objetivo | Parâmetros / Body | Resposta | HTTP Status | Auth / Perfil |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/propostas` | Iniciar processo de compra / Solicitar proposta comercial (RF10, RF16, RN06) | Body: `SolicitarPropostaRequest` (`veiculoId`, `unidadeId`, `nomeCliente`, `emailCliente`, `telefoneCliente`, `formaPagamento`, `possuiTroca`, `dadosTroca`, `valorEntrada`) | `PropostaResponse` com número de protocolo gerado | `201 Created` / `400 Bad Request` | Anônimo (Público) |
| `POST` | `/api/v1/contatos/vendedor` | Encaminhar cliente para atendimento direto com vendedor (RF17) | Body: `ContatoVendedorRequest` (`veiculoId`, `unidadeId`, `nome`, `email`, `telefone`, `mensagem`) | Protocolo de encaminhamento comercial | `201 Created` | Anônimo (Público) |
| `POST` | `/api/v1/contatos` | Solicitar informações adicionais ou Fale Conosco (RF09) | Body: `NovoContatoRequest` (`assunto`, `nome`, `email`, `telefone`, `mensagem`, `veiculoId`) | Protocolo de atendimento | `201 Created` | Anônimo (Público) |
| `GET` | `/api/v1/propostas` | Gestão comercial de propostas recebidas | Query: `status`, `unidadeId`, `dataInicio`, `pagina` | PagedResult de `PropostaResponse` | `200 OK` | `[Authorize(Roles = "Administrador,Comercial")]` |
| `PATCH` | `/api/v1/propostas/{id}/status` | Atualizar andamento da proposta comercial | Path: `id`, Body: `{ "status": "EmAtendimento" \| "Concluida" \| "Descartada", "notas": "..." }` | `PropostaResponse` | `200 OK` | `[Authorize(Roles = "Administrador,Comercial")]` |
| `GET` | `/api/v1/contatos` | Gestão de contatos e solicitações (RF25) | Query: `respondido`, `pagina`, `itensPorPagina` | PagedResult de `ContatoResponse` | `200 OK` | `[Authorize(Roles = "Administrador,Comercial")]` |
| `PATCH` | `/api/v1/contatos/{id}/responder` | Marcar contato como atendido/respondido | Path: `id`, Body: `{ "notasResposta": "..." }` | `ContatoResponse` | `200 OK` | `[Authorize(Roles = "Administrador,Comercial")]` |

### 7.5 Módulo: Peças Automotivas

| Método | Rota | Objetivo | Parâmetros / Body | Resposta | HTTP Status | Auth / Perfil |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/pecas` | Consultar catálogo de peças com filtros (RF21, RF23) | Query: `busca`, `categoriaId`, `fabricante`, `disponivel`, `pagina` | PagedResult de `PecaResponse` | `200 OK` | Anônimo (Público) |
| `GET` | `/api/v1/pecas/{id}` | Detalhes e especificações da peça | Path: `id` | `PecaResponse` | `200 OK` / `404 Not Found` | Anônimo (Público) |
| `GET` | `/api/v1/pecas/categorias` | Listar categorias de peças cadastradas | N/A | Lista de `PecaCategoriaResponse` | `200 OK` | Anônimo (Público) |
| `POST` | `/api/v1/pecas` | Cadastrar nova peça (RF22) | Body: `SalvarPecaRequest` | `PecaResponse` criado | `201 Created` | `[Authorize(Roles = "Administrador,Logistica")]` |
| `PUT` | `/api/v1/pecas/{id}` | Atualizar informações e saldo de estoque de peças (RF22, RF23) | Path: `id`, Body: `AtualizarPecaRequest` | `PecaResponse` atualizado | `200 OK` | `[Authorize(Roles = "Administrador,Logistica")]` |

### 7.6 Módulo: Unidades Físicas (Lojas)

| Método | Rota | Objetivo | Parâmetros / Body | Resposta | HTTP Status | Auth / Perfil |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/unidades` | Listar lojas físicas e localização para o cliente (RF19) | Query: `cidade`, `estado`, `somenteAtivas` | Lista de `UnidadeResponse` | `200 OK` | Anônimo (Público) |
| `GET` | `/api/v1/unidades/{id}` | Detalhes da unidade física e horário | Path: `id` | `UnidadeResponse` | `200 OK` / `404 Not Found` | Anônimo (Público) |
| `POST` | `/api/v1/unidades` | Cadastrar nova loja física | Body: `SalvarUnidadeRequest` | `UnidadeResponse` criado | `201 Created` | `[Authorize(Roles = "Administrador")]` |
| `PUT` | `/api/v1/unidades/{id}` | Atualizar dados da loja física | Path: `id`, Body: `SalvarUnidadeRequest` | `UnidadeResponse` atualizado | `200 OK` | `[Authorize(Roles = "Administrador")]` |

### 7.7 Módulo: Administração, Autenticação e Indicadores Gerenciais

| Método | Rota | Objetivo | Parâmetros / Body | Resposta | HTTP Status | Auth / Perfil |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/login` | Autenticação administrativa com credenciais (RF24) | Body: `LoginRequest` (`email`, `senha`) | `LoginResponse` (Token JWT, expiração, dados do usuário e perfil) | `200 OK` / `401 Unauthorized` | Anônimo (Público) |
| `GET` | `/api/v1/auth/me` | Obter dados do usuário logado via token | Cabeçalho `Authorization: Bearer <token>` | `UsuarioResponse` | `200 OK` / `401 Unauthorized` | Qualquer usuário autenticado |
| `GET` | `/api/v1/vendas` | Acompanhamento gerencial de vendas concluídas (RF28) | Query: `unidadeId`, `vendedorId`, `periodoInicio`, `periodoFim`, `pagina` | PagedResult de `VendaResponse` | `200 OK` | `[Authorize(Roles = "Administrador,Comercial")]` |
| `POST` | `/api/v1/vendas` | Registrar venda concluída de veículo (conecta à proposta/veículo) | Body: `RegistrarVendaRequest` | `VendaResponse` criado | `201 Created` | `[Authorize(Roles = "Administrador,Comercial")]` |
| `GET` | `/api/v1/dashboard/indicadores` | Painel de indicadores administrativos e KPIs consolidados (RF29) | Query: `unidadeId` (opcional para filtro por loja) | `IndicadoresGerenciaisResponse` (totais de veículos disponíveis, contatos recebidos, reservas ativas, vendas, peças mais vendidas, estoque por loja) | `200 OK` | `[Authorize(Roles = "Administrador,Comercial,Logistica")]` |
| `GET` | `/api/v1/faqs` | Listar perguntas frequentes e respostas (RF30) | Query: `categoria` | Lista de `FaqResponse` | `200 OK` | Anônimo (Público) |
| `GET` | `/api/v1/institucional/formas-pagamento` | Apresentar formas de pagamento aceitas pela concessionária (RF18) | N/A | Lista explicativa de formas aceitas (`A Vista`, `Consorcio`, `Financiamento`) | `200 OK` | Anônimo (Público) |

---

## 8. REGRAS DE NEGÓCIO E CONTROLE DE CONCORRÊNCIA

Esta seção detalha o mapeamento exato das regras de negócio do documento de requisitos (`RN01` a `RN08`), especificando a camada responsável por sua aplicação, tratamento de exceções e exemplos práticos de fluxos de sucesso e falha.

### 8.1 Detalhamento das Regras de Negócio

#### RN01 — Disponibilidade de veículos para reserva
- **Regra:** Somente veículos classificados com a situação `Disponivel` poderão receber novas solicitações de reserva.
- **Camada de Implementação:** `Domain` (método de negócio na entidade `Veiculo.SolicitarReserva(...)`) e `Application` (`ReservaService`).
- **Comportamento:** Antes de aceitar a reserva, o sistema consulta a situação do veículo. Se estiver `Reservado` ou `Vendido`, lança `VeiculoIndisponivelException`.
- **Cenário de Sucesso:** Veículo com `Situacao = Disponivel` recebe solicitação de reserva; o sistema cria a reserva com status `Pendente` e altera a situação do veículo para `Reservado`.
- **Cenário de Falha:** Cliente tenta reservar veículo com status `Reservado`. A API retorna `409 Conflict` ou `400 Bad Request` com a mensagem *"O veículo selecionado não está disponível para reserva no momento."*

#### RN02 — Veículo vendido
- **Regra:** Quando um veículo for classificado como `Vendido`, o sistema deverá impedir terminantemente novas reservas e novos agendamentos relacionados à sua disponibilidade.
- **Camada de Implementação:** `Domain` (`Veiculo.MarcarComoVendido(...)`) e `Application` (`VeiculoService`, `ReservaService`, `AgendamentoService`).
- **Comportamento:** O veículo vendido não é elegível para test-drive e nem para reserva. Além disso, cancela automaticamente reservas pendentes anteriores e envia notificação.
- **Cenário de Sucesso:** Administrador registra venda do veículo. Situação do veículo transita para `Vendido`.
- **Cenário de Falha:** Requisição para `/api/v1/agendamentos/test-drive` informando veículo com situação `Vendido`. A API rejeita com `422 Unprocessable Entity` informando *"Veículo já comercializado. Test-drives e reservas estão desabilitados."*

#### RN03 — Prevenção de reservas concorrentes e duplicadas
- **Regra:** Ao existir uma reserva válida (ativa, pendente ou confirmada dentro do prazo de expiração), o sistema deverá considerar essa informação antes de aceitar uma nova solicitação para o mesmo veículo.
- **Camada de Implementação:** `Application` (`ReservaService`) e `Infrastructure` (transação com bloqueio pessimista ou verificação atômica de concorrência).
- **Mecanismo Técnico:**
  ```csharp
  await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
  var veiculo = await _context.Veiculos
      .FromSqlInterpolated($"SELECT * FROM "Veiculos" WHERE "Id" = {veiculoId} FOR UPDATE")
      .FirstOrDefaultAsync();

  if (veiculo == null || veiculo.Situacao != SituacaoVeiculo.Disponivel)
      throw new VeiculoIndisponivelException("Veículo indisponível ou já reservado.");
  ```
- **Cenário de Sucesso:** Usuário A solicita reserva às 10:00:00. O veículo é bloqueado, a reserva é registrada e a situação muda para `Reservado`.
- **Cenário de Falha:** Usuário B solicita reserva do mesmo veículo às 10:00:01. Ao adquirir a trava de linha, o sistema detecta que o veículo agora está `Reservado` e rejeita a requisição do Usuário B com `409 Conflict`.

#### RN04 — Elegibilidade e disponibilidade de Test-drive
- **Regra:** O agendamento de test-drive deverá estar rigorosamente condicionado à disponibilidade do veículo no período solicitado.
- **Camada de Implementação:** `Domain` (`Agendamento`) e `Application` (`AgendamentoService`).
- **Comportamento:** Verifica se o veículo está na loja da visita, se sua situação é `Disponivel` ou `Reservado` (caso a concessionária permita test-drive em veículo reservado sob autorização, por padrão bloqueia-se) e se não existe nenhum outro agendamento no intervalo `[DataHoraInicio, DataHoraFim]`.
- **Cenário de Sucesso:** Solicitado test-drive para quarta-feira às 14:00. Nenhum agendamento prévio existe entre 13:30 e 15:00 para aquele veículo. O agendamento é registrado como `Solicitado`.
- **Cenário de Falha:** Solicitado test-drive para o mesmo veículo às 14:15. O sistema detecta sobreposição no intervalo e rejeita com `409 Conflict` sugerindo os próximos horários livres.

#### RN05 — Atualização de disponibilidade e reflexo no catálogo
- **Regra:** Alterações na situação de um veículo deverão refletir imediatamente nas funcionalidades oferecidas ao cliente.
- **Camada de Implementação:** `Domain` e `Application` (`VeiculoService`, cache invalidation).
- **Comportamento:** Quando um veículo passa de `Disponivel` para `Reservado` ou `Vendido`, o catálogo público oculta botões de "Reservar Agora" e "Agendar Test-Drive", e consultas com filtro `somenteDisponiveis=true` deixam de exibi-lo imediatamente.

#### RN06 — Início da compra de veículos pela plataforma (Processo Assistido)
- **Regra:** A compra de um veículo não será concluída integralmente pela plataforma. O sistema atuará como meio para iniciar a negociação (proposta comercial qualificada), enquanto o fechamento será realizado posteriormente com um vendedor ou presencialmente na concessionária física.
- **Camada de Implementação:** `Application` (`PropostaService`).
- **Comportamento:** O endpoint `POST /api/v1/propostas` gera um protocolo de atendimento comercial, envia dados do cliente e veículo para a fila da equipe comercial da respectiva loja, sem gateway de pagamento de cartão ou cobrança online para a aquisição integral do automóvel.

#### RN07 — Responsabilidade administrativa segmentada
- **Regra:** O cadastro e a atualização dos veículos serão realizados pelos setores responsáveis definidos pela empresa, principalmente Comercial e Logística.
- **Camada de Implementação:** `Api` (Filtros de autorização `[Authorize(Roles = "...")]`) e `Application` (gravação de histórico com `UsuarioId`).
- **Comportamento:**
  - O perfil `Comercial` pode alterar preço, descrição e aprovar propostas/reservas.
  - O perfil `Logistica` pode transferir veículo entre lojas físicas (`UnidadeId`) e atualizar dados de transporte/avarias.
  - O perfil `Administrador` tem acesso total irrestrito.
  - Todas as modificações geram registros na tabela `VeiculoHistoricoAlteracoes`.

#### RN08 — Consistência dos agendamentos e capacidade física
- **Regra:** O sistema não deverá permitir que a quantidade de agendamentos ultrapasse a disponibilidade real dos veículos ou a capacidade de atendimento da unidade física.
- **Camada de Implementação:** `Application` (`AgendamentoService`).
- **Comportamento:** Para visitas presenciais gerais, a loja possui um teto de atendimentos simultâneos por hora (ex: máximo de 5 clientes por faixa horária por loja). Para test-drives, a relação é estritamente 1 para 1 com o veículo específico.

---

## 9. AUTENTICAÇÃO, AUTORIZAÇÃO E SEGURANÇA

### 9.1 Matriz de Perfis e Permissões (RBAC)

O sistema adota o modelo de Controle de Acesso Baseado em Perfis (Role-Based Access Control - RBAC), diferenciando clientes anônimos da equipe interna:

| Recurso / Funcionalidade | Cliente (Público) | Setor Comercial | Setor de Logística | Administrador |
| :--- | :---: | :---: | :---: | :---: |
| Consultar catálogo de veículos e detalhes |  |  |  |  |
| Consultar catálogo de peças |  |  |  |  |
| Solicitar propostas, contatos e dúvidas |  |  |  |  |
| Solicitar reservas de veículos |  |  |  |  |
| Solicitar agendamentos de visita/test-drive |  |  |  |  |
| Cadastrar e editar veículos | ❌ |  (Comercial) |  (Logística) |  |
| Alterar situação de veículos | ❌ |  |  |  |
| Fazer upload de fotos de veículos | ❌ |  |  |  |
| Visualizar histórico de alterações do veículo | ❌ |  |  |  |
| Gerenciar e aprovar reservas | ❌ |  | ❌ |  |
| Gerenciar agendamentos de test-drives | ❌ |  |  |  |
| Gerenciar e responder contatos/propostas | ❌ |  | ❌ |  |
| Cadastrar e atualizar estoque de peças | ❌ | ❌ |  |  |
| Cadastrar e gerenciar lojas físicas (Unidades) | ❌ | ❌ | ❌ |  |
| Visualizar indicadores e KPIs da Área Gerencial | ❌ |  |  |  |
| Gerenciar usuários internos e permissões | ❌ | ❌ | ❌ |  |

### 9.2 Implementação do Fluxo JWT

1. **Credenciais e Hash:** Senhas de colaboradores são armazenadas no banco exclusivamente sob a forma de hash utilizando o algoritmo **BCrypt** com fator de trabalho (work factor) configurado para 11.
2. **Payload do Token JWT:**
   - `sub`: Identificador do usuário (`Guid`).
   - `email`: E-mail corporativo.
   - `role`: Perfil do colaborador (`Administrador`, `Comercial`, `Logistica`).
   - `unidadeId`: Identificador da loja vinculada (quando aplicável).
   - `exp`: Prazo de validade do token (configurado para 8 horas de expediente).
3. **Validação no Pipeline ASP.NET Core:**
   Configuração nativa no `Program.cs`:
   ```csharp
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = builder.Configuration["Jwt:Issuer"],
               ValidAudience = builder.Configuration["Jwt:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(
                   Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
               ClockSkew = TimeSpan.Zero
           };
       });
   ```

### 9.3 Proteção de Dados Pessoais (LGPD) e Resiliência (RNF09, RNF04)

1. **Minimização de Coleta:** O cliente na fase de navegação pública não precisa criar conta nem fornecer dados bancários sensíveis (RV04). Para reservas e propostas, coletam-se apenas nome, e-mail, telefone e CPF estritamente para identificação do lead comercial.
2. **Tratamento Seguro de Erros:** Respostas padronizadas via RFC 7807 (`ProblemDetails`). Stack traces nunca são expostos em ambiente de produção.
3. **Prevenção de Injeções:** O uso de Entity Framework Core com consultas parametrizadas garante proteção nativa total contra SQL Injection.
4. **Rate Limiting:** Implementação de limitador de requisições nativo do ASP.NET Core (`Microsoft.AspNetCore.RateLimiting`) nos endpoints públicos de reserva, propostas e contatos para mitigar abusos, bots e ataques de negação de serviço (DoS).

---

## 10. PLANO DE IMPLEMENTAÇÃO POR FASES

O plano de execução é organizado em **14 fases sequenciais e incrementais**, garantindo que a base de infraestrutura e domínio esteja estável antes da construção dos casos de uso, e que os módulos críticos de concorrência (reservas e agendamentos) possuam suporte completo.

---

### Fase 1: Preparação do Ambiente e Criação da Solução
- **Objetivo:** Estabelecer a estrutura inicial do repositório, solução .NET e orquestração Docker.
- **Funcionalidades Envolvidas:** Configuração de build, Docker Compose para PostgreSQL, `.gitignore`, `.editorconfig`.
- **Tarefas Técnicas:**
  1. Criar a solução .NET `StarConcessionaria.sln`.
  2. Criar os projetos de biblioteca de classes e API: `StarConcessionaria.Domain`, `StarConcessionaria.Application`, `StarConcessionaria.Infrastructure`, `StarConcessionaria.Api`.
  3. Criar os projetos de teste: `StarConcessionaria.UnitTests` e `StarConcessionaria.IntegrationTests`.
  4. Configurar as referências de projeto respeitando as camadas da Clean Architecture.
  5. Configurar `docker-compose.yml` contendo serviço `postgres:16-alpine` com volume persistente e variáveis de ambiente.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.sln`
  - Arquivos `.csproj` de cada projeto
  - `docker-compose.yml`
  - `Dockerfile`
- **Dependências:** Nenhuma.
- **Critérios de Conclusão:** Solução compila com `dotnet build` sem erros; container do PostgreSQL sobe e aceita conexões na porta 5432.
- **Testes:** Teste de compilação da solução e script de verificação de conectividade do banco.

---

### Fase 2: Arquitetura Base, Middleware e Configuração Inicial
- **Objetivo:** Configurar o pipeline HTTP da API, injeção de dependências e tratamento global de erros.
- **Funcionalidades Envolvidas:** Middleware de exceções RFC 7807, OpenAPI / Scalar, logging com Serilog ou ILogger estruturado.
- **Tarefas Técnicas:**
  1. Implementar `GlobalExceptionHandlerMiddleware` capturando `DomainException`, `ValidationException` e exceções não tratadas.
  2. Configurar OpenAPI com anotações de documentação Swagger.
  3. Configurar injeção de dependências modular (`DependencyInjection.cs` em cada camada).
  4. Configurar leitura tipada de configurações (`appsettings.json`).
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Api/Middlewares/GlobalExceptionHandlerMiddleware.cs`
  - `StarConcessionaria.Api/Extensions/DependencyInjection.cs`
  - `StarConcessionaria.Application/Common/ApiResponse.cs`
  - `StarConcessionaria.Domain/Exceptions/DomainException.cs`
- **Dependências:** Fase 1 concluída.
- **Critérios de Conclusão:** Endpoint de healthcheck `/health` responde `200 OK`; chamadas com erro disparam respostas em formato JSON estruturado com status code correto.
- **Testes:** Teste unitário do middleware de exceções com simulação de erro de domínio.

---

### Fase 3: Banco de Dados, Mapeamento EF Core e Migrations
- **Objetivo:** Mapear todas as entidades no Entity Framework Core e executar a primeira migração no PostgreSQL.
- **Funcionalidades Envolvidas:** `ApplicationDbContext`, mapeamentos via Fluent API, migrações versionadas, Seeder de dados iniciais.
- **Tarefas Técnicas:**
  1. Adicionar pacotes `Npgsql.EntityFrameworkCore.PostgreSQL` e `Microsoft.EntityFrameworkCore.Design`.
  2. Criar classes de configuração (`IEntityTypeConfiguration<T>`) para todas as 12 entidades mapeadas.
  3. Implementar `ApplicationDbContext` com interceptor para atualizar `DataAtualizacao` automaticamente.
  4. Gerar a primeira migração (`InitialCreate`) via `dotnet ef migrations add`.
  5. Implementar `DatabaseSeeder` para carga de lojas físicas de exemplo e usuário administrador padrão.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Infrastructure/Persistence/ApplicationDbContext.cs`
  - Classes em `Configurations/*Configuration.cs`
  - `DatabaseSeeder.cs`
  - Pasta `Migrations/`
- **Dependências:** Fases 1 e 2.
- **Critérios de Conclusão:** Migração aplicada com sucesso no PostgreSQL via `dotnet ef database update`; tabelas e restrições criadas no banco.
- **Testes:** Teste de integração inicial verificando conexão e schema do banco via Testcontainers.

---

### Fase 4: Módulo de Unidades Físicas (Lojas)
- **Objetivo:** Fornecer CRUD de lojas físicas para que veículos e agendamentos possam ser associados a elas.
- **Funcionalidades Envolvidas:** Consulta de lojas para o cliente (RF19), cadastro de lojas pelo administrador.
- **Tarefas Técnicas:**
  1. Implementar entidade de domínio `Unidade` com validações cadastrais.
  2. Criar repositório `IUnidadeRepository` e implementação em EF Core.
  3. Implementar `UnidadeService` com DTOs de listagem e criação.
  4. Criar `UnidadesController` com endpoints públicos de listagem e administrativos protegidos.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Domain/Entities/Unidade.cs`
  - `StarConcessionaria.Application/Services/UnidadeService.cs`
  - `StarConcessionaria.Api/Controllers/UnidadesController.cs`
- **Dependências:** Fase 3 concluída.
- **Critérios de Conclusão:** É possível cadastrar uma unidade física e listá-la via GET `/api/v1/unidades`.
- **Testes:** Testes unitários de validação de unidade; teste de integração do endpoint GET e POST.

---

### Fase 5: Módulo de Veículos — Cadastro e Gestão Administrativa
- **Objetivo:** Implementar o gerenciamento completo do estoque de veículos por setores autorizados.
- **Funcionalidades Envolvidas:** Cadastro de novos e seminovos (RF01, RF02), atualização de dados (RF03), controle de situação (RF04), upload de imagens e histórico de alterações (RN07).
- **Tarefas Técnicas:**
  1. Implementar entidade `Veiculo` com métodos de transição de estado (`AtualizarDados`, `AlterarSituacao`, `VincularImagem`).
  2. Implementar registro de auditoria na entidade `VeiculoHistoricoAlteracao`.
  3. Criar `IVeiculoRepository` com consultas otimizadas.
  4. Implementar `VeiculoService` e validações com FluentValidation (`CriarVeiculoRequestValidator`).
  5. Implementar endpoints no `VeiculosController` para criação, edição, alteração de situação e imagens.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Domain/Entities/Veiculo.cs`
  - `StarConcessionaria.Domain/Entities/VeiculoImagem.cs`
  - `StarConcessionaria.Domain/Entities/VeiculoHistoricoAlteracao.cs`
  - `StarConcessionaria.Application/Services/VeiculoService.cs`
  - `StarConcessionaria.Api/Controllers/VeiculosController.cs`
- **Dependências:** Fase 4 (Unidades) concluída.
- **Critérios de Conclusão:** Cadastro de veículos funcionando com validação de campos obrigatórios, vinculação à loja física e registro de histórico.
- **Testes:** Testes unitários para regras de transição de situação do veículo e testes de validação de DTOs.

---

### Fase 6: Consulta Pública de Veículos e Filtros Simultâneos
- **Objetivo:** Entregar o catálogo aberto de veículos para o portal do cliente com alta performance de busca.
- **Funcionalidades Envolvidas:** Catálogo público (RF05), pesquisa por marca, modelo, ano, preço (RF06), filtros simultâneos (RF07), página de detalhes e localização (RF08, RF20).
- **Tarefas Técnicas:**
  1. Construir query dinâmica com EF Core (`IQueryable`) aplicando filtros condicionais e paginação (`PagedResult`).
  2. Implementar projeção direta para `VeiculoCatalogoResponse` e `VeiculoDetalheResponse`.
  3. Garantir que apenas veículos com `Ativo = true` e situação relevante sejam exibidos nas listagens públicas.
  4. Implementar endpoint GET `/api/v1/veiculos` e GET `/api/v1/veiculos/{id}`.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Application/DTOs/Veiculos/FiltroVeiculosParameters.cs`
  - `StarConcessionaria.Application/DTOs/Veiculos/VeiculoCatalogoResponse.cs`
  - Implementação de busca em `VeiculoRepository.cs`
- **Dependências:** Fase 5 concluída.
- **Critérios de Conclusão:** Busca pública filtrando simultaneamente por múltiplos parâmetros retornando status 200 em menos de 100ms.
- **Testes:** Testes unitários testando diferentes combinações de filtros; teste de integração cobrindo paginação.

---

### Fase 7: Clientes, Contatos e Propostas Comerciais
- **Objetivo:** Viabilizar os canais de interação do cliente para manifestar interesse de compra e tirar dúvidas.
- **Funcionalidades Envolvidas:** Solicitação de informações (RF09), solicitação de proposta comercial (RF10), contato com vendedor (RF17), início do processo de compra (RF16, RN06), formas de pagamento (RF18) e gestão de contatos (RF25).
- **Tarefas Técnicas:**
  1. Criar entidades `PropostaComercial` e `Contato`.
  2. Implementar `PropostaService` para captura de dados do lead, validação de forma de pagamento aceita (à vista, consórcio, financiamento).
  3. Implementar `ContatoService` para registro de dúvidas gerais e mensagens a vendedores.
  4. Criar controllers `PropostasController` e `ContatosController`.
  5. Implementar endpoint estático/informativo de formas de pagamento aceitas.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Domain/Entities/PropostaComercial.cs`
  - `StarConcessionaria.Domain/Entities/Contato.cs`
  - `StarConcessionaria.Application/Services/PropostaService.cs`
  - `StarConcessionaria.Application/Services/ContatoService.cs`
  - `StarConcessionaria.Api/Controllers/PropostasController.cs`
  - `StarConcessionaria.Api/Controllers/ContatosController.cs`
- **Dependências:** Fases 5 e 6 concluídas.
- **Critérios de Conclusão:** Cliente consegue enviar uma proposta comercial e um contato; registros gravados no banco e consultáveis pela administração.
- **Testes:** Testes unitários de validação dos dados de proposta e testes de integração dos endpoints.

---

### Fase 8: Reservas de Veículos e Prevenção de Concorrência
- **Objetivo:** Implementar o fluxo de reservas com proteção absoluta contra reservas duplicadas e verificação de situação.
- **Funcionalidades Envolvidas:** Solicitação de reserva (RF11), validação dupla de disponibilidade (RF15), gestão administrativa de reservas (RF26), regras RN01 e RN03.
- **Tarefas Técnicas:**
  1. Implementar entidade de domínio `Reserva` com ciclo de vida (Pendente, Confirmada, Cancelada, Expirada).
  2. Implementar método transacional com bloqueio atômico em `ReservaService`.
  3. Aplicar alteração de situação do veículo para `Reservado` quando a reserva é aprovada/solicitada.
  4. Implementar endpoints no `ReservasController`.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Domain/Entities/Reserva.cs`
  - `StarConcessionaria.Application/Services/ReservaService.cs`
  - `StarConcessionaria.Api/Controllers/ReservasController.cs`
- **Dependências:** Fase 5 (Veículos) concluída.
- **Critérios de Conclusão:** Duas requisições simultâneas para o mesmo veículo não conseguem reservar simultaneamente (uma sucede, a outra recebe 409 Conflict).
- **Testes:** Testes unitários de regras de reserva; **teste de integração de concorrência massiva** simulando chamadas paralelas com `Task.WhenAll`.

---

### Fase 9: Agendamentos de Visitas e Test-Drives
- **Objetivo:** Disponibilizar o agendamento de visitas presenciais e test-drives sem conflito de agenda física.
- **Funcionalidades Envolvidas:** Agendamento de visita (RF12), agendamento de test-drive (RF13), controle de horários disponíveis (RF14), validação de disponibilidade (RF15), gestão administrativa (RF27), regras RN02, RN04 e RN08.
- **Tarefas Técnicas:**
  1. Criar entidade `Agendamento` com cálculo de janela de horário (`PeriodoAgendamento`).
  2. Implementar consulta de horários disponíveis considerando a grade da loja e test-drives já marcados no veículo.
  3. Implementar validação impeditiva para veículos com situação `Vendido` (RN02).
  4. Criar endpoints em `AgendamentosController`.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Domain/Entities/Agendamento.cs`
  - `StarConcessionaria.Domain/ValueObjects/PeriodoAgendamento.cs`
  - `StarConcessionaria.Application/Services/AgendamentoService.cs`
  - `StarConcessionaria.Api/Controllers/AgendamentosController.cs`
- **Dependências:** Fases 4, 5 e 8.
- **Critérios de Conclusão:** Não é permitido agendar dois test-drives para o mesmo veículo no mesmo horário; agendamento bloqueado para veículos vendidos.
- **Testes:** Testes unitários para algoritmo de sobreposição de horários; testes de integração para endpoints de agendamento.

---

### Fase 10: Módulo de Peças Automotivas
- **Objetivo:** Fornecer o catálogo e controle de disponibilidade de peças e acessórios.
- **Funcionalidades Envolvidas:** Cadastro de peças (RF21, RF22), controle de disponibilidade e estoque (RF23), suporte a futuras vendas.
- **Tarefas Técnicas:**
  1. Implementar entidades `Peca` e `PecaCategoria`.
  2. Criar `IPecaRepository` com buscas textuais por nome, fabricante ou modelos compatíveis.
  3. Implementar `PecaService` com atualização de saldo em estoque e contador de saídas.
  4. Criar `PecasController` com catálogo público e endpoints administrativos protegidos para logística.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Domain/Entities/Peca.cs`
  - `StarConcessionaria.Domain/Entities/PecaCategoria.cs`
  - `StarConcessionaria.Application/Services/PecaService.cs`
  - `StarConcessionaria.Api/Controllers/PecasController.cs`
- **Dependências:** Fase 3 concluída.
- **Critérios de Conclusão:** Peças cadastradas são pesquisáveis publicamente e seu estoque é atualizado pelos usuários autorizados.
- **Testes:** Testes unitários de validação de estoque e preços; testes de integração para listagem pública de peças.

---

### Fase 11: Autenticação, Perfis de Usuário e Segurança (RBAC)
- **Objetivo:** Proteger todos os endpoints administrativos com autenticação JWT e autorização por perfis.
- **Funcionalidades Envolvidas:** Autenticação administrativa (RF24, RNF04), papéis de Administrador, Comercial e Logística (RN07).
- **Tarefas Técnicas:**
  1. Implementar entidade `Usuario` com hash BCrypt.
  2. Implementar `TokenService` gerando claims de id, e-mail e perfil.
  3. Configurar autenticação JWT no pipeline e decorar controllers com atributos `[Authorize(Roles = "...")]`.
  4. Criar `AuthController` com endpoint de login `/api/v1/auth/login`.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Domain/Entities/Usuario.cs`
  - `StarConcessionaria.Infrastructure/Security/PasswordHasher.cs`
  - `StarConcessionaria.Infrastructure/Security/TokenService.cs`
  - `StarConcessionaria.Api/Controllers/AuthController.cs`
- **Dependências:** Fase 3 e fases de negócio correspondentes.
- **Critérios de Conclusão:** Tentativa de acesso sem token retorna 401 Unauthorized; acesso com token de perfil não autorizado retorna 403 Forbidden.
- **Testes:** Testes unitários de geração e validação de tokens JWT; testes de integração de segurança em endpoints protegidos.

---

### Fase 12: Administração, Vendas e Indicadores Gerenciais (Dashboard)
- **Objetivo:** Fornecer à diretoria e gerência visão consolidada da operação e acompanhamento de vendas.
- **Funcionalidades Envolvidas:** Acompanhamento de vendas registradas (RF28), indicadores consolidados (RF29), catálogo de Dúvidas Frequentes (RF30).
- **Tarefas Técnicas:**
  1. Implementar entidade `Venda` vinculada a veículo, loja e vendedor.
  2. Implementar `DashboardService` calculando KPIs agregados: contagem de veículos disponíveis por loja, total de contatos recebidos, reservas em andamento, total de vendas e ranking de peças com maior saída.
  3. Implementar entidade e controller de `FaqsController` (RF30).
  4. Criar `DashboardController` e `VendasController`.
- **Arquivos/Componentes a Criar:**
  - `StarConcessionaria.Domain/Entities/Venda.cs`
  - `StarConcessionaria.Domain/Entities/Faq.cs`
  - `StarConcessionaria.Application/Services/DashboardService.cs`
  - `StarConcessionaria.Api/Controllers/DashboardController.cs`
  - `StarConcessionaria.Api/Controllers/VendasController.cs`
  - `StarConcessionaria.Api/Controllers/FaqsController.cs`
- **Dependências:** Todas as fases de negócio anteriores (4 a 11).
- **Critérios de Conclusão:** Dashboard retorna indicadores consolidados corretos condizentes com os dados persistidos no banco.
- **Testes:** Testes unitários dos cálculos agregados do dashboard e testes de integração com queries analíticas no PostgreSQL.

---

### Fase 13: Testes de Carga, Segurança e Refinamento de Documentação
- **Objetivo:** Validar requisitos não funcionais de performance, segurança da informação e documentação técnica interativa.
- **Funcionalidades Envolvidas:** Conformidade RNF03 (Desempenho), RNF05 (Integridade), RNF09 (Proteção de dados), documentação Swagger/Scalar.
- **Tarefas Técnicas:**
  1. Executar testes de concorrência com cenários de alta carga em reservas e agendamentos.
  2. Verificar headers de segurança (HSTS, Content-Type-Options, X-Frame-Options).
  3. Revisar documentação Swagger com exemplos completos de requisição e resposta para todos os endpoints.
  4. Ajustar rate limiting para prevenção de abusos.
- **Arquivos/Componentes a Criar:**
  - Suites de testes de carga e concorrência em `StarConcessionaria.IntegrationTests`.
- **Dependências:** Fases 1 a 12 concluídas.
- **Critérios de Conclusão:** 100% dos testes automatizados passando; Swagger gerando especificação OpenAPI 3.0 válida sem pendências.
- **Testes:** Bateria completa de testes automatizados com cobertura superior a 80% das regras de negócio.

---

### Fase 14: Preparação para Publicação e Deploy
- **Objetivo:** Empacotar a aplicação em imagem de container Docker otimizada pronta para produção.
- **Funcionalidades Envolvidas:** Conteinerização multi-stage, variáveis de ambiente seguras, healthchecks de orquestração.
- **Tarefas Técnicas:**
  1. Construir `Dockerfile` multi-stage com imagem base `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` para tamanho reduzido e alta segurança.
  2. Configurar scripts de inicialização com aplicação de migrations automática opcional em container de inicialização (`entrypoint.sh`).
  3. Validar execução completa da stack via `docker compose up --build`.
- **Arquivos/Componentes a Criar:**
  - `Dockerfile` definitivo
  - `docker-compose.prod.yml`
- **Dependências:** Fase 13 concluída.
- **Critérios de Conclusão:** Aplicação e banco de dados sobem via Docker sem intervenção manual, passando nos healthchecks HTTP.
- **Testes:** Teste E2E de inicialização em ambiente de container isolado.

---

## 11. ESTRATÉGIA DE TESTES

A garantia de qualidade da API é estruturada no conceito de **Pirâmide de Testes**, com ênfase em testes automatizados rápidos de unidade para regras de negócio e testes de integração com banco de dados real para validação transacional e de concorrência.

```
       /       /   \     E2E / Testes de Carga (Testcontainers, K6)
     /-----    /       \   Testes de Integração (WebApplicationFactory + PostgreSQL Real)
   /---------  /           \ Testes Unitários (xUnit + Moq + FluentAssertions: Domínio e Services)
 /-------------```

### 11.1 Tipos de Testes e Escopos

| Categoria | Foco e Escopo | Ferramentas | Exemplo Prático |
| :--- | :--- | :--- | :--- |
| **Testes Unitários de Domínio** | Regras puras de entidades e Value Objects (sem banco ou HTTP) | `xUnit`, `FluentAssertions` | Testar se `veiculo.AlterarSituacao(Vendido)` impede nova reserva e se transições inválidas disparam `DomainException`. |
| **Testes Unitários de Aplicação** | Orquestração dos casos de uso, mocks de repositórios | `xUnit`, `Moq` | Testar se `ReservaService` calcula corretamente a data de expiração da reserva e chama o repositório. |
| **Testes de Validação** | Validação de payloads e contratos de entrada | `FluentValidation.TestHelper` | Testar se `CriarVeiculoRequest` rejeita preço negativo ou placa em formato inválido. |
| **Testes de Integração de Endpoints** | Ciclo completo HTTP -> Controller -> Middleware -> Banco | `WebApplicationFactory<Program>`, `Testcontainers` | Enviar requisição POST para `/api/v1/veiculos` e validar retorno 201 e persistência no banco. |
| **Testes de Concorrência Crítica** | Transações concorrentes simulando múltiplos clientes simultâneos | `Task.WhenAll`, `Testcontainers` | Disparar 10 requisições simultâneas para reservar o mesmo veículo: exatamente 1 deve ter sucesso e 9 devem retornar 409 Conflict. |
| **Testes de Autenticação e RBAC** | Verificação de tokens JWT e políticas de perfil | `WebApplicationFactory` | Garantir que usuário com perfil `Logistica` receba 403 Forbidden ao tentar aprovar uma proposta comercial. |

### 11.2 Exemplo de Teste de Concorrência em Reservas (C# com xUnit)

```csharp
[Fact]
public async Task SolicitarReserva_MultiplasRequisicoesSimultaneas_DevePermitirApenasUmaReserva()
{
    // Arrange
    var client = _factory.CreateClient();
    var veiculoId = await CriarVeiculoDisponivelNoBancoAsync();

    var request = new SolicitarReservaRequest
    {
        VeiculoId = veiculoId,
        NomeCliente = "Cliente Teste",
        EmailCliente = "cliente@email.com",
        TelefoneCliente = "11999998888",
        CpfCliente = "12345678901"
    };

    int totalRequisicoesParalelas = 10;
    var tasks = new List<Task<HttpResponseMessage>>();

    // Act - Disparo simultâneo
    for (int i = 0; i < totalRequisicoesParalelas; i++)
    {
        tasks.Add(client.PostAsJsonAsync("/api/v1/reservas", request));
    }

    var responses = await Task.WhenAll(tasks);

    // Assert
    var reservasComSucesso = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
    var conflitos = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

    reservasComSucesso.Should().Be(1, "Apenas uma solicitação concorrente pode obter a reserva");
    conflitos.Should().Be(totalRequisicoesParalelas - 1, "Todas as demais devem receber conflito de disponibilidade");
}
```

---

## 12. REQUISITOS PENDENTES (VALIDAÇÃO COM O CLIENTE)

Durante o levantamento de requisitos do documento oficial, foram identificados 8 pontos que demandam validação futura pelo cliente (**RV01** a **RV08**). A tabela abaixo detalha o que precisa ser definido, seu impacto na arquitetura, a estratégia técnica recomendada para o MVP e se a decisão impede o início do projeto.

| Item | O que precisa ser definido | Impacto Técnico no Back-End | Estratégia Recomendada para o MVP | Impede Início do Projeto? |
| :--- | :--- | :--- | :--- | :---: |
| **RV01 — Venda de peças** | Definir se a compra de peças será concluída diretamente pela plataforma ou encaminhada para atendimento comercial/balcão. | Se for venda direta online, exigirá carrinho de compras, integração com gateway de pagamento (Stripe/MercadoPago) e cálculo de frete. Se for encaminhamento, exige apenas formulário de lead de peças. | Desenvolver a área de peças inicialmente como **catálogo consultivo com botão de "Tenho Interesse / Falar com Vendedor"**, mantendo as tabelas preparadas para receber campos de pedidos no futuro. | ❌ Não impede |
| **RV02 — Pagamento de peças** | Definir quais meios de pagamento específicos (PIX, Cartão de Crédito, Boleto) serão suportados caso haja checkout online. | Seleção do SDK de pagamentos, geração de webhooks assíncronos e conciliação bancária. | Adiar a integração de gateway. No catálogo, expor apenas o preço de tabela e as formas aceitas nas lojas físicas. | ❌ Não impede |
| **RV03 — Entrega de peças** | Definir se haverá envio via transportadora/Correios, retirada exclusiva na concessionária ou ambas as opções. | Integração com APIs de frete (Melhor Envio / Correios) por CEP ou rotas de agendamento de retirada em loja específica. | Implementar na modelagem a coluna `PermiteRetiradaEmLoja = true` e adiar integrações com APIs de transportadoras. | ❌ Não impede |
| **RV04 — Cadastro dos clientes** | Definir se o cliente final é obrigado a possuir login/senha para realizar reservas, propostas ou agendar test-drives. | Implementação de Identity/cadastro público de clientes com confirmação de e-mail e recuperação de senha. | **Permitir solicitação anônima informando dados básicos de contato (Nome, E-mail, Telefone, CPF)** com rate limiting. O modelo não é bloqueante e reduz drasticamente a fricção do lead comercial. | ❌ Não impede |
| **RV05 — Tempo de reserva** | Definir o prazo padrão de expiração de uma reserva de veículo antes do cancelamento automático (ex: 24h, 48h, 72h). | Criação de job em background (HostedService / Worker com Quartz ou Hangfire) para expiração automática de reservas expiradas. | Configurar parâmetro flexível no `appsettings.json` (`Reservas:TempoExpiracaoHoras = 48`) e criar um `BackgroundService` simples que roda a cada hora cancelando reservas vencidas. | ❌ Não impede |
| **RV06 — Cancelamento de reservas e agendamentos** | Definir regras de antecedência mínima para cancelamento e se o próprio cliente pode cancelar via link/token de e-mail ou apenas a loja. | Geração de token assinado no e-mail de confirmação para cancelamento direto pelo cliente (`/api/v1/reservas/{token}/cancelar`). | Disponibilizar no MVP o cancelamento operacional pela Área Administrativa (Comercial) e preparar o endpoint para receber cancelamento com token assinado. | ❌ Não impede |
| **RV07 — Informações adicionais dos veículos** | Determinar quais dados técnicos extras (consumo urbano/rodoviário, potência em cv, torque, volume do porta-malas, lista detalhada de opcionais) devem ser armazenados. | Adição de colunas estruturadas ou coluna JSONB (`EspecificacoesTecnicas`) na tabela de veículos. | Adicionar uma coluna tipo `jsonb` no PostgreSQL (`PropriedadesAdicionais`) na entidade `Veiculo`, permitindo que novos atributos técnicos sejam armazenados dinamicamente sem necessidade de novas migrations. | ❌ Não impede |
| **RV08 — Comunicação cliente-vendedor** | Definir se a interação ocorrerá via link direto para WhatsApp, chat interno na plataforma ou e-mail corporativo. | Integração com API oficial do WhatsApp Business ou apenas geração de link `https://wa.me/...` dinâmico na API. | A API fornecerá no retorno da unidade física e do vendedor o número de telefone e o link sanitizado para abertura direta do WhatsApp, além de registrar o lead na base de dados. | ❌ Não impede |

---

## 13. BACKLOG TÉCNICO PRIORIZADO

O backlog é estruturado nas prioridades:
- **P0 (Essencial para funcionamento inicial):** Infraestrutura básica, persistência, cadastro e catálogo de veículos, propostas, reservas ativas e autenticação administrativa.
- **P1 (Importante para completar o escopo):** Agendamentos de test-drive, catálogo de peças, gestão operacional de contatos, indicadores gerenciais e histórico de alterações.
- **P2 (Melhorias e funcionalidades complementares):** Automação de expiração de reservas, FAQs com busca, otimizações adicionais de cache e métricas avançadas.

| ID | Descrição da Tarefa | Prioridade | Dependências | Critério de Aceite |
| :--- | :--- | :---: | :--- | :--- |
| **TASK-01** | Criar solução .NET 10, estrutura de 4 projetos e docker-compose com PostgreSQL | **P0** | Nenhuma | Build executa com sucesso; container PostgreSQL acessível e funcional. |
| **TASK-02** | Configurar ApplicationDbContext, Fluent API e migrations iniciais com tabelas core | **P0** | TASK-01 | Migrations aplicadas gerando todas as tabelas e índices no PostgreSQL. |
| **TASK-03** | Implementar autenticação JWT, usuários administrativos e perfis (RBAC) com BCrypt | **P0** | TASK-02 | Login gera token JWT com claims de role; endpoints protegidos retornam 401/403. |
| **TASK-04** | Implementar CRUD de Lojas Físicas (Unidades) com endereço e contato | **P0** | TASK-02, TASK-03 | Listagem pública de lojas funcionando e cadastro protegido para Admin. |
| **TASK-05** | Implementar entidade Veículo, controle de situação e repositório com EF Core | **P0** | TASK-04 | Veículo cadastrado com associação obrigatória a uma unidade física. |
| **TASK-06** | Implementar catálogo público de veículos com filtros múltiplos simultâneos e paginação | **P0** | TASK-05 | Endpoint GET `/api/v1/veiculos` filtra simultaneamente por marca, modelo, ano e preço. |
| **TASK-07** | Implementar upload e gerenciamento de fotos do veículo com controle de imagem de capa | **P0** | TASK-05 | Imagens associadas ao veículo; apenas uma foto com flag `Principal = true`. |
| **TASK-08** | Implementar módulo de propostas comerciais e início de compra de veículos (RN06) | **P0** | TASK-05 | Envio de proposta gera protocolo comercial com forma de pagamento informada. |
| **TASK-09** | Implementar solicitação e gestão de reservas com bloqueio atômico de concorrência (RN01, RN03) | **P0** | TASK-05 | Veículo só aceita reserva se disponível; duas requisições paralelas não duplicam reserva. |
| **TASK-10** | Implementar agendamento de visitas a unidades físicas e cálculo de horários disponíveis | **P1** | TASK-04 | Consulta de horários da loja respeita capacidade máxima de agendamentos simultâneos. |
| **TASK-11** | Implementar agendamento de test-drive com validação de veículo e regras de conflito (RN04, RN08) | **P1** | TASK-05, TASK-10 | Agendamento rejeitado caso veículo esteja em test-drive no mesmo horário ou vendido. |
| **TASK-12** | Implementar catálogo e gestão de peças automotivas, categorias e saldo em estoque | **P1** | TASK-02, TASK-03 | Peças catalogadas por categoria com consulta pública de disponibilidade. |
| **TASK-13** | Implementar módulo de contatos, Fale Conosco e encaminhamento para vendedores | **P1** | TASK-04, TASK-05 | Mensagens registradas e listadas no painel administrativo para resposta comercial. |
| **TASK-14** | Implementar registro de vendas de veículos e acompanhamento comercial | **P1** | TASK-05, TASK-08 | Venda registrada atualiza situação do veículo para `Vendido` e cancela reservas ativas. |
| **TASK-15** | Implementar painel de indicadores administrativos e dashboard gerencial (RF29) | **P1** | TASK-05, TASK-09, TASK-12, TASK-14 | Retorna KPIs de veículos disponíveis por loja, reservas, contatos e peças mais vendidas. |
| **TASK-16** | Implementar trilha de auditoria e histórico de alterações em veículos (RN07) | **P1** | TASK-05 | Qualquer modificação em dados de veículo gera log com usuário, valor antigo e novo. |
| **TASK-17** | Implementar endpoint institucional de formas de pagamento aceitas e FAQ (RF18, RF30) | **P2** | TASK-02 | Endpoints informativos respondendo perguntas frequentes e regras de financiamento. |
| **TASK-18** | Implementar Worker em background para expiração automática de reservas vencidas | **P2** | TASK-09 | Reservas sem confirmação no prazo configurado passam para status `Expirada`. |
| **TASK-19** | Implementar rate limiting e headers de segurança HTTP (OWASP) | **P2** | TASK-01 | Proteção contra força bruta em login e sobrecarga de requisições públicas. |
| **TASK-20** | Configurar Dockerfile multi-stage de produção e scripts de deploy contínuo | **P2** | TASK-01 | Imagem otimizada compilada e testada em ambiente conteinerizado. |

---

## 14. PADRÕES DE DESENVOLVIMENTO

Para garantir coerência, legibilidade e manutenibilidade entre os desenvolvedores, os seguintes padrões devem ser rigorosamente seguidos:

### 14.1 Nomenclatura e Convenções
- **Projetos, Namespaces e Classes:** PascalCase (ex: `VeiculoService`, `ApplicationDbContext`, `ReservaResponse`).
- **Interfaces:** PascalCase prefixado com `I` (ex: `IVeiculoRepository`, `ITokenService`).
- **Métodos e Propriedades:** PascalCase (ex: `ObterPorIdAsync`, `DataCriacao`).
- **Parâmetros e Variáveis Locais:** camelCase (ex: `veiculoId`, `filtroParameters`).
- **Campos Privados Injetados:** camelCase prefixado com `_` (ex: `_veiculoRepository`, `_unitOfWork`).
- **Rotas de Endpoints REST:** kebab-case ou lowercase no plural (ex: `/api/v1/veiculos`, `/api/v1/agendamentos/test-drive`).

### 14.2 Padrão de Entidade de Domínio Rica (Sem Propriedades Anêmicas)

```csharp
namespace StarConcessionaria.Domain.Entities;

public class Veiculo : BaseEntity
{
    public Guid UnidadeId { get; private set; }
    public string Marca { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int AnoFabricacao { get; private set; }
    public int AnoModelo { get; private set; }
    public decimal Preco { get; private set; }
    public SituacaoVeiculo Situacao { get; private set; }
    public int VersaoConcorrencia { get; private set; }

    // Construtor protegido para o EF Core
    protected Veiculo() { }

    public Veiculo(Guid unidadeId, string marca, string modelo, int anoFabricacao, int anoModelo, decimal preco)
    {
        if (preco <= 0)
            throw new DomainException("O preço do veículo deve ser estritamente maior que zero.");

        Id = Guid.NewGuid();
        UnidadeId = unidadeId;
        Marca = marca;
        Modelo = modelo;
        AnoFabricacao = anoFabricacao;
        AnoModelo = anoModelo;
        Preco = preco;
        Situacao = SituacaoVeiculo.Disponivel;
        VersaoConcorrencia = 1;
        DataCriacao = DateTime.UtcNow;
    }

    public void Reservar()
    {
        if (Situacao != SituacaoVeiculo.Disponivel)
            throw new VeiculoIndisponivelException("O veículo não está disponível para receber reservas.");

        Situacao = SituacaoVeiculo.Reservado;
        VersaoConcorrencia++;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void MarcarComoVendido()
    {
        Situacao = SituacaoVeiculo.Vendido;
        VersaoConcorrencia++;
        DataAtualizacao = DateTime.UtcNow;
    }
}
```

### 14.3 Padrão de Resposta HTTP e Paginação Padronizada

```csharp
namespace StarConcessionaria.Application.Common;

public class ApiResponse<T>
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public T? Dados { get; set; }
    public IEnumerable<string>? Erros { get; set; }

    public static ApiResponse<T> Ok(T dados, string? mensagem = null) =>
        new() { Sucesso = true, Dados = dados, Mensagem = mensagem };

    public static ApiResponse<T> Falha(string erro) =>
        new() { Sucesso = false, Erros = new[] { erro } };

    public static ApiResponse<T> Falha(IEnumerable<string> erros) =>
        new() { Sucesso = false, Erros = erros };
}

public class PagedResult<T>
{
    public IEnumerable<T> Itens { get; set; } = Enumerable.Empty<T>();
    public int PaginaAtual { get; set; }
    public int ItensPorPagina { get; set; }
    public int TotalItens { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItens / ItensPorPagina);
}
```

### 14.4 Padrão de Controller RESTful

```csharp
namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _veiculoService;

    public VeiculosController(IVeiculoService veiculoService)
    {
        _veiculoService = veiculoService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VeiculoCatalogoResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterCatalogo([FromQuery] FiltroVeiculosParameters filtros, CancellationToken ct)
    {
        var resultado = await _veiculoService.ObterCatalogoAsync(filtros, ct);
        return Ok(ApiResponse<PagedResult<VeiculoCatalogoResponse>>.Ok(resultado));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Comercial,Logistica")]
    [ProducesResponseType(typeof(ApiResponse<VeiculoDetalheResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarVeiculoRequest request, CancellationToken ct)
    {
        var veiculo = await _veiculoService.CriarVeiculoAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = veiculo.Id }, ApiResponse<VeiculoDetalheResponse>.Ok(veiculo));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<VeiculoDetalheResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var veiculo = await _veiculoService.ObterPorIdAsync(id, ct);
        if (veiculo == null) return NotFound();
        return Ok(ApiResponse<VeiculoDetalheResponse>.Ok(veiculo));
    }
}
```

### 14.5 Padrão de Validação com FluentValidation

```csharp
namespace StarConcessionaria.Application.Validators;

public class CriarVeiculoRequestValidator : AbstractValidator<CriarVeiculoRequest>
{
    public CriarVeiculoRequestValidator()
    {
        RuleFor(x => x.Marca)
            .NotEmpty().WithMessage("A marca do veículo é obrigatória.")
            .MaximumLength(80).WithMessage("A marca não pode exceder 80 caracteres.");

        RuleFor(x => x.Modelo)
            .NotEmpty().WithMessage("O modelo do veículo é obrigatório.")
            .MaximumLength(100).WithMessage("O modelo não pode exceder 100 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("O preço deve ser superior a zero.");

        RuleFor(x => x.AnoFabricacao)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage("Ano de fabricação inválido.");

        RuleFor(x => x.UnidadeId)
            .NotEmpty().WithMessage("A identificação da loja física (UnidadeId) é obrigatória.");
    }
}
```

---

## 15. CRITÉRIOS DE CONCLUSÃO DO PROJETO

A conclusão e entrega da API da Star Concessionária serão homologadas mediante o cumprimento rigoroso dos seguintes critérios objetivos:

1. **Atendimento Integral aos Requisitos:** Todos os 30 Requisitos Funcionais (RF01 a RF30) e 8 Regras de Negócio (RN01 a RN08) testados e operacionais.
2. **Integridade de Concorrência Comprovada:** Testes automatizados de concorrência demonstrando a impossibilidade de reservas simultâneas do mesmo veículo e sobreposição de test-drives.
3. **Segurança e RBAC:** Proteção total dos endpoints administrativos contra acessos anônimos ou perfis incompatíveis, com geração de tokens JWT válidos e senhas criptografadas em BCrypt.
4. **Isolamento Arquitetural:** Domínio e Casos de Uso totalmente desacoplados de frameworks de persistência e apresentação, com cobertura de testes unitários superior a 80%.
5. **Documentação Viva:** Documentação OpenAPI/Swagger 100% atualizada, contendo schemas, descrições e exemplos claros de requisição e resposta para todos os endpoints.
6. **Deploy Reprodutível via Docker:** Solução completa (API + PostgreSQL) inicializável via `docker compose up` sem falhas ou intervenção manual de banco.
7. **Tratamento de Erros Padronizado:** Nenhuma exceção não tratada retornando stack trace HTTP 500 desformatado; conformidade com a especificação RFC 7807 (Problem Details).
