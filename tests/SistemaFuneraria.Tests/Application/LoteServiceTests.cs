using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.DTOs.Lotes;
using SistemaFuneraria.Application.Servicios.Lotes;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Application;

/// <summary>
/// Pruebas unitarias para ILoteService y LoteService (SF-041).
/// Valida cálculo de KPIs, filtros paginados y operaciones CRUD administrativas.
/// </summary>
[Trait("Ticket", "SF-041")]
[Trait("Capa", "Application")]
[Trait("Tipo", "Unit")]
public class LoteServiceTests
{
    private static AppDbContext CrearContextoEnMemoria()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"TestLotesDb_{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }

    private static async Task PoblarCatalogoMockupAsync(AppDbContext context)
    {
        var lotes = new[]
        {
            Lote.Crear("JN-A-042", "Jardín Norte", "Andador A · Fila 4", 2, new Dinero(120000m)),
            Lote.Crear("JN-A-043", "Jardín Norte", "Andador A · Fila 4", 2, new Dinero(120000m)),
            Lote.Crear("JC-C-118", "Jardín Central", "Andador C · Fila 11", 3, new Dinero(150000m)),
            Lote.Crear("RO-B-027", "Robles", "Andador B · Fila 2", 4, new Dinero(180000m)),
            Lote.Crear("LM-A-014", "Las Magnolias", "Andador A · Fila 1", 2, new Dinero(120000m))
        };

        // Estado inicial de mockup: RO-B-027 Reservado, LM-A-014 Vendido
        lotes[3].Reservar();
        lotes[4].MarcarVendido();

        await context.Lotes.AddRangeAsync(lotes);
        await context.SaveChangesAsync();
    }

    #region 1. Catálogo Paginado y KPIs de Resumen

    [Fact(DisplayName = "SF-041: Cálculo de KPIs de cabecera en catálogo completo")]
    public async Task ObtenerCatalogoAsync_ConLotesMultiples_DebeCalcularKpisCorrectamente()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        await PoblarCatalogoMockupAsync(context);
        var service = new LoteService(context);

        // Act
        var resultado = await service.ObtenerCatalogoAsync();

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        var resumen = resultado.Valor;
        resumen.TotalLotes.Should().Be(5);
        resumen.TotalDisponibles.Should().Be(3);
        resumen.PrecioDesde.Should().Be(120000m);
        resumen.SeccionMasEspacio.Should().Be("Jardín Norte");
        resumen.LotesLibresSeccionMasEspacio.Should().Be(2);
        resumen.Items.Should().HaveCount(5);
    }

    [Fact(DisplayName = "SF-041 (BDD): Cálculo de KPIs y filtrado por sección y estado (CP-01)")]
    public async Task ObtenerCatalogoAsync_FiltroSeccionYEstado_DebeRetornarLotesFiltrados()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        await PoblarCatalogoMockupAsync(context);
        var service = new LoteService(context);

        // Act
        var resultado = await service.ObtenerCatalogoAsync(seccion: "Jardín Norte", estado: "Disponible");

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        var resumen = resultado.Valor;
        resumen.Items.Should().HaveCount(2);
        resumen.Items.Should().OnlyContain(l => l.Seccion == "Jardín Norte" && l.Estado == "Disponible");
        resumen.TotalDisponibles.Should().Be(3); // KPIs globales del catálogo
        resumen.PrecioDesde.Should().Be(120000m);
        resumen.SeccionMasEspacio.Should().Be("Jardín Norte");
    }

    [Fact(DisplayName = "SF-041: Búsqueda reactiva por texto libre en clave o gavetas")]
    public async Task ObtenerCatalogoAsync_FiltroBuscar_DebeRetornarCoincidencias()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        await PoblarCatalogoMockupAsync(context);
        var service = new LoteService(context);

        // Act
        var resultado = await service.ObtenerCatalogoAsync(buscar: "4 gavetas");

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        var resumen = resultado.Valor;
        resumen.Items.Should().ContainSingle();
        resumen.Items[0].Clave.Should().Be("RO-B-027");
        resumen.Items[0].Gavetas.Should().Be(4);
    }

    [Fact(DisplayName = "SF-041: Catálogo sin lotes disponibles debe retornar PrecioDesde = 0m")]
    public async Task ObtenerCatalogoAsync_SinLotesDisponibles_DebeRetornarPrecioDesdeCero()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var lote = Lote.Crear("LM-A-014", "Las Magnolias", "Andador A · Fila 1", 2, new Dinero(120000m));
        lote.MarcarVendido();
        await context.Lotes.AddAsync(lote);
        await context.SaveChangesAsync();

        var service = new LoteService(context);

        // Act
        var resultado = await service.ObtenerCatalogoAsync();

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        var resumen = resultado.Valor;
        resumen.TotalDisponibles.Should().Be(0);
        resumen.PrecioDesde.Should().Be(0m);
    }

    #endregion

    #region 2. Consultas por Identificador y Clave

    [Fact(DisplayName = "SF-041: ObtenerPorId con lote existente debe retornar DTO")]
    public async Task ObtenerPorIdAsync_LoteExistente_DebeRetornarExitoConDto()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var lote = Lote.Crear("JN-A-042", "Jardín Norte", "Andador A · Fila 4", 2, new Dinero(120000m));
        await context.Lotes.AddAsync(lote);
        await context.SaveChangesAsync();

        var service = new LoteService(context);

        // Act
        var resultado = await service.ObtenerPorIdAsync(lote.Id);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Clave.Should().Be("JN-A-042");
        resultado.Valor.PrecioContado.Should().Be(120000m);
    }

    [Fact(DisplayName = "SF-041: ObtenerPorId inexistente debe retornar [NO_ENCONTRADO]")]
    public async Task ObtenerPorIdAsync_LoteInexistente_DebeRetornarFalloNoEncontrado()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new LoteService(context);

        // Act
        var resultado = await service.ObtenerPorIdAsync(Guid.NewGuid());

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().StartWith("[NO_ENCONTRADO]");
    }

    #endregion

    #region 3. CRUD Administrativo de Lotes

    [Fact(DisplayName = "SF-041: CrearLoteAsync con clave única debe registrar y retornar Id")]
    public async Task CrearLoteAsync_ConClaveUnica_DebeRegistrarLoteYRetornarId()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new LoteService(context);
        var command = new CrearLoteCommand("JN-B-010", "Jardín Norte", "Andador B · Fila 1", 3, 150000m);

        // Act
        var resultado = await service.CrearLoteAsync(command);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Should().NotBeEmpty();

        var loteEnDb = await context.Lotes.FindAsync(resultado.Valor);
        loteEnDb.Should().NotBeNull();
        loteEnDb!.Clave.Should().Be("JN-B-010");
        loteEnDb.Estado.Should().Be(EstadoLote.Disponible);
    }

    [Fact(DisplayName = "SF-041: CrearLoteAsync con clave duplicada debe retornar [CONFLICTO]")]
    public async Task CrearLoteAsync_ConClaveDuplicada_DebeRetornarConflicto()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var existente = Lote.Crear("JN-A-042", "Jardín Norte", "Andador A · Fila 4", 2, new Dinero(120000m));
        await context.Lotes.AddAsync(existente);
        await context.SaveChangesAsync();

        var service = new LoteService(context);
        var command = new CrearLoteCommand("JN-A-042", "Jardín Central", "Andador C", 2, 120000m);

        // Act
        var resultado = await service.CrearLoteAsync(command);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().StartWith("[CONFLICTO]");
    }

    [Fact(DisplayName = "SF-041: ActualizarLoteAsync actualiza capacidad, precio RN-01 y estado manteniendo ubicación inmutable")]
    public async Task ActualizarLoteAsync_ConDatosValidos_DebeActualizarCampos()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var lote = Lote.Crear("JN-A-042", "Jardín Norte", "Andador A · Fila 4", 2, new Dinero(120000m));
        await context.Lotes.AddAsync(lote);
        await context.SaveChangesAsync();

        var service = new LoteService(context);
        var command = new ActualizarLoteCommand("Robles", "Andador B · Fila 9", 3, 150000m, EstadoLote.Reservado);

        // Act
        var resultado = await service.ActualizarLoteAsync(lote.Id, command);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        var actualizado = await context.Lotes.FindAsync(lote.Id);
        actualizado!.Seccion.Should().Be("Jardín Norte");
        actualizado.AndadorFila.Should().Be("Andador A · Fila 4");
        actualizado.Gavetas.Should().Be(3);
        actualizado.PrecioContado.Monto.Should().Be(150000m);
        actualizado.Estado.Should().Be(EstadoLote.Reservado);
    }

    [Fact(DisplayName = "Generador secuencial: ObtenerSiguienteUbicacionAsync calcula bloques 1-based de 10 lotes/fila y 100/andador")]
    public async Task ObtenerSiguienteUbicacionAsync_DebeCalcularBloques1BasedYSaltarColisiones()
    {
        using var context = CrearContextoEnMemoria();
        context.Lotes.AddRange(
            Lote.Crear("JN-D-065", "Jardín Norte", "Andador D · Fila 6", 3, new Dinero(150000m)),
            Lote.Crear("JC-A-100", "Jardín Central", "Andador A · Fila 10", 2, new Dinero(120000m)));
        await context.SaveChangesAsync();

        var service = new LoteService(context);

        var sigJn = await service.ObtenerSiguienteUbicacionAsync("Jardín Norte");
        sigJn.EsExitoso.Should().BeTrue();
        sigJn.Valor.Clave.Should().Be("JN-A-066");
        sigJn.Valor.Andador.Should().Be("A");
        sigJn.Valor.Fila.Should().Be(7);
        sigJn.Valor.AndadorFila.Should().Be("Andador A · Fila 7");

        var sigJc = await service.ObtenerSiguienteUbicacionAsync("Jardín Central");
        sigJc.EsExitoso.Should().BeTrue();
        sigJc.Valor.Clave.Should().Be("JC-B-101");
        sigJc.Valor.Andador.Should().Be("B");
        sigJc.Valor.Fila.Should().Be(1);
        sigJc.Valor.AndadorFila.Should().Be("Andador B · Fila 1");

        var sigLm = await service.ObtenerSiguienteUbicacionAsync("Las Magnolias");
        sigLm.EsExitoso.Should().BeTrue();
        sigLm.Valor.Clave.Should().Be("LM-A-001");
        sigLm.Valor.AndadorFila.Should().Be("Andador A · Fila 1");
    }

    [Fact(DisplayName = "Alta automática: CrearLoteAsync sin Clave ni AndadorFila genera ubicación y precio RN-01 automáticamente")]
    public async Task CrearLoteAsync_SinClave_DebeGenerarUbicacionYPrecioRn01()
    {
        using var context = CrearContextoEnMemoria();
        context.Lotes.Add(Lote.Crear("RO-A-010", "Robles", "Andador A · Fila 1", 2, new Dinero(120000m)));
        await context.SaveChangesAsync();

        var service = new LoteService(context);
        var command = new CrearLoteCommand(Clave: null, Seccion: "Robles", AndadorFila: null, Gavetas: 4, PrecioContado: 0m);

        var resultado = await service.CrearLoteAsync(command);

        resultado.EsExitoso.Should().BeTrue();
        var creado = await context.Lotes.FindAsync(resultado.Valor);
        creado.Should().NotBeNull();
        creado!.Clave.Should().Be("RO-A-011");
        creado.Seccion.Should().Be("Robles");
        creado.AndadorFila.Should().Be("Andador A · Fila 2");
        creado.Gavetas.Should().Be(4);
        creado.PrecioContado.Monto.Should().Be(180000m);
    }

    [Fact(DisplayName = "SF-041: CambiarEstadoLoteAsync cambia estado operativo del lote")]
    public async Task CambiarEstadoLoteAsync_TransicionValida_DebeCambiarEstado()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var lote = Lote.Crear("JN-A-042", "Jardín Norte", "Andador A · Fila 4", 2, new Dinero(120000m));
        await context.Lotes.AddAsync(lote);
        await context.SaveChangesAsync();

        var service = new LoteService(context);

        // Act
        var resultado = await service.CambiarEstadoLoteAsync(lote.Id, EstadoLote.Reservado);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        var actualizado = await context.Lotes.FindAsync(lote.Id);
        actualizado!.Estado.Should().Be(EstadoLote.Reservado);
    }

    #endregion
}
