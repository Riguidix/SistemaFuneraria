using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Application.DTOs.Contratos;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Tests.Unit.Api;
using Xunit;

namespace SistemaFuneraria.Tests.Integration.Api;

[Trait("Ticket", "SF-074")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Integration")]
public class VentasFinanciadasControllerTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public VentasFinanciadasControllerTests(AuthHttpPipelineTests.ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> AutenticarVendedorAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("vendedor@sistemafunerario.mx", "Sistema Funerario2026!"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return dto!.Token;
    }

    private static HttpRequestMessage CrearRequest(HttpMethod metodo, string url, string token, string? jsonBody = null)
    {
        var request = new HttpRequestMessage(metodo, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (jsonBody is not null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }
        return request;
    }

    private async Task<(Guid ClienteId, Guid LoteId)> PrepararClienteYLoteDisponibleAsync(string token, int? gavetasRequeridas = null)
    {
        var resLotes = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/lotes?estado=Disponible&pageSize=50", token));
        resLotes.StatusCode.Should().Be(HttpStatusCode.OK);
        using var lotesDoc = JsonDocument.Parse(await resLotes.Content.ReadAsStringAsync());
        var items = lotesDoc.RootElement.GetProperty("items");

        Guid loteId = Guid.Empty;
        foreach (var item in items.EnumerateArray())
        {
            if (!gavetasRequeridas.HasValue || item.GetProperty("gavetas").GetInt32() == gavetasRequeridas.Value)
            {
                loteId = item.GetProperty("id").GetGuid();
                break;
            }
        }
        loteId.Should().NotBe(Guid.Empty);

        var sufijo = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var clienteReq = new RegistrarClienteRequest(
            Nombre: "Valeria",
            Apellidos: "Mendoza Ruiz",
            Curp: $"SF74{sufijo}"[..18],
            Telefono: "5587654321",
            Correo: $"valeria.{sufijo[..8].ToLowerInvariant()}@ejemplo.mx",
            Direccion: "Av. Reforma 500, CDMX");

        var resCliente = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/clientes", token, JsonSerializer.Serialize(clienteReq)));
        resCliente.StatusCode.Should().Be(HttpStatusCode.Created);
        using var cliDoc = JsonDocument.Parse(await resCliente.Content.ReadAsStringAsync());
        return (cliDoc.RootElement.GetProperty("id").GetGuid(), loteId);
    }

    [Fact(DisplayName = "SF-067 / SF-074 (BDD): POST /ventas/financiada 3 gavetas ($150k, 20%, 24m) persiste 24 cuotas exactas y aparece en cartera")]
    public async Task RegistrarFinanciada_Lote150k20Porciento24Meses_DebePersistir24CuotasExactasYAparecerEnCartera()
    {
        var token = await AutenticarVendedorAsync();
        var (clienteId, loteId) = await PrepararClienteYLoteDisponibleAsync(token, 3);

        var payload = JsonSerializer.Serialize(new
        {
            clienteId,
            loteId,
            porcentajeEnganche = 20,
            plazoMeses = 24
        });

        var resPost = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/financiada", token, payload));
        resPost.StatusCode.Should().Be(HttpStatusCode.Created);
        var contrato = await resPost.Content.ReadFromJsonAsync<ContratoFinanciadoResumenDto>();
        contrato.Should().NotBeNull();
        contrato!.PrecioPactado.Should().Be(150000m);
        contrato.CapitalFinanciado.Should().Be(120000m);
        contrato.TotalMensualidades.Should().Be(24);
        contrato.FolioContrato.Should().StartWith("CTR-");

        // Verificar tabla de amortización desde /api/v1/ventas/{id}/amortizacion y cuadre exacto al centavo
        var resAmortizacion = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/ventas/{contrato.VentaId}/amortizacion", token));
        resAmortizacion.StatusCode.Should().Be(HttpStatusCode.OK);
        var cuotas = await resAmortizacion.Content.ReadFromJsonAsync<List<CuotaAmortizacionContratoDto>>();
        cuotas.Should().NotBeNull();
        var listaCuotas = cuotas!;
        listaCuotas.Should().HaveCount(24);
        listaCuotas.Should().OnlyContain(c => c.Estado == "Pendiente");
        listaCuotas.Take(23).Should().OnlyContain(c => c.CuotaTotal == 6107.50m);
        listaCuotas[23].SaldoFinal.Should().Be(0.00m);
        listaCuotas.Sum(c => c.Capital).Should().Be(120000.00m);

        // Verificar que aparece en GET /api/v1/contratos?tab=Activos y GET /api/v1/contratos/{folio}
        var resCartera = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/contratos?tab=Activos&buscar={contrato.FolioContrato}", token));
        resCartera.StatusCode.Should().Be(HttpStatusCode.OK);
        var cartera = await resCartera.Content.ReadFromJsonAsync<CarteraContratosResponseDto>();
        cartera!.Items.Should().Contain(i => i.FolioContrato == contrato.FolioContrato);

        // Segunda venta sobre el mismo lote -> 409 Conflict
        var resDobleVenta = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/financiada", token, payload));
        resDobleVenta.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "SF-069 / SF-074: GET /api/v1/contratos filtra tabs y resuelve detalle por folio CTR-2024-0118 y por UUID")]
    public async Task ContratosCartera_DebeFiltrarTabsYResolverPorFolioYGuid()
    {
        var token = await AutenticarVendedorAsync();

        var resVencidos = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/contratos?tab=Vencidos&buscar=Elena", token));
        resVencidos.StatusCode.Should().Be(HttpStatusCode.OK);
        var vencidos = await resVencidos.Content.ReadFromJsonAsync<CarteraContratosResponseDto>();
        vencidos!.Items.Should().Contain(i =>
            i.FolioContrato == "CTR-2024-0118" &&
            i.Estado == "Vencido" &&
            i.AccionSugerida == "Cobrar");

        var resPorFolio = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/contratos/CTR-2024-0118", token));
        resPorFolio.StatusCode.Should().Be(HttpStatusCode.OK);
        var detalle = await resPorFolio.Content.ReadFromJsonAsync<ContratoDetalleDto>();
        detalle!.FolioContrato.Should().Be("CTR-2024-0118");
        detalle.Amortizacion.Should().HaveCount(24);

        var resPorGuid = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/contratos/{detalle.VentaId}", token));
        resPorGuid.StatusCode.Should().Be(HttpStatusCode.OK);

        var resNotFound = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/contratos/CTR-9999-9999", token));
        resNotFound.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "SF-067 / SF-074: Parámetros fuera de matriz en POST /ventas/financiada retornan 400 Bad Request")]
    public async Task RegistrarFinanciada_ParametrosFueraDeMatriz_DebeRetornar400()
    {
        var token = await AutenticarVendedorAsync();
        var (clienteId, loteId) = await PrepararClienteYLoteDisponibleAsync(token);

        var payloadInvalido = JsonSerializer.Serialize(new
        {
            clienteId,
            loteId,
            porcentajeEnganche = 15,
            plazoMeses = 36
        });

        var res = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/financiada", token, payloadInvalido));
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
