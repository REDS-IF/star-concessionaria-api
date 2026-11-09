using FluentAssertions;
using StarConcessionaria.Domain.Entities;
using StarConcessionaria.Domain.Enums;
using StarConcessionaria.Domain.Exceptions;
using Xunit;

namespace StarConcessionaria.UnitTests;

public class VeiculoDomainTests
{
    [Fact]
    public void Reservar_VeiculoDisponivel_DeveAlterarSituacaoParaReservado()
    {
        // Arrange
        var veiculo = new Veiculo
        {
            IdVei = 1,
            MarcaVei = "Toyota",
            ModeloVei = "Corolla",
            PrecoVei = 150000m,
            SituacaoVei = SituacaoVeiculo.Disponivel
        };

        // Act
        veiculo.Reservar();

        // Assert
        veiculo.SituacaoVei.Should().Be(SituacaoVeiculo.Reservado);
    }

    [Fact]
    public void Reservar_VeiculoJaReservado_DeveLancarVeiculoIndisponivelException()
    {
        // Arrange
        var veiculo = new Veiculo
        {
            IdVei = 1,
            MarcaVei = "Toyota",
            ModeloVei = "Corolla",
            PrecoVei = 150000m,
            SituacaoVei = SituacaoVeiculo.Reservado
        };

        // Act
        var act = () => veiculo.Reservar();

        // Assert
        act.Should().Throw<VeiculoIndisponivelException>()
            .WithMessage("*não aceita reservas*");
    }

    [Fact]
    public void Reservar_VeiculoVendido_DeveLancarVeiculoIndisponivelException()
    {
        // Arrange
        var veiculo = new Veiculo
        {
            IdVei = 1,
            MarcaVei = "Toyota",
            ModeloVei = "Corolla",
            PrecoVei = 150000m,
            SituacaoVei = SituacaoVeiculo.Vendido
        };

        // Act
        var act = () => veiculo.Reservar();

        // Assert
        act.Should().Throw<VeiculoIndisponivelException>();
    }

    [Fact]
    public void LiberarReserva_VeiculoReservado_DeveVoltarParaDisponivel()
    {
        // Arrange
        var veiculo = new Veiculo
        {
            IdVei = 1,
            SituacaoVei = SituacaoVeiculo.Reservado
        };

        // Act
        veiculo.LiberarReserva();

        // Assert
        veiculo.SituacaoVei.Should().Be(SituacaoVeiculo.Disponivel);
    }

    [Fact]
    public void MarcarComoVendido_DeveDefinirSituacaoVendido()
    {
        // Arrange
        var veiculo = new Veiculo
        {
            IdVei = 1,
            SituacaoVei = SituacaoVeiculo.Disponivel
        };

        // Act
        veiculo.MarcarComoVendido();

        // Assert
        veiculo.SituacaoVei.Should().Be(SituacaoVeiculo.Vendido);
    }
}
