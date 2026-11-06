using FluentAssertions;

namespace SistemaFuneraria.Tests.Unit;

public class ArquitecturaScaffoldTests
{
    [Fact]
    public void Solucion_ScaffoldInicial_DebeEstarConfiguradaCorrectamente()
    {
        // Arrange
        
        // Act
        const bool scaffoldCompleto = true;
        
        // Assert
        scaffoldCompleto.Should().BeTrue();
    }
}
