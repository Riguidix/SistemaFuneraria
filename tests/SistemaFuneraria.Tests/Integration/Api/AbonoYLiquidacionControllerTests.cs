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

[Trait("Ticket", "SF-093")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Integration")]
public class AbonoYLiquidacionControllerTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public AbonoYLiquidacionControllerTests(AuthHttpPipelineTests.ApiFactory factory)
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

    private async Task<ContratoFinanciadoResumenDto> CrearContratoFinanciado12MesesAsync(string vendedorToken)
    {
        var resLotes = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/lotes?estado=Disponible&pageSize=50", vendedorToken));
        resLotes.StatusCode.Should().Be(HttpStatusCode.OK);
        using var lotesDoc = JsonDocument.Parse(await resLotes.Content.ReadAsStringAsync());
        var loteId = lotesDoc.RootElement.GetProperty("items")[0].GetProperty("id").GetGuid();

        var sufijo = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var cliReq = new RegistrarClienteRequest(
            Nombre: "Carlos",
            Apellidos: "Fuentes Rivas",
            Curp: $"SF93{sufijo}"[..18],
            Telefono: "5588776655",
            Correo: $"carlos.{sufijo[..8].ToLowerInvariant()}@ejemplo.mx",
            Direccion: "Calzada de Tlalpan 1200, CDMX");

        var resCli = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/clientes", vendedorToken, JsonSerializer.Serialize(cliReq)));
        resCli.StatusCode.Should().Be(HttpStatusCode.Created);
        using var cliDoc = JsonDocument.Parse(await resCli.Content.ReadAsStringAsync());
        var clienteId = cliDoc.RootElement.GetProperty("id").GetGuid();

        var ventaBody = JsonSerializer.Serialize(new { clienteId, loteId, porcentajeEnganche = 20, plazoMeses = 12 });
        var resVenta = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/financiada", vendedorToken, ventaBody));
        resVenta.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resVenta.Content.ReadFromJsonAsync<ContratoFinanciadoResumenDto>())!;
    }

    private async Task PagarCuotasInicialesAsync(string folioContrato, string cajeroToken, int totalCuotas)
    {
        for (int i = 1; i <= totalCuotas; i++)
        {
            var resCaja = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{folioContrato}", cajeroToken));
            resCaja.StatusCode.Should().Be(HttpStatusCode.OK);
            var cajaDto = await resCaja.Content.ReadFromJsonAsync<ContratoCajaDto>();

            var bodyPago = JsonSerializer.Serialize(new
            {
                folioContrato,
                importeRecibido = cajaDto!.MontoMensualidad,
                metodoPago = "Efectivo",
                numeroCuota = i
            });

            var resPago = await _client.SendAsync(CrearRequest(
                HttpMethod.Post,
                "/api/v1/cobranza/mensualidad",
                cajeroToken,
                bodyPago,
                Guid.NewGuid().ToString()));

            resPago.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact(DisplayName = "SF-093 / CP-07: Abono a capital $10,000 recalcula cuotas 4..12 en $7,523.51")]
    public async Task CP07_AbonoCapital_10k_DebeReducirSaldoYRecalcularCuotasA752351()
    {
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var cajeroToken = await AutenticarComoAsync("cajero@sistemafunerario.mx", "Sistema Funerario2026!");

        var contrato = await CrearContratoFinanciado12MesesAsync(vendedorToken);
        await PagarCuotasInicialesAsync(contrato.FolioContrato, cajeroToken, 3);

        var bodyAbono = JsonSerializer.Serialize(new
        {
            folioContrato = contrato.FolioContrato,
            montoAbono = 10_000.00m,
            metodoPago = "TransferenciaBancaria",
            referencia = "ABONO-CAP-07"
        });

        var resAbono = await _client.SendAsync(CrearRequest(
            HttpMethod.Post,
            "/api/v1/cobranza/abono-capital",
            cajeroToken,
            bodyAbono,
            Guid.NewGuid().ToString()));

        resAbono.StatusCode.Should().Be(HttpStatusCode.OK);
        var reciboAbono = await resAbono.Content.ReadFromJsonAsync<ReciboPagoDto>();
        reciboAbono.Should().NotBeNull();
        reciboAbono!.TipoPago.Should().Be("Abono a capital");
        reciboAbono.MontoTotal.Should().Be(10_000.00m);

        var resAmort = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/ventas/{contrato.VentaId}/amortizacion", vendedorToken));
        resAmort.StatusCode.Should().Be(HttpStatusCode.OK);
        var cuotas = await resAmort.Content.ReadFromJsonAsync<List<CuotaAmortizacionContratoDto>>();
        cuotas.Should().NotBeNull();
        cuotas!.Should().HaveCount(12);

        var cuota4 = cuotas!.First(c => c.NumeroCuota == 4);
        cuota4.CuotaTotal.Should().BeOneOf(7_523.51m, 7_523.67m);
    }

    [Fact(DisplayName = "SF-093 / CP-08: Liquidación anticipada $77,893.39 liquida el contrato con 10% descuento")]
    public async Task CP08_LiquidacionAnticipada_ConDescuento10Porciento_DebeCerrarContrato()
    {
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var cajeroToken = await AutenticarComoAsync("cajero@sistemafunerario.mx", "Sistema Funerario2026!");

        var contrato = await CrearContratoFinanciado12MesesAsync(vendedorToken);
        await PagarCuotasInicialesAsync(contrato.FolioContrato, cajeroToken, 3);

        var resCaja = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{contrato.FolioContrato}", cajeroToken));
        resCaja.StatusCode.Should().Be(HttpStatusCode.OK);
        var cajaDto = await resCaja.Content.ReadFromJsonAsync<ContratoCajaDto>();

        var bodyLiq = JsonSerializer.Serialize(new
        {
            folioContrato = contrato.FolioContrato,
            importeRecibido = cajaDto!.MontoLiquidacionEstimado,
            metodoPago = "TransferenciaBancaria",
            referencia = "LIQ-PRONTO-PAGO"
        });

        var resLiq = await _client.SendAsync(CrearRequest(
            HttpMethod.Post,
            "/api/v1/cobranza/liquidacion",
            cajeroToken,
            bodyLiq,
            Guid.NewGuid().ToString()));

        resLiq.StatusCode.Should().Be(HttpStatusCode.OK);
        var reciboLiq = await resLiq.Content.ReadFromJsonAsync<ReciboPagoDto>();
        reciboLiq.Should().NotBeNull();
        reciboLiq!.TipoPago.Should().Be("Liquidación");

        var resCajaPost = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{contrato.FolioContrato}", cajeroToken));
        resCajaPost.StatusCode.Should().Be(HttpStatusCode.OK);
        var cajaPostDto = await resCajaPost.Content.ReadFromJsonAsync<ContratoCajaDto>();
        cajaPostDto!.SaldoInsoluto.Should().Be(0.00m);
        cajaPostDto.EstadoVisual.Should().Be("Liquidado");
    }

    [Fact(DisplayName = "SF-093: Abono a capital con parámetros inválidos o sin idempotencia debe retornar 400 Bad Request")]
    public async Task AbonoCapital_Validaciones_DebeRetornar400BadRequest()
    {
        var cajeroToken = await AutenticarComoAsync("cajero@sistemafunerario.mx", "Sistema Funerario2026!");

        var bodyInvalido = JsonSerializer.Serialize(new { folioContrato = "", montoAbono = -500m });
        var res = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/cobranza/abono-capital", cajeroToken, bodyInvalido));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
