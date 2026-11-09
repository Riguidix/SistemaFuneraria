using System;
using System.Collections.Generic;
using FluentAssertions;
using SistemaFuneraria.Application.Comun;
using Xunit;

namespace SistemaFuneraria.Tests.Application;

public class ResultadoTests
{
    [Fact]
    public void Exito_DebeCrearResultadoExitoso_SinErrores()
    {
        // Act
        var resultado = Resultado.Exito();

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.EsFallo.Should().BeFalse();
        resultado.Error.Should().BeEmpty();
        resultado.Errores.Should().BeEmpty();
    }

    [Fact]
    public void Fallo_ConMensajeUnico_DebeCrearResultadoFallidoConError()
    {
        // Arrange
        const string errorEsperado = "Fondos insuficientes para el enganche.";

        // Act
        var resultado = Resultado.Fallo(errorEsperado);

        // Assert
        resultado.EsExitoso.Should().BeFalse();
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().Be(errorEsperado);
        resultado.Errores.Should().ContainSingle().Which.Should().Be(errorEsperado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Fallo_ConMensajeInvalido_DebeLanzarArgumentException(string? mensajeInvalido)
    {
        // Act
        Action act = () => Resultado.Fallo(mensajeInvalido!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("error");
    }

    [Fact]
    public void Fallo_ConListaDeErrores_DebePreservarColeccionYFormatearMensaje()
    {
        // Arrange
        var errores = new[] { "CURP inválida.", "El cliente ya cuenta con un crédito activo." };

        // Act
        var resultado = Resultado.Fallo(errores);

        // Assert
        resultado.EsExitoso.Should().BeFalse();
        resultado.EsFallo.Should().BeTrue();
        resultado.Errores.Should().HaveCount(2).And.ContainInOrder(errores);
        resultado.Error.Should().Be("CURP inválida.; El cliente ya cuenta con un crédito activo.");
    }

    [Fact]
    public void Fallo_ConColeccionNulaOVacia_DebeLanzarExcepcionApropiada()
    {
        // Act & Assert (Null)
        Action actNull = () => Resultado.Fallo((IEnumerable<string>)null!);
        actNull.Should().Throw<ArgumentNullException>();

        // Act & Assert (Vacía)
        Action actVacia = () => Resultado.Fallo(Array.Empty<string>());
        actVacia.Should().Throw<ArgumentException>()
            .WithParameterName("errores");

        // Act & Assert (Elementos en blanco)
        Action actEspacios = () => Resultado.Fallo(new[] { "  ", "\t" });
        actEspacios.Should().Throw<ArgumentException>()
            .WithParameterName("errores");
    }

    [Fact]
    public void ExitoTipado_DebeTransportarValorCorrectamente()
    {
        // Arrange
        const decimal valorEsperado = 125000.50m;

        // Act
        var resultado = Resultado<decimal>.Exito(valorEsperado);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.EsFallo.Should().BeFalse();
        resultado.Valor.Should().Be(valorEsperado);
        resultado.Error.Should().BeEmpty();
        resultado.Errores.Should().BeEmpty();
    }

    [Fact]
    public void FalloTipado_AlAccederValor_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var resultado = Resultado<string>.Fallo("Operación cancelada.");

        // Act
        Action act = () => _ = resultado.Valor;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No se puede acceder al valor de un resultado fallido.");
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().Be("Operación cancelada.");
    }

    [Fact]
    public void FalloTipado_ConMultiplesErrores_DebeRetenerColeccionYBloquearValor()
    {
        // Arrange
        var errores = new[] { "Lote no disponible", "Plazo fuera de rango" };

        // Act
        var resultado = Resultado<int>.Fallo(errores);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Errores.Should().HaveCount(2);
        resultado.Error.Should().Be("Lote no disponible; Plazo fuera de rango");

        Action act = () => _ = resultado.Valor;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConversionImplicita_DebeCrearResultadoExitoso()
    {
        // Act
        Resultado<int> resultado = 100;

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Should().Be(100);
    }
}
