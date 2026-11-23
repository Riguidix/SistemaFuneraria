using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Tests.Unit.Api;
using Xunit;

namespace SistemaFuneraria.Tests.Integration.Api;

[Trait("Ticket", "SF-065")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Integration")]
public class VentasContadoControllerTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public VentasContadoControllerTests(AuthHttpPipelineTests.ApiFactory factory)
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
        string? jsonBody = null)
    {
        var request = new HttpRequestMessage(metodo, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (jsonBody is not null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }
        return request;
    }

    private async Task<(Guid ClienteId, Guid LoteId)> PrepararClienteYLoteDisponibleAsync(string token, int indiceLote = 0)
    {
        var resLotes = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/lotes?estado=Disponible&pageSize=20", token));
        resLotes.StatusCode.Should().Be(HttpStatusCode.OK);
        using var lotesDoc = JsonDocument.Parse(await resLotes.Content.ReadAsStringAsync());
        var items = lotesDoc.RootElement.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(indiceLote);
        var loteId = items[indiceLote].GetProperty("id").GetGuid();

        var sufijo = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var curp = $"SF65{sufijo}"[..18];
        var clienteReq = new RegistrarClienteRequest(
            Nombre: "Mariana",
            Apellidos: "Torres Aguirre",
            Curp: curp,
            Telefono: "5512345678",
            Correo: $"mariana.{sufijo[..8].ToLowerInvariant()}@ejemplo.mx",
            Direccion: "Av. Juárez 120, Col. Centro"
        );

        var resCliente = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/clientes", token, JsonSerializer.Serialize(clienteReq)));
        resCliente.StatusCode.Should().Be(HttpStatusCode.Created);
        using var cliDoc = JsonDocument.Parse(await resCliente.Content.ReadAsStringAsync());
        var clienteId = cliDoc.RootElement.GetProperty("id").GetGuid();

        return (clienteId, loteId);
    }

    [Fact(DisplayName = "SF-065 / CP-02 / ST-01: Venta Contado retorna 201 Liquidado, marca lote Vendido y rechaza doble venta con 409")]
    public async Task ST01_CP02_VentaContadoE2E_DebeRetornar201MarcarLoteVendidoYRechazarDobleVenta409()
    {
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var (clienteId, loteId) = await PrepararClienteYLoteDisponibleAsync(vendedorToken, 0);

        var jsonBody = JsonSerializer.Serialize(new
        {
            clienteId,
            loteId,
            metodoPago = "Efectivo",
            referenciaPago = "EFEC-CP02"
        });

        var resVenta = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/contado", vendedorToken, jsonBody));

        resVenta.StatusCode.Should().Be(HttpStatusCode.Created);
        var contrato = await resVenta.Content.ReadFromJsonAsync<ContratoResumenDto>();
        contrato.Should().NotBeNull();
        contrato!.Modalidad.Should().Be("Contado");
        contrato.Estado.Should().Be("Liquidado");
        contrato.FolioContrato.Should().StartWith("CTR-");
        contrato.FolioPago.Should().StartWith("PAG-");

        // Verificar que el lote cambió a estado Vendido
        var resLote = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/lotes/{loteId}", vendedorToken));
        resLote.StatusCode.Should().Be(HttpStatusCode.OK);
        using var loteDoc = JsonDocument.Parse(await resLote.Content.ReadAsStringAsync());
        loteDoc.RootElement.GetProperty("estado").GetString().Should().Be("Vendido");

        // Verificar consulta GET /api/v1/ventas/{id}
        var resDetalle = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/ventas/{contrato.VentaId}", vendedorToken));
        resDetalle.StatusCode.Should().Be(HttpStatusCode.OK);
        var detalleDto = await resDetalle.Content.ReadFromJsonAsync<ContratoResumenDto>();
        detalleDto!.FolioContrato.Should().Be(contrato.FolioContrato);

        // Segundo intento de venta sobre el mismo lote -> 409 Conflict
        var resDobleVenta = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/contado", vendedorToken, jsonBody));
        resDobleVenta.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "SF-063 / SF-065: Cancelación Admin libera lote a Disponible, Vendedor recibe 403 y motivo corto 400")]
    public async Task CancelacionAdmin_DebeBloquearVendedor403RechazarMotivoCorto400YLiberarLoteDisponible()
    {
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var adminToken = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");
        var (clienteId, loteId) = await PrepararClienteYLoteDisponibleAsync(vendedorToken, 0);

        var jsonVenta = JsonSerializer.Serialize(new { clienteId, loteId, metodoPago = "TransferenciaBancaria", referenciaPago = "SPEI-063" });
        var resVenta = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/contado", vendedorToken, jsonVenta));
        resVenta.StatusCode.Should().Be(HttpStatusCode.Created);
        var contrato = await resVenta.Content.ReadFromJsonAsync<ContratoResumenDto>();

        // 1. Vendedor intenta cancelar -> 403 Forbidden
        var resVendedor = await _client.SendAsync(CrearRequest(
            HttpMethod.Delete,
            $"/api/v1/ventas/{contrato!.VentaId}?motivo=Intento no autorizado suficiente",
            vendedorToken));
        resVendedor.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 2. Admin con motivo < 10 chars -> 400 Bad Request
        var resMotivoCorto = await _client.SendAsync(CrearRequest(
            HttpMethod.Delete,
            $"/api/v1/ventas/{contrato.VentaId}?motivo=corto",
            adminToken));
        resMotivoCorto.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 3. Admin cancela correctamente -> 200 OK y lote vuelve a Disponible
        var resCancel = await _client.SendAsync(CrearRequest(
            HttpMethod.Delete,
            $"/api/v1/ventas/{contrato.VentaId}?motivo=Error de captura en titular",
            adminToken));
        resCancel.StatusCode.Should().Be(HttpStatusCode.OK);

        var resContratoFinal = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/ventas/{contrato.VentaId}", adminToken));
        resContratoFinal.StatusCode.Should().Be(HttpStatusCode.OK);
        var contratoFinal = await resContratoFinal.Content.ReadFromJsonAsync<ContratoResumenDto>();
        contratoFinal!.Estado.Should().Be("Cancelado");

        var resLoteFinal = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/lotes/{loteId}", adminToken));
        resLoteFinal.StatusCode.Should().Be(HttpStatusCode.OK);
        using var loteDoc = JsonDocument.Parse(await resLoteFinal.Content.ReadAsStringAsync());
        loteDoc.RootElement.GetProperty("estado").GetString().Should().Be("Disponible");
    }

    [Fact(DisplayName = "SF-065: Consulta de venta inexistente retorna 404 y sin token retorna 401")]
    public async Task SeguridadYNotFound_DebeRetornar401Y404()
    {
        var resSinToken = await _client.GetAsync($"/api/v1/ventas/{Guid.NewGuid()}");
        resSinToken.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var token = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var resNotFound = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/ventas/{Guid.NewGuid()}", token));
        resNotFound.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
