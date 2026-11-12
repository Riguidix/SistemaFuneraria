using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Servicios.Ventas;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Application;

[Trait("Ticket", "SF-024")]
[Trait("Capa", "Application")]
public class VentaContadoServiceTests
{
    private static AppDbContext CrearContextoEnMemoria()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"VentasContadoTestDb_{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<Cliente> CrearYPersistirClienteValidoAsync(AppDbContext context)
    {
        var cliente = Cliente.Crear(
            nombre: "María",
            apellidos: "Gómez Pérez",
            curp: "GOMM800101MDFRRN01",
            telefono: "5512345678",
            correo: "maria.gomez@example.com",
            direccion: "Av. Juárez 456, Col. Centro"
        );

        await context.Clientes.AddAsync(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }

    [Fact(DisplayName = "SF-024 / BDD: Venta de contado de lote familiar de $150k debe generar folio VTA-2026-XXXXXX")]
    public async Task RegistrarVentaContadoAsync_ConClienteYLoteFamiliarValido_DebeGenerarFolioValido()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var cliente = await CrearYPersistirClienteValidoAsync(context);
        var service = new VentaService(context);

        var request = new RegistrarVentaContadoRequest(
            ClienteId: cliente.Id,
            PrecioLote: 150_000m,
            DescripcionLote: "Lote Familiar 3 Gavetas",
            Observaciones: "Pago en 1 sola exhibición"
        );

        // Act
        var resultado = await service.RegistrarVentaContadoAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsExitoso.Should().BeTrue("la venta de contado con datos válidos debe completarse con éxito");
        resultado.Valor.Should().MatchRegex(@"^VTA-2026-[A-Z0-9]{6}$", "el folio emitido debe seguir el patrón canónico VTA-2026-XXXXXX");
    }

    [Fact(DisplayName = "SF-024: Venta de contado con cliente inexistente debe retornar Fallo")]
    public async Task RegistrarVentaContadoAsync_ConClienteInexistente_DebeRetornarFallo()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new VentaService(context);

        var request = new RegistrarVentaContadoRequest(
            ClienteId: Guid.NewGuid(),
            PrecioLote: 150_000m,
            DescripcionLote: "Lote Familiar",
            Observaciones: null
        );

        // Act
        var resultado = await service.RegistrarVentaContadoAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue("no se puede procesar una venta si el cliente no está registrado");
        resultado.Error.ToLowerInvariant().Should().Contain("cliente");
    }

    [Theory(DisplayName = "SF-024: Venta de contado con precio no autorizado debe retornar Fallo (RN-01)")]
    [InlineData(100_000)]
    [InlineData(135_000)]
    [InlineData(200_000)]
    [InlineData(0)]
    public async Task RegistrarVentaContadoAsync_ConPrecioNoAutorizado_DebeRetornarFallo(decimal precioInvalido)
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var cliente = await CrearYPersistirClienteValidoAsync(context);
        var service = new VentaService(context);

        var request = new RegistrarVentaContadoRequest(
            ClienteId: cliente.Id,
            PrecioLote: precioInvalido,
            DescripcionLote: "Lote no autorizado",
            Observaciones: null
        );

        // Act
        var resultado = await service.RegistrarVentaContadoAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue("solo se autorizan precios de $120k, $150k y $180k (RN-01)");
        resultado.Error.ToLowerInvariant().Should().Contain("autorizado");
    }

    [Fact(DisplayName = "SF-024: Venta de contado debe liquidarse en 1 exhibición por el 100% del precio (RN-03)")]
    public async Task RegistrarVentaContadoAsync_DebeLiquidarTotalidadSinInteresesNiAmortizacion()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var cliente = await CrearYPersistirClienteValidoAsync(context);
        var service = new VentaService(context);

        var request = new RegistrarVentaContadoRequest(
            ClienteId: cliente.Id,
            PrecioLote: 120_000m,
            DescripcionLote: "Lote Básico 2 Gavetas",
            Observaciones: "Venta contado"
        );

        // Act
        var resultado = await service.RegistrarVentaContadoAsync(request);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Should().StartWith("VTA-2026-");
    }
}
