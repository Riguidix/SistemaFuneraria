using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Application.DTOs.Cobranza;
using SistemaFuneraria.Application.DTOs.Configuracion;
using SistemaFuneraria.Application.DTOs.Lotes;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Tests.Unit.Api;
using Xunit;

namespace SistemaFuneraria.Tests.Integration.Api;

[Trait("Ticket", "SF-098")]
[Trait("Capa", "Api")]
[Trait("Tipo", "UAT")]
public class UatEscenariosSistemaTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public UatEscenariosSistemaTests(AuthHttpPipelineTests.ApiFactory factory)
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
        var req = new HttpRequestMessage(metodo, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            req.Headers.Add("X-Idempotency-Key", idempotencyKey);
        }
        if (jsonBody is not null)
        {
            req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }
        return req;
    }

    private async Task<(Guid ClienteId, Guid LoteId)> CrearClienteYLoteDisponibleAsync(string adminToken, int gavetas = 2, decimal precio = 120000m)
    {
        var clave = $"UT-{ (char)('A' + Random.Shared.Next(0, 26)) }-{Random.Shared.Next(100, 999)}";
        var resLote = await _client.SendAsync(CrearRequest(
            HttpMethod.Post,
            "/api/v1/lotes",
            adminToken,
            JsonSerializer.Serialize(new CrearLoteCommand(clave, "Jardín UAT", "Andador UAT", gavetas, precio))));
        resLote.StatusCode.Should().Be(HttpStatusCode.Created);
        using var loteDoc = JsonDocument.Parse(await resLote.Content.ReadAsStringAsync());
        var loteId = loteDoc.RootElement.GetProperty("id").GetGuid();

        var sufijo = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var cliReq = new RegistrarClienteRequest("Elena", "UAT Prueba", $"UAT8{sufijo}"[..18], "5512349876", $"uat.{sufijo[..8].ToLowerInvariant()}@ejemplo.mx", "Av. Memorial 10");
        var resCli = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/clientes", adminToken, JsonSerializer.Serialize(cliReq)));
        resCli.StatusCode.Should().Be(HttpStatusCode.Created);
        using var cliDoc = JsonDocument.Parse(await resCli.Content.ReadAsStringAsync());
        return (cliDoc.RootElement.GetProperty("id").GetGuid(), loteId);
    }

    [Fact(DisplayName = "SF-098 (UAT ST-01 a ST-05): Certificación integral E2E sin errores bloqueantes")]
    public async Task UAT_EscenariosST01aST05_DebenCompletarExitosamente()
    {
        var adminToken = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var cajeroToken = await AutenticarComoAsync("cajero@sistemafunerario.mx", "Sistema Funerario2026!");

        // ST-01: Venta de Contado + bloqueo de lote (409)
        var (cli1, lote1) = await CrearClienteYLoteDisponibleAsync(adminToken);
        var bodyContado = JsonSerializer.Serialize(new { clienteId = cli1, loteId = lote1, metodoPago = "Efectivo", referenciaPago = "UAT-ST01" });
        var resSt01 = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/contado", vendedorToken, bodyContado));
        resSt01.StatusCode.Should().Be(HttpStatusCode.Created);
        var resDobleSt01 = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/contado", vendedorToken, bodyContado));
        resDobleSt01.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // ST-02: Venta a Crédito + 3 pagos ordinarios + Abono a Capital $10,000
        var (cli2, lote2) = await CrearClienteYLoteDisponibleAsync(adminToken, 2, 120000m);
        var resSt02 = await _client.SendAsync(CrearRequest(
            HttpMethod.Post,
            "/api/v1/ventas/financiada",
            vendedorToken,
            JsonSerializer.Serialize(new { clienteId = cli2, loteId = lote2, porcentajeEnganche = 20, plazoMeses = 12 })));
        resSt02.StatusCode.Should().Be(HttpStatusCode.Created);
        var fin2 = (await resSt02.Content.ReadFromJsonAsync<ContratoFinanciadoResumenDto>())!;

        for (int cuota = 1; cuota <= 3; cuota++)
        {
            var cajaDto = (await (await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{fin2.FolioContrato}", cajeroToken)))
                .Content.ReadFromJsonAsync<ContratoCajaDto>())!;
            var resPago = await _client.SendAsync(CrearRequest(
                HttpMethod.Post,
                "/api/v1/cobranza/mensualidad",
                cajeroToken,
                JsonSerializer.Serialize(new { folioContrato = fin2.FolioContrato, importeRecibido = cajaDto.MontoMensualidad, metodoPago = "Efectivo", numeroCuota = cuota }),
                Guid.NewGuid().ToString()));
            resPago.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var resAbono = await _client.SendAsync(CrearRequest(
            HttpMethod.Post,
            "/api/v1/cobranza/abono-capital",
            cajeroToken,
            JsonSerializer.Serialize(new { folioContrato = fin2.FolioContrato, montoAbono = 10_000m, metodoPago = "TransferenciaBancaria", referencia = "UAT-ABONO" }),
            Guid.NewGuid().ToString()));
        resAbono.StatusCode.Should().Be(HttpStatusCode.OK);

        // ST-03: Liquidación anticipada sin penalización -> Liquidado
        var cajaPreLiq = (await (await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{fin2.FolioContrato}", cajeroToken)))
            .Content.ReadFromJsonAsync<ContratoCajaDto>())!;
        var resLiq = await _client.SendAsync(CrearRequest(
            HttpMethod.Post,
            "/api/v1/cobranza/liquidacion",
            cajeroToken,
            JsonSerializer.Serialize(new { folioContrato = fin2.FolioContrato, importeRecibido = cajaPreLiq.MontoLiquidacionEstimado, metodoPago = "TransferenciaBancaria", referencia = "UAT-LIQ" }),
            Guid.NewGuid().ToString()));
        resLiq.StatusCode.Should().Be(HttpStatusCode.OK);
        var reciboLiq = (await resLiq.Content.ReadFromJsonAsync<ReciboPagoDto>())!;
        reciboLiq.SaldoPosterior.Should().Be(0m);

        // ST-04: Concurrencia (Task.WhenAll) + Idempotencia
        var (cli4, lote4) = await CrearClienteYLoteDisponibleAsync(adminToken);
        var resFin4 = await _client.SendAsync(CrearRequest(
            HttpMethod.Post,
            "/api/v1/ventas/financiada",
            vendedorToken,
            JsonSerializer.Serialize(new { clienteId = cli4, loteId = lote4, porcentajeEnganche = 20, plazoMeses = 12 })));
        var fin4 = (await resFin4.Content.ReadFromJsonAsync<ContratoFinanciadoResumenDto>())!;
        var caja4 = (await (await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{fin4.FolioContrato}", cajeroToken)))
            .Content.ReadFromJsonAsync<ContratoCajaDto>())!;
        var payloadSt04 = JsonSerializer.Serialize(new { folioContrato = fin4.FolioContrato, importeRecibido = caja4.MontoMensualidad, metodoPago = "Efectivo", numeroCuota = 1 });
        var paralelas = await Task.WhenAll(
            _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/cobranza/mensualidad", cajeroToken, payloadSt04, Guid.NewGuid().ToString())),
            _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/cobranza/mensualidad", cajeroToken, payloadSt04, Guid.NewGuid().ToString())));
        paralelas.Select(r => r.StatusCode).Should().ContainSingle(c => c == HttpStatusCode.OK);
        paralelas.Select(r => r.StatusCode).Should().ContainSingle(c => c == HttpStatusCode.Conflict);

        // ST-05: Ciclo Administrador (Sucursal, Usuario con login inmediato, cancelación de contrato liberando lote)
        var resSuc = await _client.SendAsync(CrearRequest(
            HttpMethod.Put,
            "/api/v1/configuracion/sucursal",
            adminToken,
            JsonSerializer.Serialize(new ActualizarConfiguracionSucursalRequest("Jardines del Valle", "55 4120 9800", "operacion@sistemafunerario.mx", "Av. Paseo de los Cipreses 420", "Zapopan", "45010", true, true))));
        resSuc.StatusCode.Should().Be(HttpStatusCode.OK);

        var correoUat = $"uat.{Guid.NewGuid():N}"[..16] + "@sistemafunerario.mx";
        var resUser = await _client.SendAsync(CrearRequest(
            HttpMethod.Post,
            "/api/v1/configuracion/usuarios",
            adminToken,
            JsonSerializer.Serialize(new CrearUsuarioConfiguracionRequest("Cajero UAT", correoUat, "CajeroUat2026!", "Cajero"))));
        resUser.StatusCode.Should().Be(HttpStatusCode.Created);
        (await AutenticarComoAsync(correoUat, "CajeroUat2026!")).Should().NotBeNullOrWhiteSpace();

        var resCancel = await _client.SendAsync(CrearRequest(
            HttpMethod.Delete,
            $"/api/v1/ventas/{fin4.VentaId}?motivo=Cancelación UAT ST-05 liberando lote",
            adminToken));
        resCancel.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
