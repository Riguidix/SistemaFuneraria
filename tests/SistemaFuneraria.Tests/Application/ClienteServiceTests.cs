using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Application;

[Trait("Ticket", "SF-018")]
[Trait("Capa", "Application")]
public class ClienteServiceTests
{
    private static AppDbContext CrearContextoEnMemoria()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"ClientesTestDb_{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }

    private static RegistrarClienteRequest CrearRequestValido(
        string nombre = "Gustavo",
        string apellidos = "Lucio Rodriguez",
        string curp = "LURG850101HDFRRN09",
        string telefono = "5512345678",
        string correo = "gustavo.lucio@example.com",
        string direccion = "Av. Insurgentes Sur 1234, CDMX")
    {
        return new RegistrarClienteRequest(nombre, apellidos, curp, telefono, correo, direccion);
    }

    #region BDD / Criterios de Aceptación: Validaciones de Registro (RF-02)

    [Theory(DisplayName = "BDD: Teléfono menor a 10 dígitos o con formato inválido debe retornar Fallo y no persistir")]
    [InlineData("12345")]
    [InlineData("551234567")]
    [InlineData("55123456789")]
    [InlineData("551234567a")]
    [InlineData("55-1234-56")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegistrarClienteAsync_ConTelefonoInvalido_DebeRetornarFalloYNoPersistir(string telefonoInvalido)
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);
        var request = CrearRequestValido(telefono: telefonoInvalido);

        // Act
        var resultado = await service.RegistrarClienteAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue("un teléfono no numérico o distinto a 10 dígitos debe ser rechazado por negocio");
        resultado.Error.Should().NotBeNullOrWhiteSpace();
        resultado.Error.ToLowerInvariant().Should().Contain("teléfono", "el mensaje de error debe indicar el campo inválido");

        context.Clientes.Should().BeEmpty("no debe persistirse ningún cliente con datos inválidos");
    }

    [Theory(DisplayName = "BDD: CURP vacía o inválida (distinta a 18 caracteres alfanuméricos) debe retornar Fallo y no persistir")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("LURG850101HDFRRN0")]      // 17 caracteres
    [InlineData("LURG850101HDFRRN099")]     // 19 caracteres
    [InlineData("LURG850101HDFRR-09")]      // caracteres no alfanuméricos
    public async Task RegistrarClienteAsync_ConCurpInvalida_DebeRetornarFalloYNoPersistir(string curpInvalida)
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);
        var request = CrearRequestValido(curp: curpInvalida);

        // Act
        var resultado = await service.RegistrarClienteAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue("el CURP debe tener exactamente 18 caracteres alfanuméricos");
        resultado.Error.Should().NotBeNullOrWhiteSpace();
        resultado.Error.ToLowerInvariant().Should().Contain("curp", "el mensaje de error debe indicar el motivo de la falla");

        context.Clientes.Should().BeEmpty("no debe persistirse ningún cliente con CURP inválida");
    }

    [Theory(DisplayName = "Validación: Nombre o apellidos obligatorios y longitud máxima de 100 caracteres")]
    [InlineData("", "Apellidos Validos")]
    [InlineData("   ", "Apellidos Validos")]
    [InlineData("Nombre Valido", "")]
    [InlineData("Nombre Valido", "   ")]
    public async Task RegistrarClienteAsync_ConNombreOApellidosVacios_DebeRetornarFalloYNoPersistir(string nombre, string apellidos)
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);
        var request = CrearRequestValido(nombre: nombre, apellidos: apellidos);

        // Act
        var resultado = await service.RegistrarClienteAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().NotBeNullOrWhiteSpace();
        context.Clientes.Should().BeEmpty();
    }

    [Fact(DisplayName = "Validación: Nombre o apellidos que excedan 100 caracteres deben ser rechazados")]
    public async Task RegistrarClienteAsync_ConNombreOApellidosExcediendo100Caracteres_DebeRetornarFallo()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);
        string nombreLargo = new('A', 101);
        var request = CrearRequestValido(nombre: nombreLargo);

        // Act
        var resultado = await service.RegistrarClienteAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue();
        context.Clientes.Should().BeEmpty();
    }

    [Theory(DisplayName = "Validación: Correo electrónico inválido o ausente debe retornar Fallo y no persistir")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("correo-sin-arroba")]
    [InlineData("@sin-usuario.com")]
    [InlineData("usuario@")]
    public async Task RegistrarClienteAsync_ConCorreoInvalido_DebeRetornarFalloYNoPersistir(string correoInvalido)
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);
        var request = CrearRequestValido(correo: correoInvalido);

        // Act
        var resultado = await service.RegistrarClienteAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().NotBeNullOrWhiteSpace();
        context.Clientes.Should().BeEmpty();
    }

    [Theory(DisplayName = "Validación: Dirección vacía o en blanco debe retornar Fallo")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegistrarClienteAsync_ConDireccionVacia_DebeRetornarFallo(string direccionInvalida)
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);
        var request = CrearRequestValido(direccion: direccionInvalida);

        // Act
        var resultado = await service.RegistrarClienteAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue();
        context.Clientes.Should().BeEmpty();
    }

    [Fact(DisplayName = "Validación: Petición nula debe lanzar ArgumentNullException o retornar Fallo")]
    public async Task RegistrarClienteAsync_ConRequestNulo_DebeLanzarArgumentNullExceptionORetornarFallo()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);

        // Act & Assert
        try
        {
            var resultado = await service.RegistrarClienteAsync(null!);
            resultado.EsFallo.Should().BeTrue();
        }
        catch (ArgumentNullException ex)
        {
            ex.Should().NotBeNull();
        }
    }

    #endregion

    #region Unicidad / Control de Duplicados en Base de Datos

    [Fact(DisplayName = "Unicidad: Intentar registrar un cliente con CURP ya existente debe retornar Fallo")]
    public async Task RegistrarClienteAsync_ConCurpDuplicado_DebeRetornarFalloYNoDuplicar()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var clienteExistente = Cliente.Crear(
            "Carlos",
            "Mendoza Ruiz",
            "LURG850101HDFRRN09",
            "5598765432",
            "carlos.mendoza@example.com",
            "Calle Allende 456, CDMX"
        );
        context.Clientes.Add(clienteExistente);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);
        var request = CrearRequestValido(
            curp: "  lurg850101hdfrrn09  ", // Mismo CURP con espacios y minúsculas
            correo: "otro.correo@example.com"
        );

        // Act
        var resultado = await service.RegistrarClienteAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue("no se permiten clientes duplicados por CURP en el padrón");
        resultado.Error.ToLowerInvariant().Should().Contain("curp");

        context.Clientes.Count().Should().Be(1, "no debe agregarse un segundo cliente con el mismo CURP");
    }

    [Fact(DisplayName = "Unicidad: Intentar registrar un cliente con Correo ya existente debe retornar Fallo")]
    public async Task RegistrarClienteAsync_ConCorreoDuplicado_DebeRetornarFalloYNoDuplicar()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var clienteExistente = Cliente.Crear(
            "Rosa",
            "Gómez Pérez",
            "GOPR900303MDFRRN05",
            "5543219876",
            "gustavo.lucio@example.com",
            "Av. Hidalgo 789, Toluca"
        );
        context.Clientes.Add(clienteExistente);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);
        var request = CrearRequestValido(
            curp: "OTRO850101HDFRRN09",
            correo: "  GUSTAVO.LUCIO@EXAMPLE.COM  " // Mismo correo con mayúsculas y espacios
        );

        // Act
        var resultado = await service.RegistrarClienteAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue("no se permiten clientes con el mismo correo electrónico registrado");
        resultado.Error.ToLowerInvariant().Should().Contain("correo");

        context.Clientes.Count().Should().Be(1, "no debe agregarse un segundo cliente con el mismo correo");
    }

    #endregion

    #region Happy Path: Registro Exitoso y Normalización

    [Fact(DisplayName = "Registro Exitoso: Con datos válidos persiste en BD, normaliza y retorna Id")]
    public async Task RegistrarClienteAsync_ConDatosValidos_DebePersistirClienteYRetornarIdExitoso()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);
        var request = CrearRequestValido(
            nombre: "  Gustavo  ",
            apellidos: "  Lucio Rodriguez  ",
            curp: "  lurg850101hdfrrn09  ",
            telefono: "  5512345678  ",
            correo: "  Gustavo.Lucio@Example.COM  ",
            direccion: "  Av. Insurgentes Sur 1234  "
        );

        // Act
        var resultado = await service.RegistrarClienteAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsExitoso.Should().BeTrue("la petición contiene todos los datos obligatorios válidos");
        resultado.Valor.Should().NotBeEmpty("debe retornar el Guid del nuevo cliente registrado");

        var clientePersistido = await context.Clientes.FirstOrDefaultAsync(c => c.Id == resultado.Valor);
        clientePersistido.Should().NotBeNull("el cliente debe haber sido persistido en la base de datos");
        clientePersistido!.Nombre.Should().Be("Gustavo");
        clientePersistido.Apellidos.Should().Be("Lucio Rodriguez");
        clientePersistido.Curp.Should().Be("LURG850101HDFRRN09", "el CURP debe normalizarse en mayúsculas y sin espacios");
        clientePersistido.Telefono.Should().Be("5512345678");
        clientePersistido.Correo.Should().Be("gustavo.lucio@example.com", "el correo debe normalizarse en minúsculas");
        clientePersistido.Direccion.Should().Be("Av. Insurgentes Sur 1234");
        clientePersistido.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Consultas: ObtenerPorIdAsync

    [Fact(DisplayName = "Consulta por Id: Cliente existente retorna DTO exitoso con información completa")]
    public async Task ObtenerPorIdAsync_ConClienteExistente_DebeRetornarClienteDtoExitoso()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var cliente = Cliente.Crear(
            "Ana",
            "Salas Morales",
            "SAMA920404MDFRN001",
            "5567890123",
            "ana.salas@example.com",
            "Calle Juárez 100"
        );
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var resultado = await service.ObtenerPorIdAsync(cliente.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsExitoso.Should().BeTrue();
        var dto = resultado.Valor;
        dto.Should().NotBeNull();
        dto.Id.Should().Be(cliente.Id);
        dto.Nombre.Should().Be("Ana");
        dto.Apellidos.Should().Be("Salas Morales");
        dto.Curp.Should().Be("SAMA920404MDFRN001");
        dto.Telefono.Should().Be("5567890123");
        dto.Correo.Should().Be("ana.salas@example.com");
        dto.Direccion.Should().Be("Calle Juárez 100");
        dto.CreatedAt.Should().Be(cliente.CreatedAt);
    }

    [Fact(DisplayName = "Consulta por Id: Guid inexistente debe retornar Fallo")]
    public async Task ObtenerPorIdAsync_ConIdInexistente_DebeRetornarFallo()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);
        var idInexistente = Guid.NewGuid();

        // Act
        var resultado = await service.ObtenerPorIdAsync(idInexistente);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue("no existe ningún cliente con ese ID");
        resultado.Error.ToLowerInvariant().Should().Contain("no encontrado");
    }

    #endregion

    #region Consultas: ObtenerPorCurpAsync

    [Fact(DisplayName = "Consulta por CURP: Cliente existente retorna DTO exitoso incluso con espacios o minúsculas")]
    public async Task ObtenerPorCurpAsync_ConClienteExistente_DebeRetornarClienteDtoExitoso()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var cliente = Cliente.Crear(
            "Roberto",
            "Vega Castillo",
            "VECR880505HDFRRN02",
            "5589012345",
            "roberto.vega@example.com",
            "Paseo de la Reforma 200"
        );
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var resultado = await service.ObtenerPorCurpAsync("  vecr880505hdfrrn02  ");

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsExitoso.Should().BeTrue();
        var dto = resultado.Valor;
        dto.Should().NotBeNull();
        dto.Id.Should().Be(cliente.Id);
        dto.Curp.Should().Be("VECR880505HDFRRN02");
        dto.Nombre.Should().Be("Roberto");
    }

    [Theory(DisplayName = "Consulta por CURP: CURP no encontrada o inválida retorna Fallo")]
    [InlineData("CURPINEXISTENTE188")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ObtenerPorCurpAsync_ConCurpInexistente_DebeRetornarFallo(string curpABuscar)
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);

        // Act
        var resultado = await service.ObtenerPorCurpAsync(curpABuscar);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue();
    }

    #endregion

    #region Consultas: ListarClientesAsync

    [Fact(DisplayName = "Listado de Clientes: Si la base está vacía debe retornar lista vacía exitosa")]
    public async Task ListarClientesAsync_CuandoNoHayClientes_DebeRetornarListaVaciaExitosa()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);

        // Act
        var resultado = await service.ListarClientesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
        resultado.Valor.Should().BeEmpty();
    }

    [Fact(DisplayName = "Listado de Clientes: Retorna todos los clientes registrados ordenados y mapeados")]
    public async Task ListarClientesAsync_ConClientesRegistrados_DebeRetornarTodosLosClientesMapeados()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var c1 = Cliente.Crear("Alfonso", "Bermúdez", "BEBA800101HDFRRN01", "5511111111", "alfonso@example.com", "Dir 1");
        var c2 = Cliente.Crear("Beatriz", "Calderón", "CABX820202MDFRRN02", "5522222222", "beatriz@example.com", "Dir 2");
        context.Clientes.AddRange(c1, c2);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var resultado = await service.ListarClientesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Should().HaveCount(2);
        resultado.Valor.Select(c => c.Curp).Should().Contain(new[] { "BEBA800101HDFRRN01", "CABX820202MDFRRN02" });
    }

    #endregion

    #region Soporte de Cancelación con CancellationToken

    [Fact(DisplayName = "Cancelación: RegistrarClienteAsync debe respetar CancellationToken cancelado")]
    public async Task RegistrarClienteAsync_ConCancellationTokenCancelado_DebeCancelarOperacion()
    {
        // Arrange
        using var context = CrearContextoEnMemoria();
        var service = new ClienteService(context);
        var request = CrearRequestValido();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var accion = () => service.RegistrarClienteAsync(request, cts.Token);
        await accion.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
