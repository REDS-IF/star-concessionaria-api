using Microsoft.EntityFrameworkCore;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Infrastructure.Security;

namespace StarConcessionaria.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(StarConcessionariaDbContext context, IPasswordHasher hasher)
    {
        // 1. Seed Lojas
        if (!await context.Lojas.AnyAsync())
        {
            var lojaMatriz = new Loja
            {
                NomeLoj = "Star Concessionária - Matriz Campinas",
                TelefoneLoj = "(19) 3344-5500",
                CepLoj = "13080-000",
                EnderecoLoj = "Av. José de Souza Campos",
                NumeroLoj = "1500",
                BairroLoj = "Cambuí",
                CidadeLoj = "Campinas",
                EstadoLoj = "SP",
                LatitudeLoj = -22.8989000m,
                LongitudeLoj = -47.0543000m
            };

            var lojaFilial = new Loja
            {
                NomeLoj = "Star Concessionária - São Paulo Sul",
                TelefoneLoj = "(11) 5055-9000",
                CepLoj = "04538-132",
                EnderecoLoj = "Av. Brigadeiro Faria Lima",
                NumeroLoj = "3500",
                BairroLoj = "Itaim Bibi",
                CidadeLoj = "São Paulo",
                EstadoLoj = "SP",
                LatitudeLoj = -23.5855000m,
                LongitudeLoj = -46.6811000m
            };

            context.Lojas.AddRange(lojaMatriz, lojaFilial);
            await context.SaveChangesAsync();
        }

        // 2. Seed Usuários
        if (!await context.Usuarios.AnyAsync())
        {
            var admin = new UsuarioSistema
            {
                NomeUsu = "Administrador Geral",
                EmailUsu = "admin@starconcessionaria.com.br",
                SenhaUsu = hasher.Hash("Admin@2026"),
                PerfilUsu = PerfilUsuario.Administrador
            };

            var vendedor = new UsuarioSistema
            {
                NomeUsu = "Carlos Vendedor",
                EmailUsu = "vendas@starconcessionaria.com.br",
                SenhaUsu = hasher.Hash("Vendas@2026"),
                PerfilUsu = PerfilUsuario.Comercial
            };

            var logistica = new UsuarioSistema
            {
                NomeUsu = "Mariana Logística",
                EmailUsu = "logistica@starconcessionaria.com.br",
                SenhaUsu = hasher.Hash("Log@2026"),
                PerfilUsu = PerfilUsuario.Logistica
            };

            context.Usuarios.AddRange(admin, vendedor, logistica);
            await context.SaveChangesAsync();
        }

        // 3. Seed Formas de Pagamento (RF18)
        if (!await context.FormasPagamento.AnyAsync())
        {
            context.FormasPagamento.AddRange(
                new FormaPagamento { NomeFp = "Pagamento à vista" },
                new FormaPagamento { NomeFp = "Consórcio" },
                new FormaPagamento { NomeFp = "Financiamento" }
            );
            await context.SaveChangesAsync();
        }

        // 4. Seed Dúvidas Frequentes (RF30)
        if (!await context.DuvidasFrequentes.AnyAsync())
        {
            context.DuvidasFrequentes.AddRange(
                new DuvidaFrequente
                {
                    PerguntaDf = "Como funciona a reserva de veículos pela plataforma?",
                    RespostaDf = "Você pode solicitar a reserva diretamente no site. O veículo fica bloqueado temporariamente enquanto nosso consultor comercial entra em contato para prosseguir com a documentação."
                },
                new DuvidaFrequente
                {
                    PerguntaDf = "A compra do veículo é concluída integralmente pelo site?",
                    RespostaDf = "Não. A plataforma web serve para você encontrar o veículo ideal, simular condições e iniciar a negociação. O fechamento e a entrega ocorrem presencialmente com nossos consultores."
                },
                new DuvidaFrequente
                {
                    PerguntaDf = "O que é necessário para agendar um test-drive?",
                    RespostaDf = "Basta selecionar a loja, o veículo desejado, informar sua CNH válida e o horário de sua preferência."
                }
            );
            await context.SaveChangesAsync();
        }

        // 5. Seed Veículos Iniciais
        if (!await context.Veiculos.AnyAsync())
        {
            var loja = await context.Lojas.FirstAsync();
            var veiculo1 = new Veiculo
            {
                IdLojFk = loja.IdLoj,
                TipoVei = TipoVeiculo.Seminovo,
                MarcaVei = "Toyota",
                ModeloVei = "Corolla 2.0 Altis Premium Flex",
                AnoVei = 2024,
                PrecoVei = 158900.00m,
                QuilometragemVei = 18500,
                CorVei = "Prata",
                DescricaoVei = "Único dono, revisões na concessionária, teto solar e bancos em couro.",
                SituacaoVei = SituacaoVeiculo.Disponivel
            };

            var veiculo2 = new Veiculo
            {
                IdLojFk = loja.IdLoj,
                TipoVei = TipoVeiculo.Novo,
                MarcaVei = "Volkswagen",
                ModeloVei = "Nivus Highline 200 TSI",
                AnoVei = 2026,
                PrecoVei = 142000.00m,
                QuilometragemVei = 0,
                CorVei = "Cinza Moonstone",
                DescricaoVei = "0km pronta entrega, painel digital Active Info Display, rodas aro 17.",
                SituacaoVei = SituacaoVeiculo.Disponivel
            };

            context.Veiculos.AddRange(veiculo1, veiculo2);
            await context.SaveChangesAsync();
        }
    }
}
