using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Application.DTOs.Cobranza;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Tests.Unit.Api;
using Xunit;

namespace SistemaFuneraria.Tests.Integration.Api;

[Trait("Ticket", "SF-083")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Integration")]
public class CobranzaConcurrenciaTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public CobranzaConcurrenciaTests(AuthHttpPipelineTests.ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> AutenticarComoAsync(string correo, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(correo, password));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return dto!.Token;
    }

    private static HttpRequestMessage CrearRequest(
        HttpMethod metodo,
        string url,
        string token,
        string? jsonBody = null,
        string? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(metodo, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.Add("X-Idempotency-Key", idempotencyKey);
        }
        if (jsonBody is not null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }
        return request;
    }

    private async Task<ContratoFinanciadoResumenDto> CrearContratoFinanciadoAsync(string vendedorToken)
    {
        var resLotes = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/lotes?estado=Disponible&pageSize=50", vendedorToken));
        resLotes.StatusCode.Should().Be(HttpStatusCode.OK);
        using var lotesDoc = JsonDocument.Parse(await resLotes.Content.ReadAsStringAsync());
        var loteId = lotesDoc.RootElement.GetProperty("items")[0].GetProperty("id").GetGuid();

        var sufijo = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var cliReq = new RegistrarClienteRequest(
            Nombre: "Adriana",
            Apellidos: "Navarro Solís",
            Curp: $"SF83{sufijo}"[..18],
            Telefono: "5544332211",
            Correo: $"adriana.{sufijo[..8].ToLowerInvariant()}@ejemplo.mx",
            Direccion: "Av. Insurgentes 800, CDMX");

        var resCli = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/clientes", vendedorToken, JsonSerializer.Serialize(cliReq)));
        resCli.StatusCode.Should().Be(HttpStatusCode.Created);
        using var cliDoc = JsonDocument.Parse(await resCli.Content.ReadAsStringAsync());
        var clienteId = cliDoc.RootElement.GetProperty("id").GetGuid();

        var ventaBody = JsonSerializer.Serialize(new { clienteId, loteId, porcentajeEnganche = 20, plazoMeses = 24 });
        var resVenta = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/financiada", vendedorToken, ventaBody));
        resVenta.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resVenta.Content.ReadFromJsonAsync<ContratoFinanciadoResumenDto>())!;
    }

    [Fact(DisplayName = "SF-078 / SF-083: Vendedor recibe 403, falta X-Idempotency-Key da 400 y Cajero cobra + consulta recibo e idempotencia")]
    public async Task CobranzaHttp_DebeValidarRolesIdempotenciaYReciboOficial()
    {
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var cajeroToken = await AutenticarComoAsync("cajero@sistemafunerario.mx", "Sistema Funerario2026!");
        var contrato = await CrearContratoFinanciadoAsync(vendedorToken);

        // 1. Consulta en Caja de contrato demo CTR-2024-0118 y contrato recién creado
        var resDemo = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/cobranza/contrato/CTR-2024-0118", cajeroToken));
        resDemo.StatusCode.Should().Be(HttpStatusCode.OK);

        var resContratoCaja = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{contrato.FolioContrato}", cajeroToken));
        resContratoCaja.StatusCode.Should().Be(HttpStatusCode.OK);
        var cajaDto = await resContratoCaja.Content.ReadFromJsonAsync<ContratoCajaDto>();
        cajaDto!.ProximaCuotaNumero.Should().Be(1);

        // 2. Vendedor intenta cobrar -> 403 Forbidden
        var bodyPago = JsonSerializer.Serialize(new
        {
            folioContrato = contrato.FolioContrato,
            importeRecibido = cajaDto.MontoMensualidad,
            metodoPago = "TransferenciaBancaria",
            referencia = "SPEI-9841",
            numeroCuota = 1
        });
        var resVendedor = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/cobranza/mensualidad", vendedorToken, bodyPago, Guid.NewGuid().ToString()));
        resVendedor.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 3. Sin X-Idempotency-Key -> 400 Bad Request
        var resSinClave = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/cobranza/mensualidad", cajeroToken, bodyPago));
        resSinClave.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 4. Cajero cobra cuota #1 con idempotencia
        var claveIdemp = Guid.NewGuid().ToString();
        var resCobro1 = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/cobranza/mensualidad", cajeroToken, bodyPago, claveIdemp));
        resCobro1.StatusCode.Should().Be(HttpStatusCode.OK);
        var recibo1 = await resCobro1.Content.ReadFromJsonAsync<ReciboPagoDto>();
        recibo1!.FolioPago.Should().StartWith("PAG-");
        recibo1.SaldoPosterior.Should().BeLessThan(recibo1.SaldoAnterior);

        // 5. Reintento con misma ClaveIdempotencia -> 200 OK mismo recibo sin doble cobro
        var resCobro2 = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/cobranza/mensualidad", cajeroToken, bodyPago, claveIdemp));
        resCobro2.StatusCode.Should().Be(HttpStatusCode.OK);
        var recibo2 = await resCobro2.Content.ReadFromJsonAsync<ReciboPagoDto>();
        recibo2!.FolioPago.Should().Be(recibo1.FolioPago);

        // 6. Recibo imprimible y resumen-caja en vivo
        var resRecibo = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/recibos/{recibo1.FolioPago}", cajeroToken));
        resRecibo.StatusCode.Should().Be(HttpStatusCode.OK);

        var resResumen = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/cobranza/resumen-caja", cajeroToken));
        resResumen.StatusCode.Should().Be(HttpStatusCode.OK);
        var corte = await resResumen.Content.ReadFromJsonAsync<CorteDelDiaDto>();
        corte!.PagosRecibidosHoy.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact(DisplayName = "SF-083 (ST-04): Task.WhenAll sobre mismo contrato/cuota con distinta ClaveIdempotencia -> uno 200 OK y uno 409 Conflict")]
    public async Task ST04_CobroConcurrenteTaskWhenAll_DebePermitirUnoYRechazarOtroCon409()
    {
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var cajeroToken = await AutenticarComoAsync("cajero@sistemafunerario.mx", "Sistema Funerario2026!");
        var contrato = await CrearContratoFinanciadoAsync(vendedorToken);

        var resCaja = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{contrato.FolioContrato}", cajeroToken));
        var cajaDto = (await resCaja.Content.ReadFromJsonAsync<ContratoCajaDto>())!;

        var body = JsonSerializer.Serialize(new
        {
            folioContrato = contrato.FolioContrato,
            importeRecibido = cajaDto.MontoMensualidad,
            metodoPago = "Efectivo",
            numeroCuota = 1
        });

        var req1 = CrearRequest(HttpMethod.Post, "/api/v1/cobranza/mensualidad", cajeroToken, body, Guid.NewGuid().ToString());
        var req2 = CrearRequest(HttpMethod.Post, "/api/v1/cobranza/mensualidad", cajeroToken, body, Guid.NewGuid().ToString());

        var respuestas = await Task.WhenAll(_client.SendAsync(req1), _client.SendAsync(req2));
        var codes = respuestas.Select(r => r.StatusCode).ToList();

        codes.Should().ContainSingle(c => c == HttpStatusCode.OK);
        codes.Should().ContainSingle(c => c == HttpStatusCode.Conflict);

        // Verificar que SaldoInsoluto se descontó exactamente una vez (y sigue quedando cuota #2 siguiente)
        var resDespues = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{contrato.FolioContrato}", cajeroToken));
        var despues = (await resDespues.Content.ReadFromJsonAsync<ContratoCajaDto>())!;
        despues.ProximaCuotaNumero.Should().Be(2);
    }
}
