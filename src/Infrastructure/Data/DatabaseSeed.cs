using StarConcessionaria.Application.Services;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Repositories;

namespace StarConcessionaria.Infrastructure.Data;

/// <summary>
/// Seed de dados iniciais para validação, demonstração e testes da plataforma Star Concessionária.
/// </summary>
public static class DatabaseSeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var usuarioRepo = services.GetRequiredService<IUsuarioRepository>();
        var unidadeRepo = services.GetRequiredService<IUnidadeRepository>();
        var veiculoRepo = services.GetRequiredService<IVeiculoRepository>();
        var pecaRepo = services.GetRequiredService<IPecaRepository>();
        var faqRepo = services.GetRequiredService<IFaqRepository>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        // 1. Cadastrar Unidades Físicas (Lojas)
        var unidadeSP = new Unidade(
            id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            nome: "Star Concessionária - Matriz Jardins (SP)",
            endereco: "Av. Europa, 1500 - Jardins",
            cidade: "São Paulo",
            estado: "SP",
            telefone: "(11) 3456-7890",
            horarioFuncionamento: "Seg a Sex: 08:00 às 19:00 | Sáb: 09:00 às 16:00"
        );

        var unidadeCampinas = new Unidade(
            id: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            nome: "Star Concessionária - Filial Campinas",
            endereco: "Av. José de Souza Campos, 800 - Cambuí",
            cidade: "Campinas",
            estado: "SP",
            telefone: "(19) 3210-4567",
            horarioFuncionamento: "Seg a Sex: 08:30 às 18:30 | Sáb: 09:00 às 14:00"
        );

        var unidadeSantos = new Unidade(
            id: Guid.Parse("33333333-3333-3333-3333-333333333333"),
            nome: "Star Concessionária - Filial Baixada Santista",
            endereco: "Av. Ana Costa, 350 - Gonzaga",
            cidade: "Santos",
            estado: "SP",
            telefone: "(13) 3890-1234",
            horarioFuncionamento: "Seg a Sex: 08:00 às 18:00 | Sáb: 09:00 às 13:00"
        );

        if (!await unidadeRepo.ExisteAsync(unidadeSP.Id))
        {
            await unidadeRepo.AdicionarAsync(unidadeSP);
            await unidadeRepo.AdicionarAsync(unidadeCampinas);
            await unidadeRepo.AdicionarAsync(unidadeSantos);
        }

        // 2. Cadastrar Usuários nos 4 perfis RBAC
        if (await usuarioRepo.ObterPorEmailAsync("admin@starconcessionaria.com.br") == null)
        {
            await usuarioRepo.AdicionarAsync(new Usuario(
                id: Guid.NewGuid(),
                nome: "Administrador Geral",
                email: "admin@starconcessionaria.com.br",
                senhaHash: passwordHasher.HashSenha("Admin@123"),
                perfil: PerfilUsuario.ADMINISTRADOR
            ));

            await usuarioRepo.AdicionarAsync(new Usuario(
                id: Guid.NewGuid(),
                nome: "Carlos Vendedor (Comercial)",
                email: "comercial@starconcessionaria.com.br",
                senhaHash: passwordHasher.HashSenha("Comercial@123"),
                perfil: PerfilUsuario.COMERCIAL
            ));

            await usuarioRepo.AdicionarAsync(new Usuario(
                id: Guid.NewGuid(),
                nome: "Mariana Silva (Logística)",
                email: "logistica@starconcessionaria.com.br",
                senhaHash: passwordHasher.HashSenha("Logistica@123"),
                perfil: PerfilUsuario.LOGISTICA
            ));

            await usuarioRepo.AdicionarAsync(new Usuario(
                id: Guid.NewGuid(),
                nome: "João Cliente",
                email: "cliente@gmail.com",
                senhaHash: passwordHasher.HashSenha("Cliente@123"),
                perfil: PerfilUsuario.CLIENTE
            ));
        }

        // 3. Cadastrar Veículos de Demonstração (Disponíveis, Reservados, Vendidos)
        var veiculosExistentes = await veiculoRepo.ListarAsync();
        if (!veiculosExistentes.Any())
        {
            await veiculoRepo.AdicionarAsync(new Veiculo(
                id: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                marca: "Toyota",
                modelo: "Corolla Cross XRE 2.0",
                ano: 2024,
                preco: 178900.00m,
                quilometragem: 12500,
                cor: "Branco Pérola",
                fotografias: new List<string>
                {
                    "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?auto=format&fit=crop&w=1200&q=80",
                    "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?auto=format&fit=crop&w=1200&q=80"
                },
                alteracoes: "Único dono, revisões na concessionária autorizada, garantia de fábrica até 2029.",
                unidadeId: unidadeSP.Id,
                situacao: SituacaoVeiculo.DISPONIVEL
            ));

            await veiculoRepo.AdicionarAsync(new Veiculo(
                id: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                marca: "Honda",
                modelo: "Civic Touring 1.5 Turbo",
                ano: 2023,
                preco: 219000.00m,
                quilometragem: 24000,
                cor: "Cinza Metálico",
                fotografias: new List<string>
                {
                    "https://images.unsplash.com/photo-1606016159991-dfe4f2746ad5?auto=format&fit=crop&w=1200&q=80"
                },
                alteracoes: "Teto solar panorâmico, vitrificação de pintura recém realizada.",
                unidadeId: unidadeCampinas.Id,
                situacao: SituacaoVeiculo.DISPONIVEL
            ));

            await veiculoRepo.AdicionarAsync(new Veiculo(
                id: Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                marca: "BMW",
                modelo: "320i M Sport 2.0 Turbo",
                ano: 2024,
                preco: 325000.00m,
                quilometragem: 8000,
                cor: "Azul Portimao",
                fotografias: new List<string>
                {
                    "https://images.unsplash.com/photo-1555215695-3004980ad54e?auto=format&fit=crop&w=1200&q=80"
                },
                alteracoes: "Pacote M Sport completo, som Harman Kardon, IPVA 2026 integralmente pago.",
                unidadeId: unidadeSP.Id,
                situacao: SituacaoVeiculo.RESERVADO
            ));

            await veiculoRepo.AdicionarAsync(new Veiculo(
                id: Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                marca: "Volkswagen",
                modelo: "Taos Highline 250 TSI",
                ano: 2023,
                preco: 165000.00m,
                quilometragem: 31000,
                cor: "Preto Ninja",
                fotografias: new List<string>
                {
                    "https://images.unsplash.com/photo-1549399542-7e3f8b79c341?auto=format&fit=crop&w=1200&q=80"
                },
                alteracoes: "Venda concluída e entregue ao cliente presencialmente.",
                unidadeId: unidadeSantos.Id,
                situacao: SituacaoVeiculo.VENDIDO
            ));
        }

        // 4. Cadastrar Peças e Acessórios
        var pecasExistentes = await pecaRepo.ListarAsync();
        if (!pecasExistentes.Any())
        {
            await pecaRepo.AdicionarAsync(new Peca(
                id: Guid.NewGuid(),
                nome: "Pastilha de Freio Dianteira Cerâmica",
                codigo: "PST-FR-001",
                preco: 389.90m,
                quantidadeEstoque: 24,
                descricao: "Jogo de pastilhas de freio dianteiras de alta performance para linha sedã e SUV."
            ));

            await pecaRepo.AdicionarAsync(new Peca(
                id: Guid.NewGuid(),
                nome: "Filtro de Óleo Sintético Original",
                codigo: "FLT-OL-045",
                preco: 65.00m,
                quantidadeEstoque: 50,
                descricao: "Filtro de óleo blindado de alta retenção de partículas para motores turbo e aspirados."
            ));

            await pecaRepo.AdicionarAsync(new Peca(
                id: Guid.NewGuid(),
                nome: "Amortecedor Dianteiro Pressurizado",
                codigo: "AMT-DT-882",
                preco: 720.00m,
                quantidadeEstoque: 8,
                descricao: "Amortecedor a gás nitrogênio pressurizado, garantindo estabilidade e conforto superior."
            ));
        }

        // 5. Cadastrar Perguntas Frequentes (FAQs)
        var faqsExistentes = await faqRepo.ListarTodasAsync();
        if (!faqsExistentes.Any())
        {
            await faqRepo.AdicionarAsync(new Faq(
                id: Guid.NewGuid(),
                pergunta: "Como funciona a reserva online de um veículo?",
                resposta: "Ao solicitar a reserva de um veículo disponível no portal, ele é bloqueado provisoriamente em nosso estoque. Nossa equipe comercial entrará em contato em até 2 horas úteis para formalizar a proposta e agendar a visita presencial.",
                categoria: "Reservas",
                ordemExibicao: 1
            ));

            await faqRepo.AdicionarAsync(new Faq(
                id: Guid.NewGuid(),
                pergunta: "A compra do veículo é concluída 100% online?",
                resposta: "Não. A plataforma web serve para consulta de estoque, agendamento de visitas/test-drives, reservas e envio de propostas comerciais. O fechamento do contrato, vistoria presencial e entrega das chaves são feitos exclusivamente na loja física com o vendedor.",
                categoria: "Compra e Atendimento",
                ordemExibicao: 2
            ));

            await faqRepo.AdicionarAsync(new Faq(
                id: Guid.NewGuid(),
                pergunta: "Quais são os requisitos para realizar um Test-Drive?",
                resposta: "Para realizar o test-drive, é necessário ter mais de 18 anos, CNH válida na categoria B com no mínimo 2 anos de emissão, e realizar o agendamento prévio pelo sistema escolhendo a loja onde o carro está localizado.",
                categoria: "Test-Drive",
                ordemExibicao: 3
            ));

            await faqRepo.AdicionarAsync(new Faq(
                id: Guid.NewGuid(),
                pergunta: "Quais são as formas de pagamento aceitas?",
                resposta: "Trabalhamos com pagamento à vista via TED/PIX, consórcios contemplados de todas as administradoras e financiamento bancário em até 60x através de bancos parceiros credenciados.",
                categoria: "Financiamento",
                ordemExibicao: 4
            ));
        }
    }
}
