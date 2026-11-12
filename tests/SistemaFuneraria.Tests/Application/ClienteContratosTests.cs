using System.Reflection;
using FluentAssertions;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Servicios.Clientes;
using Xunit;

namespace SistemaFuneraria.Tests.Application;

public class ClienteContratosTests
{
    [Fact(DisplayName = "SF-017 / BDD: IClienteService debe exponer contratos asíncronos con CancellationToken y Resultado<T>")]
    public void IClienteService_DebeExponerMetodosAsincronosConResultadoYCancellationToken()
    {
        var tipoServicio = typeof(IClienteService);
        tipoServicio.IsInterface.Should().BeTrue("IClienteService debe ser un contrato (interfaz)");

        // 1. RegistrarClienteAsync
        var metodoRegistrar = tipoServicio.GetMethod("RegistrarClienteAsync");
        metodoRegistrar.Should().NotBeNull("debe existir el método RegistrarClienteAsync");
        metodoRegistrar!.ReturnType.Should().Be(typeof(Task<Resultado<Guid>>), "debe retornar Task<Resultado<Guid>>");
        
        var paramsRegistrar = metodoRegistrar.GetParameters();
        paramsRegistrar.Should().HaveCount(2);
        paramsRegistrar[0].ParameterType.Should().Be(typeof(RegistrarClienteRequest));
        paramsRegistrar[1].ParameterType.Should().Be(typeof(CancellationToken));

        // 2. ObtenerPorIdAsync
        var metodoObtenerPorId = tipoServicio.GetMethod("ObtenerPorIdAsync");
        metodoObtenerPorId.Should().NotBeNull("debe existir el método ObtenerPorIdAsync");
        metodoObtenerPorId!.ReturnType.Should().Be(typeof(Task<Resultado<ClienteDto>>), "debe retornar Task<Resultado<ClienteDto>>");
        
        var paramsObtenerId = metodoObtenerPorId.GetParameters();
        paramsObtenerId.Should().HaveCount(2);
        paramsObtenerId[0].ParameterType.Should().Be(typeof(Guid));
        paramsObtenerId[1].ParameterType.Should().Be(typeof(CancellationToken));

        // 3. ObtenerPorCurpAsync
        var metodoObtenerPorCurp = tipoServicio.GetMethod("ObtenerPorCurpAsync");
        metodoObtenerPorCurp.Should().NotBeNull("debe existir el método ObtenerPorCurpAsync");
        metodoObtenerPorCurp!.ReturnType.Should().Be(typeof(Task<Resultado<ClienteDto>>), "debe retornar Task<Resultado<ClienteDto>>");
        
        var paramsObtenerCurp = metodoObtenerPorCurp.GetParameters();
        paramsObtenerCurp.Should().HaveCount(2);
        paramsObtenerCurp[0].ParameterType.Should().Be(typeof(string));
        paramsObtenerCurp[1].ParameterType.Should().Be(typeof(CancellationToken));

        // 4. ListarClientesAsync
        var metodoListar = tipoServicio.GetMethod("ListarClientesAsync");
        metodoListar.Should().NotBeNull("debe existir el método ListarClientesAsync");
        metodoListar!.ReturnType.Should().Be(typeof(Task<Resultado<IReadOnlyList<ClienteDto>>>), "debe retornar Task<Resultado<IReadOnlyList<ClienteDto>>>");
        
        var paramsListar = metodoListar.GetParameters();
        paramsListar.Should().HaveCount(1);
        paramsListar[0].ParameterType.Should().Be(typeof(CancellationToken));
    }

    [Fact(DisplayName = "SF-017: RegistrarClienteRequest debe ser inmutable y contener campos obligatorios de negocio")]
    public void RegistrarClienteRequest_DebeSerInmutableYContenerPropiedadesRequeridas()
    {
        var request = new RegistrarClienteRequest(
            Nombre: "Juan",
            Apellidos: "Pérez Gómez",
            Curp: "PEGI850101HDFRMN09",
            Telefono: "6141234567",
            Correo: "juan.perez@example.com",
            Direccion: "Av. Universidad 1234, Col. Centro"
        );

        request.Nombre.Should().Be("Juan");
        request.Apellidos.Should().Be("Pérez Gómez");
        request.Curp.Should().Be("PEGI850101HDFRMN09");
        request.Telefono.Should().Be("6141234567");
        request.Correo.Should().Be("juan.perez@example.com");
        request.Direccion.Should().Be("Av. Universidad 1234, Col. Centro");
    }

    [Fact(DisplayName = "SF-017: ClienteDto debe contener todos los campos de visualización incluyendo Id y CreatedAt")]
    public void ClienteDto_DebeTransportarInformacionCompletaDeCliente()
    {
        var id = Guid.NewGuid();
        var fecha = DateTimeOffset.UtcNow;

        var dto = new ClienteDto(
            Id: id,
            Nombre: "María",
            Apellidos: "López Domínguez",
            Curp: "LODM900202MDFRN001",
            Telefono: "6149876543",
            Correo: "maria.lopez@example.com",
            Direccion: "Calle Victoria 567",
            CreatedAt: fecha
        );

        dto.Id.Should().Be(id);
        dto.Nombre.Should().Be("María");
        dto.Apellidos.Should().Be("López Domínguez");
        dto.Curp.Should().Be("LODM900202MDFRN001");
        dto.Telefono.Should().Be("6149876543");
        dto.Correo.Should().Be("maria.lopez@example.com");
        dto.Direccion.Should().Be("Calle Victoria 567");
        dto.CreatedAt.Should().Be(fecha);
    }
}
