using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Tests.Unit.Api;

namespace SistemaFuneraria.Tests.Integration.Api;

[Trait("Ticket", "SF-038")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Integration")]
public class ClientesCrudHttpTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public ClientesCrudHttpTests(AuthHttpPipelineTests.ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> AutenticarComoAsync(string correo, string password)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(correo, password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        dto.Should().NotBeNull();
        dto!.Token.Should().NotBeNullOrWhiteSpace();
        return dto.Token;
    }

    private static HttpRequestMessage CrearRequest(
        HttpMethod metodo,
        string url,
        string token,
        object? body = null)
    {
        var request = new HttpRequestMessage(metodo, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }
        return request;
    }

    private static async Task<Guid> LeerIdCreadoAsync(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    [Fact(DisplayName = "SF-038 (BDD): Ciclo completo CRUD de clientes y exportación CSV")]
    public async Task CicloCompleto_CrudClientes_DebeCrearBuscarActualizarYExportar()
    {
        var token = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");

        var altaRequest = new RegistrarClienteRequest(
            "Laura",
            "Castañeda",
            "CASL850101MDFRRN01",
            "5512849021",
            "laura.c@email.mx",
            "Av. Insurgentes Sur 123"
        );

        using var postReq = CrearRequest(HttpMethod.Post, "/api/v1/clientes", token, altaRequest);
        var postRes = await _client.SendAsync(postReq);
        postRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var clienteId = await LeerIdCreadoAsync(postRes);
        clienteId.Should().NotBeEmpty();

        using var searchReq = CrearRequest(HttpMethod.Get, "/api/v1/clientes?filtro=Castañeda", token);
        var searchRes = await _client.SendAsync(searchReq);
        searchRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchContent = await searchRes.Content.ReadAsStringAsync();
        searchContent.Should().Contain("Laura");
        searchContent.Should().Contain("Castañeda");

        var updatePayload = new
        {
            Nombre = "Laura",
            Apellidos = "Castañeda",
            Curp = "CASL850101MDFRRN01",
            Telefono = "5598765432",
            Correo = "laura.c@email.mx",
            Direccion = "Av. Insurgentes Sur 123"
        };
        using var putReq = CrearRequest(HttpMethod.Put, $"/api/v1/clientes/{clienteId}", token, updatePayload);
        var putRes = await _client.SendAsync(putReq);
        putRes.StatusCode.Should().Be(HttpStatusCode.OK);

        using var exportReq = CrearRequest(HttpMethod.Get, "/api/v1/clientes/exportar", token);
        var exportRes = await _client.SendAsync(exportReq);
        exportRes.StatusCode.Should().Be(HttpStatusCode.OK);
        exportRes.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
        var csvContent = await exportRes.Content.ReadAsStringAsync();
        csvContent.Should().Contain("Laura");
        csvContent.Should().Contain("5598765432");
    }

    [Fact(DisplayName = "SF-038 (BDD): Validación BVA de teléfono no conforme (400) y CURP duplicada (409)")]
    public async Task ValidacionBva_TelefonoYCurp_DebeRetornar400Y409Conflict()
    {
        var token = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");

        var telefonoCorto = new RegistrarClienteRequest(
            "Cliente", "Corto", "CORC900101HDFRRN01", "551234567", "corto@email.mx", "Calle 1"
        );
        using var postCorto = CrearRequest(HttpMethod.Post, "/api/v1/clientes", token, telefonoCorto);
        var resCorto = await _client.SendAsync(postCorto);
        resCorto.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var telefonoLargo = new RegistrarClienteRequest(
            "Cliente", "Largo", "CORC900101HDFRRN02", "55123456789", "largo@email.mx", "Calle 2"
        );
        using var postLargo = CrearRequest(HttpMethod.Post, "/api/v1/clientes", token, telefonoLargo);
        var resLargo = await _client.SendAsync(postLargo);
        resLargo.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var clienteBase = new RegistrarClienteRequest(
            "Cliente", "Base", "BASE900101HDFRRN01", "5511223344", "base@email.mx", "Calle 3"
        );
        using var postBase = CrearRequest(HttpMethod.Post, "/api/v1/clientes", token, clienteBase);
        var resBase = await _client.SendAsync(postBase);
        resBase.StatusCode.Should().Be(HttpStatusCode.Created);

        var curpDuplicada = new RegistrarClienteRequest(
            "Otro", "Nombre", "BASE900101HDFRRN01", "5599887766", "otro@email.mx", "Calle 4"
        );
        using var postDup = CrearRequest(HttpMethod.Post, "/api/v1/clientes", token, curpDuplicada);
        var resDup = await _client.SendAsync(postDup);
        resDup.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "SF-038 (BDD): RBAC en DELETE /api/v1/clientes: Vendedor 403 Forbidden y Administrador 204 NoContent")]
    public async Task SeguridadRbac_EliminarCliente_VendedorDebeRecibir403YAdminDebeEliminar()
    {
        var adminToken = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");

        var clienteBorrar = new RegistrarClienteRequest(
            "Por", "Borrar", "BORR900101HDFRRN01", "5577665544", "borrar@email.mx", "Calle Temporal"
        );
        using var postReq = CrearRequest(HttpMethod.Post, "/api/v1/clientes", adminToken, clienteBorrar);
        var postRes = await _client.SendAsync(postReq);
        postRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var clienteId = await LeerIdCreadoAsync(postRes);

        using var delVendedorReq = CrearRequest(HttpMethod.Delete, $"/api/v1/clientes/{clienteId}", vendedorToken);
        var delVendedorRes = await _client.SendAsync(delVendedorReq);
        delVendedorRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using var delAdminReq = CrearRequest(HttpMethod.Delete, $"/api/v1/clientes/{clienteId}", adminToken);
        var delAdminRes = await _client.SendAsync(delAdminReq);
        delAdminRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var getReq = CrearRequest(HttpMethod.Get, $"/api/v1/clientes/{clienteId}", adminToken);
        var getRes = await _client.SendAsync(getReq);
        getRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
