using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Application;

/// <summary>
/// Pruebas unitarias para la ampliación CRUD de Cliente y resumen de cartera (SF-031).
/// </summary>
[Trait("Ticket", "SF-031")]
[Trait("Capa", "Application")]
[Trait("Tipo", "Unit")]
public class ClienteCrudCompletoTests
{
    private static AppDbContext CrearContextoEnMemoria()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"ClienteCrudTestDb_{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact(DisplayName = "SF-031: BuscarClientesAsync por nombre debe retornar coincidencias")]
    public async Task BuscarClientesAsync_ConFiltroNombre_DebeRetornarCoincidentes()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var c1 = Cliente.Crear("Carlos", "Vega Morales", "VEMC800101HDFRRN01", "5511112222", "carlos@example.com", "Dir 1");
        var c2 = Cliente.Crear("Laura", "Cortés Martínez", "COML850202MDFRRN02", "5533334444", "laura@example.com", "Dir 2");
        await context.Clientes.AddRangeAsync(c1, c2);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var resultado = await service.BuscarClientesAsync("Vega");

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Should().ContainSingle();
        resultado.Valor.First().Nombre.Should().Be("Carlos");
    }

    [Fact(DisplayName = "SF-031: BuscarClientesAsync por CURP o teléfono debe retornar coincidencias")]
    public async Task BuscarClientesAsync_ConFiltroCurpOTelefono_DebeRetornarCoincidentes()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var c1 = Cliente.Crear("Carlos", "Vega Morales", "VEMC800101HDFRRN01", "5511112222", "carlos@example.com", "Dir 1");
        var c2 = Cliente.Crear("Laura", "Cortés Martínez", "COML850202MDFRRN02", "5533334444", "laura@example.com", "Dir 2");
        await context.Clientes.AddRangeAsync(c1, c2);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var resCurp = await service.BuscarClientesAsync("COML850202");
        var resTel = await service.BuscarClientesAsync("11112222");

        // Assert
        resCurp.EsExitoso.Should().BeTrue();
        resCurp.Valor.Should().ContainSingle().Which.Nombre.Should().Be("Laura");

        resTel.EsExitoso.Should().BeTrue();
        resTel.Valor.Should().ContainSingle().Which.Nombre.Should().Be("Carlos");
    }

    [Fact(DisplayName = "SF-031: BuscarClientesAsync con filtro nulo o vacío debe retornar todos")]
    public async Task BuscarClientesAsync_ConFiltroVacio_DebeRetornarTodos()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var c1 = Cliente.Crear("Carlos", "Vega Morales", "VEMC800101HDFRRN01", "5511112222", "carlos@example.com", "Dir 1");
        var c2 = Cliente.Crear("Laura", "Cortés Martínez", "COML850202MDFRRN02", "5533334444", "laura@example.com", "Dir 2");
        await context.Clientes.AddRangeAsync(c1, c2);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var resultado = await service.BuscarClientesAsync(null);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Should().HaveCount(2);
    }

    [Fact(DisplayName = "SF-031 (BDD): ActualizarClienteAsync con teléfono válido y correo único debe persistir")]
    public async Task ActualizarClienteAsync_ConDatosValidos_DebePersistirCambios()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var cliente = Cliente.Crear("Carlos", "Vega Morales", "VEMC800101HDFRRN01", "5511112222", "carlos@example.com", "Dir 1");
        await context.Clientes.AddAsync(cliente);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);
        var request = new ActualizarClienteRequest(
            Nombre: "Carlos Alberto",
            Apellidos: "Vega Morales",
            Curp: "VEMC800101HDFRRN01",
            Telefono: "5599998888",
            Correo: "carlos.nuevo@example.com",
            Direccion: "Nueva Dirección 456"
        );

        // Act
        var resultado = await service.ActualizarClienteAsync(cliente.Id, request);

        // Assert
        resultado.EsExitoso.Should().BeTrue();

        var clienteActualizado = await context.Clientes.FindAsync(cliente.Id);
        clienteActualizado.Should().NotBeNull();
        clienteActualizado!.Nombre.Should().Be("Carlos Alberto");
        clienteActualizado.Telefono.Should().Be("5599998888");
        clienteActualizado.Correo.Should().Be("carlos.nuevo@example.com");
        clienteActualizado.Direccion.Should().Be("Nueva Dirección 456");
    }

    [Fact(DisplayName = "SF-031: ActualizarClienteAsync con CURP duplicada en otro cliente debe fallar")]
    public async Task ActualizarClienteAsync_ConCurpDuplicadaEnOtroCliente_DebeRetornarFallo()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var c1 = Cliente.Crear("Cliente Uno", "Apellido Uno", "CURP111111HDFRRN01", "5511111111", "uno@example.com", "Dir 1");
        var c2 = Cliente.Crear("Cliente Dos", "Apellido Dos", "CURP222222HDFRRN02", "5522222222", "dos@example.com", "Dir 2");
        await context.Clientes.AddRangeAsync(c1, c2);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);
        var request = new ActualizarClienteRequest("Cliente Dos", "Apellido Dos", c1.Curp, "5522222222", "dos@example.com", "Dir 2");

        // Act
        var resultado = await service.ActualizarClienteAsync(c2.Id, request);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().Contain("CURP");
    }

    [Fact(DisplayName = "SF-031: ActualizarClienteAsync con Correo duplicado en otro cliente debe fallar")]
    public async Task ActualizarClienteAsync_ConCorreoDuplicadoEnOtroCliente_DebeRetornarFallo()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var c1 = Cliente.Crear("Cliente Uno", "Apellido Uno", "CURP111111HDFRRN01", "5511111111", "uno@example.com", "Dir 1");
        var c2 = Cliente.Crear("Cliente Dos", "Apellido Dos", "CURP222222HDFRRN02", "5522222222", "dos@example.com", "Dir 2");
        await context.Clientes.AddRangeAsync(c1, c2);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);
        var request = new ActualizarClienteRequest("Cliente Dos", "Apellido Dos", c2.Curp, "5522222222", c1.Correo, "Dir 2");

        // Act
        var resultado = await service.ActualizarClienteAsync(c2.Id, request);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().Contain("correo");
    }

    [Fact(DisplayName = "SF-031 (BDD): EliminarClienteAsync sobre cliente sin contratos debe eliminar del padrón")]
    public async Task EliminarClienteAsync_ClienteSinContratos_DebeEliminarDePadron()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var cliente = Cliente.Crear("Carlos", "Vega Morales", "VEMC800101HDFRRN01", "5511112222", "carlos@example.com", "Dir 1");
        await context.Clientes.AddAsync(cliente);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var resultado = await service.EliminarClienteAsync(cliente.Id);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        var clienteEnDb = await context.Clientes.FindAsync(cliente.Id);
        clienteEnDb.Should().BeNull();
    }

    [Fact(DisplayName = "SF-031: Cliente.Actualizar aplica validaciones de invariantes")]
    public void Cliente_Actualizar_ConInvariantesValidas_DebeActualizarCampos()
    {
        // Arrange
        var cliente = Cliente.Crear("Laura", "Cortés", "COML850202MDFRRN02", "5533334444", "laura@example.com", "Dir Original");

        // Act
        cliente.Actualizar("Laura Elena", "Cortés Martínez", "COML850202MDFRRN02", "5599991111", "laura.nueva@example.com", "Nueva Dir");

        // Assert
        cliente.Nombre.Should().Be("Laura Elena");
        cliente.Apellidos.Should().Be("Cortés Martínez");
        cliente.Telefono.Should().Be("5599991111");
        cliente.Correo.Should().Be("laura.nueva@example.com");
        cliente.Direccion.Should().Be("Nueva Dir");
        cliente.ObtenerIniciales().Should().Be("LC");
    }

    [Fact(DisplayName = "SF-031: ClienteDto mapea campos de resumen de cartera e iniciales")]
    public async Task ObtenerPorIdAsync_DebeMapearCamposDeCarteraEIniciales()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var cliente = Cliente.Crear("Laura", "Cortés", "COML850202MDFRRN02", "5533334444", "laura@example.com", "Dir 1");
        await context.Clientes.AddAsync(cliente);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var resultado = await service.ObtenerPorIdAsync(cliente.Id);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        var dto = resultado.Valor;
        dto.Iniciales.Should().Be("LC");
        dto.FechaAltaFormateada.Should().StartWith("Cliente desde ");
        dto.ContratosActivos.Should().Be(0);
        dto.SaldoActual.Should().Be(0m);
        dto.EstadoCartera.Should().Be("Al corriente");
    }

    [Fact(DisplayName = "SF-031 / PR1: EliminarClienteAsync debe retornar conflicto cuando el cliente tiene contratos asociados")]
    public async Task EliminarClienteAsync_ConContratosAsociados_DebeRetornarConflicto()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var cliente = Cliente.Crear("Laura", "Cortés", "COML850202MDFRRN02", "5533334444", "laura@example.com", "Dir 1");
        var lote = Lote.Crear("JN-A-042", "Jardín Norte", "Andador A · Fila 4", 2, new Dinero(120000m));
        var venta = Venta.CrearContado("CTR-2026-0001", cliente.Id, lote.Id, Guid.NewGuid().ToString(), new Dinero(120000m), DateTimeOffset.UtcNow);

        await context.Clientes.AddAsync(cliente);
        await context.Lotes.AddAsync(lote);
        await context.Ventas.AddAsync(venta);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var resultado = await service.EliminarClienteAsync(cliente.Id);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().Contain("[CONFLICTO]");
    }

    [Fact(DisplayName = "Clientes: BuscarClientesAsync soporta nombre completo, correo y teléfono con espacios")]
    public async Task BuscarClientesAsync_ConNombreCompletoCorreoYTelefonoConEspacios_DebeEncontrarCoincidencias()
    {
        using var context = CrearContextoEnMemoria();
        var cliente = Cliente.Crear("Carlos", "Vega Morales", "VEMC800101HDFRRN01", "5511112222", "carlos.vega@example.com", "Dir 1");
        await context.Clientes.AddAsync(cliente);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        var resNombreCompleto = await service.BuscarClientesAsync("Carlos Vega");
        var resCorreo = await service.BuscarClientesAsync("carlos.vega@example.com");
        var resTelEspacios = await service.BuscarClientesAsync("55 1111 2222");

        resNombreCompleto.Valor.Should().ContainSingle(c => c.Id == cliente.Id);
        resCorreo.Valor.Should().ContainSingle(c => c.Id == cliente.Id);
        resTelEspacios.Valor.Should().ContainSingle(c => c.Id == cliente.Id);
    }

    [Fact(DisplayName = "Clientes: ObtenerPorId y Buscar calculan cartera real (ContratosActivos, SaldoActual, UltimoPago, Pago vencido)")]
    public async Task BuscarClientesAsync_ConFinanciamientoVencidoYPago_DebeCalcularCarteraReal()
    {
        using var context = CrearContextoEnMemoria();
        var cliente = Cliente.Crear("Elena", "Vargas", "VAEL880101MDFRRN01", "5544445555", "elena@example.com", "Dir 1");
        var lote = Lote.Crear("JC-B-020", "Jardín Central", "Andador B · Fila 2", 3, new Dinero(150000m));
        lote.MarcarVendido();

        var fechaVenta = DateTimeOffset.UtcNow.AddMonths(-2);
        var venta = Venta.CrearFinanciada("CTR-2026-0400", cliente.Id, lote.Id, "vend-1", lote.PrecioContado, fechaVenta);
        var tabla = GeneradorAmortizacion.Generar(120000m, 0.16m / 12m, 12);
        var financiamiento = Financiamiento.Crear(
            venta.Id,
            20m,
            new Dinero(30000m),
            new Dinero(120000m),
            12,
            0.16m,
            tabla,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)));
        var fechaPago = DateTimeOffset.UtcNow.AddDays(-5);
        var pago = Pago.Crear(
            folio: "PAG-2026-000099",
            ventaId: venta.Id,
            financiamientoId: financiamiento.Id,
            mensualidadId: null,
            cajeroId: "caj-1",
            tipo: TipoPago.Enganche,
            metodo: MetodoPago.Efectivo,
            referencia: null,
            montoTotal: new Dinero(30000m),
            montoCapital: new Dinero(30000m),
            montoInteres: new Dinero(0m),
            saldoAnterior: new Dinero(150000m),
            saldoPosterior: new Dinero(120000m),
            claveIdempotencia: "idem-cli-99",
            fechaPago: fechaPago);

        await context.Clientes.AddAsync(cliente);
        await context.Lotes.AddAsync(lote);
        await context.Ventas.AddAsync(venta);
        await context.Financiamientos.AddAsync(financiamiento);
        await context.Mensualidades.AddRangeAsync(financiamiento.Mensualidades);
        await context.Pagos.AddAsync(pago);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);
        var res = await service.BuscarClientesAsync("Elena Vargas");

        res.EsExitoso.Should().BeTrue();
        var dto = res.Valor.Should().ContainSingle().Subject;
        dto.ContratosActivos.Should().Be(1);
        dto.SaldoActual.Should().Be(financiamiento.SaldoInsoluto.Monto);
        dto.UltimoPago.Should().BeCloseTo(fechaPago.UtcDateTime, TimeSpan.FromSeconds(1));
        dto.EstadoCartera.Should().Be("Pago vencido");
    }
}
