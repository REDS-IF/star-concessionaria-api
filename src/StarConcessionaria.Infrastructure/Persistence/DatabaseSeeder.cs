using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Infrastructure.Persistence;

namespace StarConcessionaria.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    /// <summary>
    /// Executa o seed de dados iniciais para lojas, veiculos, pecas, clientes e agendamentos/contatos
    /// apenas uma única vez (se as lojas ou veículos já existirem, não executa novamente).
    /// Não toca na tabela de usuários.
    /// </summary>
    public static async Task SeedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        // 1. Verificação de idempotência: se já existirem lojas ou veículos cadastrados, não duplica.
        if (await db.Lojas.AnyAsync() || await db.Veiculos.AnyAsync())
        {
            return;
        }

        logger.LogInformation("Iniciando seed inicial do banco de dados (Lojas, Veículos, Peças, Clientes)...");

        // 2. Lojas
        var lojaMatriz = new Loja
        {
            Nome = "Star Concessionária - Matriz São Paulo",
            Telefone = "1130031000",
            Cep = "01452000",
            Endereco = "Av. Brigadeiro Faria Lima",
            Numero = "2000",
            Bairro = "Pinheiros",
            Cidade = "São Paulo",
            Estado = "SP",
            Latitude = -23.5687m,
            Longitude = -46.6934m
        };

        var lojaCampinas = new Loja
        {
            Nome = "Star Concessionária - Filial Campinas",
            Telefone = "1932345000",
            Cep = "13010111",
            Endereco = "Av. Francisco Glicério",
            Numero = "1200",
            Bairro = "Centro",
            Cidade = "Campinas",
            Estado = "SP",
            Latitude = -22.9056m,
            Longitude = -47.0608m
        };

        db.Lojas.AddRange(lojaMatriz, lojaCampinas);
        await db.SaveChangesAsync();

        // 3. Clientes
        var cliente1 = new Cliente
        {
            Nome = "Carlos Eduardo Silva",
            Email = "carlos.silva@email.com",
            Telefone = "11987654321"
        };
        var cliente2 = new Cliente
        {
            Nome = "Mariana Albuquerque",
            Email = "mariana.albuquerque@email.com",
            Telefone = "19976543210"
        };
        var cliente3 = new Cliente
        {
            Nome = "Rafael Mendonça",
            Email = "rafael.mendonca@email.com",
            Telefone = "11912345678"
        };

        db.Clientes.AddRange(cliente1, cliente2, cliente3);
        await db.SaveChangesAsync();

        // 4. Peças
        var pecas = new List<Peca>
        {
            new() { Nome = "Pastilha de Freio Dianteira", Descricao = "Jogo pastilhas cerâmica alta performance", Preco = 280.00m, QuantidadeEstoque = 35 },
            new() { Nome = "Filtro de Óleo Sintético", Descricao = "Filtro blindado para motores turbo modernos", Preco = 65.50m, QuantidadeEstoque = 80 },
            new() { Nome = "Amortecedor Dianteiro Pressurizado", Descricao = "Par de amortecedores a gás com batente", Preco = 890.00m, QuantidadeEstoque = 15 },
            new() { Nome = "Bateria Automotiva 60Ah Selada", Descricao = "Bateria 12V 60Ah livre de manutenção com 24 meses garantia", Preco = 520.00m, QuantidadeEstoque = 20 },
            new() { Nome = "Pneu 205/55 R16", Descricao = "Pneu radial alta durabilidade e aderência em pista molhada", Preco = 450.00m, QuantidadeEstoque = 40 }
        };
        db.Pecas.AddRange(pecas);
        await db.SaveChangesAsync();

        // 5. Veículos
        var v1 = new Veiculo
        {
            LojaId = lojaMatriz.Id,
            Tipo = "SUV",
            Marca = "Toyota",
            Modelo = "Corolla Cross XRX Hybrid",
            Ano = 2024,
            Preco = 189900.00m,
            Quilometragem = 12500,
            Cor = "Branco Perolizado",
            Descricao = "Único dono, todas revisões na concessionária, teto solar e pacote Toyota Safety Sense completo.",
            Situacao = SituacaoVeiculo.Disponivel
        };

        var v2 = new Veiculo
        {
            LojaId = lojaMatriz.Id,
            Tipo = "Sedan",
            Marca = "Honda",
            Modelo = "Civic Touring 1.5 Turbo",
            Ano = 2023,
            Preco = 159900.00m,
            Quilometragem = 28000,
            Cor = "Preto Cristal",
            Descricao = "Motor turbo de 173cv, bancos em couro claro, som premium e câmera de ponto cego.",
            Situacao = SituacaoVeiculo.Disponivel
        };

        var v3 = new Veiculo
        {
            LojaId = lojaCampinas.Id,
            Tipo = "Hatch",
            Marca = "Volkswagen",
            Modelo = "Polo GTS 1.4 TSI",
            Ano = 2024,
            Preco = 138000.00m,
            Quilometragem = 8500,
            Cor = "Cinza Platinum",
            Descricao = "Faróis IQ.Light Matrix, painel 100% digital active info display e rodas aro 18.",
            Situacao = SituacaoVeiculo.Disponivel
        };

        var v4 = new Veiculo
        {
            LojaId = lojaCampinas.Id,
            Tipo = "Picape",
            Marca = "Ford",
            Modelo = "Ranger Limited 3.0 V6 4x4",
            Ano = 2024,
            Preco = 299900.00m,
            Quilometragem = 15000,
            Cor = "Azul Belize",
            Descricao = "Tração 4x4 integral com reduzida, piloto automático adaptativo e capota marítima retrátil elétrica.",
            Situacao = SituacaoVeiculo.Disponivel
        };

        db.Veiculos.AddRange(v1, v2, v3, v4);
        await db.SaveChangesAsync();

        // 6. Fotos dos Veículos
        var fotos = new List<FotoVeiculo>
        {
            new() { VeiculoId = v1.Id, Url = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341?auto=format&fit=crop&w=800&q=80", Principal = true, Ordem = 1 },
            new() { VeiculoId = v1.Id, Url = "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=800&q=80", Principal = false, Ordem = 2 },
            new() { VeiculoId = v2.Id, Url = "https://images.unsplash.com/photo-1590362891991-f776e747a588?auto=format&fit=crop&w=800&q=80", Principal = true, Ordem = 1 },
            new() { VeiculoId = v3.Id, Url = "https://images.unsplash.com/photo-1541899481282-d53bffe3c35d?auto=format&fit=crop&w=800&q=80", Principal = true, Ordem = 1 },
            new() { VeiculoId = v4.Id, Url = "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?auto=format&fit=crop&w=800&q=80", Principal = true, Ordem = 1 }
        };
        db.FotosVeiculo.AddRange(fotos);
        await db.SaveChangesAsync();

        // 7. Contatos de Exemplo
        var contatos = new List<Contato>
        {
            new()
            {
                ClienteId = cliente1.Id,
                VeiculoId = v1.Id,
                LojaId = lojaMatriz.Id,
                Tipo = "Interesse em Veículo",
                Mensagem = "Olá, tenho interesse no Corolla Cross! Gostaria de saber mais sobre a avaliação de usado na troca.",
                Data = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new()
            {
                ClienteId = cliente2.Id,
                VeiculoId = v3.Id,
                LojaId = lojaCampinas.Id,
                Tipo = "Test Drive",
                Mensagem = "Gostaria de agendar um test-drive no Polo GTS para o próximo sábado.",
                Data = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };
        db.Contatos.AddRange(contatos);
        await db.SaveChangesAsync();

        logger.LogInformation("Seed inicial de dados executado com sucesso!");
    }
}
