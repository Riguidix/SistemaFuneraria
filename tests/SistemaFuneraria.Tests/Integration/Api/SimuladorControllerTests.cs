using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Application.DTOs.Simulador;
using SistemaFuneraria.Tests.Unit.Api;
using Xunit;

namespace SistemaFuneraria.Tests.Integration.Api;

[Trait("Ticket", "SF-055")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Integration")]
public class SimuladorControllerTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public SimuladorControllerTests(AuthHttpPipelineTests.ApiFactory factory)
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

    [Fact(DisplayName = "SF-055 / CP-03: Cotizar crédito 12 meses 2 gavetas debe calcular amortización exacta")]
    public async Task CP03_CotizarCredito_12Meses2Gavetas_DebeCalcularCuotasYAmortizacionExacta()
    {
        var token = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var body = new SimularCotizacionRequest(null, 2, "Credito", 20, 12, new DateOnly(2026, 12, 1));

        var response = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/simulador/cotizar", token, body));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<SimulacionCotizacionDto>();
        dto.Should().NotBeNull();
        dto!.PrecioLista.Should().Be(120_000.00m);
        dto.MontoEnganche.Should().Be(24_000.00m);
        dto.SaldoAFinanciar.Should().Be(96_000.00m);
        dto.PlazoMeses.Should().Be(12);
        dto.TasaAnualPorcentaje.Should().Be(16.00m);
        dto.MensualidadEstimada.Should().Be(8_710.16m);
        dto.TablaAmortizacion.Should().HaveCount(12);
        dto.TablaAmortizacion[0].CuotaTotal.Should().Be(8_710.16m);
        dto.TablaAmortizacion[11].CuotaTotal.Should().Be(8_710.19m);
        dto.TablaAmortizacion.Sum(f => f.Capital).Should().Be(96_000.00m);
        dto.TablaAmortizacion.Last().SaldoFinal.Should().Be(0.00m);
    }

    [Fact(DisplayName = "SF-055 / CP-04: Cotizar crédito 24 meses 2 gavetas debe calcular mensualidad exacta ($4,886.00)")]
    public async Task CP04_CotizarCredito_24Meses2Gavetas_DebeCoincidirConCaptura7Figma()
    {
        var token = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var body = new SimularCotizacionRequest(null, 2, "Credito", 20, 24, new DateOnly(2026, 12, 1));

        var response = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/simulador/cotizar", token, body));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<SimulacionCotizacionDto>();
        dto.Should().NotBeNull();
        dto!.PrecioLista.Should().Be(120_000.00m);
        dto.MontoEnganche.Should().Be(24_000.00m);
        dto.SaldoAFinanciar.Should().Be(96_000.00m);
        dto.PlazoMeses.Should().Be(24);
        dto.TasaAnualPorcentaje.Should().Be(20.00m);
        dto.MensualidadEstimada.Should().Be(4_886.00m);
        dto.TablaAmortizacion.Should().HaveCount(24);
        dto.TablaAmortizacion.Sum(f => f.Capital).Should().Be(96_000.00m);
        dto.TablaAmortizacion.Last().SaldoFinal.Should().Be(0.00m);
    }

    [Fact(DisplayName = "SF-055 / CP-05: Cotizar crédito 24 meses 3 gavetas debe calcular amortización exacta")]
    public async Task CP05_CotizarCredito_24Meses3Gavetas_DebeCalcularCuotasYAmortizacionExacta()
    {
        var token = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var body = new SimularCotizacionRequest(null, 3, "Credito", 20, 24, new DateOnly(2026, 12, 1));

        var response = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/simulador/cotizar", token, body));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<SimulacionCotizacionDto>();
        dto.Should().NotBeNull();
        dto!.PrecioLista.Should().Be(150_000.00m);
        dto.MontoEnganche.Should().Be(30_000.00m);
        dto.SaldoAFinanciar.Should().Be(120_000.00m);
        dto.PlazoMeses.Should().Be(24);
        dto.TasaAnualPorcentaje.Should().Be(20.00m);
        dto.MensualidadEstimada.Should().Be(6_107.50m);
        dto.TablaAmortizacion.Should().HaveCount(24);
        dto.TablaAmortizacion.Sum(f => f.Capital).Should().Be(120_000.00m);
        dto.TablaAmortizacion.Last().SaldoFinal.Should().Be(0.00m);
    }

    [Fact(DisplayName = "SF-055 / CP-06: Oráculo de las 36 combinaciones financieras con suma de capital exacta")]
    public async Task CP06_Oraculo36Combinaciones_TodasDebenRetornar200OkYCapitalCuadrarExacto()
    {
        var token = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var gavetasLista = new[] { 2, 3, 4 };
        var enganchesLista = new[] { 10m, 20m, 30m };
        var plazosLista = new[] { 12, 18, 24, 48 };

        int combinacionesProbadas = 0;

        foreach (var gavetas in gavetasLista)
        {
            foreach (var enganche in enganchesLista)
            {
                foreach (var plazo in plazosLista)
                {
                    var body = new SimularCotizacionRequest(null, gavetas, "Credito", enganche, plazo, new DateOnly(2026, 12, 1));
                    var response = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/simulador/cotizar", token, body));

                    response.StatusCode.Should().Be(HttpStatusCode.OK);
                    var dto = await response.Content.ReadFromJsonAsync<SimulacionCotizacionDto>();
                    dto.Should().NotBeNull();
                    dto!.TablaAmortizacion.Should().HaveCount(plazo);
                    dto.TablaAmortizacion.Sum(f => f.Capital).Should().Be(dto.SaldoAFinanciar);
                    dto.TablaAmortizacion.Last().SaldoFinal.Should().Be(0.00m);

                    combinacionesProbadas++;
                }
            }
        }

        combinacionesProbadas.Should().Be(36);
    }

    [Theory(DisplayName = "SF-055 / CP-10: Rechazo de parámetros no autorizados con 400 Bad Request")]
    [InlineData(2, "Credito", 15, 12)]
    [InlineData(2, "Credito", 25, 24)]
    [InlineData(2, "Credito", 100, 24)]
    [InlineData(2, "Credito", 20, 0)]
    [InlineData(2, "Credito", 20, 15)]
    [InlineData(2, "Credito", 20, 60)]
    [InlineData(1, "Credito", 20, 12)]
    [InlineData(5, "Credito", 20, 12)]
    [InlineData(2, "Arrendamiento", 20, 12)]
    public async Task CP10_FronterasInvalidas_DebeRetornar400BadRequest(int gavetas, string modalidad, decimal enganche, int plazo)
    {
        var token = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var body = new SimularCotizacionRequest(null, gavetas, modalidad, enganche, plazo, new DateOnly(2026, 12, 1));

        var response = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/simulador/cotizar", token, body));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "SF-055: GET /api/v1/simulador/planes debe retornar 200 OK con catálogo oficial")]
    public async Task ObtenerPlanes_DebeRetornar200OkConPlanesAutorizados()
    {
        var token = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var response = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/simulador/planes", token));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<PlanesFinanciamientoDto>();
        dto.Should().NotBeNull();
        dto!.PorcentajesEnganche.Should().BeEquivalentTo(new[] { 10, 20, 30 });
        dto.Plazos.Should().HaveCount(4);
    }

    [Fact(DisplayName = "SF-055: Cotizar modalidad Contado debe retornar 200 OK sin amortización")]
    public async Task CotizarContado_DebeRetornar200OkSinAmortizacion()
    {
        var token = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var body = new SimularCotizacionRequest(null, 2, "Contado", 0, 0, new DateOnly(2026, 12, 1));

        var response = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/simulador/cotizar", token, body));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<SimulacionCotizacionDto>();
        dto.Should().NotBeNull();
        dto!.Modalidad.Should().Be("Contado");
        dto.MontoEnganche.Should().Be(120_000.00m);
        dto.SaldoAFinanciar.Should().Be(0.00m);
        dto.TablaAmortizacion.Should().BeEmpty();
    }

    [Fact(DisplayName = "SF-055: Endpoints del simulador sin token deben retornar 401 Unauthorized")]
    public async Task Seguridad_SinToken_DebeRetornar401Unauthorized()
    {
        var responsePlanes = await _client.GetAsync("/api/v1/simulador/planes");
        responsePlanes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = new SimularCotizacionRequest(null, 2, "Credito", 20, 12, new DateOnly(2026, 12, 1));
        var responseCotizar = await _client.PostAsJsonAsync("/api/v1/simulador/cotizar", body);
        responseCotizar.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "SF-055: Cotizar con LoteId existente debe resolver gavetas desde inventario y retornar 200 OK")]
    public async Task Cotizar_ConLoteIdExistente_DebeResolverGavetasYRetornar200Ok()
    {
        var token = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");

        var resLotes = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/lotes?buscar=JN-A-042", token));
        resLotes.StatusCode.Should().Be(HttpStatusCode.OK);

        using var lotesDoc = JsonDocument.Parse(await resLotes.Content.ReadAsStringAsync());
        var items = lotesDoc.RootElement.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);

        var loteId = items[0].GetProperty("id").GetGuid();
        var body = new SimularCotizacionRequest(loteId, null, "Credito", 20, 24, new DateOnly(2026, 12, 1));

        var response = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/simulador/cotizar", token, body));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<SimulacionCotizacionDto>();
        dto.Should().NotBeNull();
        dto!.PrecioLista.Should().Be(120_000.00m);
        dto.MensualidadEstimada.Should().Be(4_886.00m);
        dto.TablaAmortizacion.Sum(f => f.Capital).Should().Be(96_000.00m);
    }

    [Fact(DisplayName = "SF-055: Cotizar con LoteId inexistente debe retornar 404 Not Found")]
    public async Task Cotizar_ConLoteIdInexistente_DebeRetornar404NotFound()
    {
        var token = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var body = new SimularCotizacionRequest(Guid.NewGuid(), null, "Credito", 20, 24, new DateOnly(2026, 12, 1));

        var response = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/simulador/cotizar", token, body));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
