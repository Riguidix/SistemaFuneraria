using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Tests.Unit.Api;

namespace SistemaFuneraria.Tests.Integration.Api;

[Trait("Ticket", "SF-047")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Integration")]
public class LotesControllerTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public LotesControllerTests(AuthHttpPipelineTests.ApiFactory factory)
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

    [Fact(DisplayName = "SF-047 / CP-01 (BDD): Filtro combinado de lotes disponibles por sección y gavetas")]
    public async Task CP01_FiltroCombinadoLotesDisponibles_DebeRetornarLotesConCriteriosExactos()
    {
        var token = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");

        using var req = CrearRequest(HttpMethod.Get, "/api/v1/lotes?seccion=Jardín Norte&estado=Disponible&gavetas=2", token);
        var res = await _client.SendAsync(req);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var items = root.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);

        foreach (var item in items.EnumerateArray())
        {
            item.GetProperty("seccion").GetString().Should().Be("Jardín Norte");
            item.GetProperty("estado").GetString().Should().Be("Disponible");
            item.GetProperty("gavetas").GetInt32().Should().Be(2);
            item.GetProperty("precioContado").GetDecimal().Should().Be(120000m);
        }
    }

    [Fact(DisplayName = "SF-047 (BDD): Consulta del catálogo debe retornar los 3 KPIs de resumen calculados")]
    public async Task ResumenKpis_DebeCalcularDisponiblesPrecioDesdeYSeccionConMasEspacio()
    {
        var token = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");

        using var req = CrearRequest(HttpMethod.Get, "/api/v1/lotes", token);
        var res = await _client.SendAsync(req);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("totalDisponibles").GetInt32().Should().BeGreaterThan(0);
        root.GetProperty("precioDesde").GetDecimal().Should().Be(120000m);
        root.GetProperty("seccionMasEspacio").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "SF-047 (BDD): Búsqueda sin coincidencias debe retornar 200 OK con lista vacía")]
    public async Task BusquedaSinCoincidencias_DebeRetornar200OkConItemsVacio()
    {
        var token = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");

        using var req = CrearRequest(HttpMethod.Get, "/api/v1/lotes?buscar=InexistenteXYZ999", token);
        var res = await _client.SendAsync(req);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var items = root.GetProperty("items");
        items.GetArrayLength().Should().Be(0);
    }

    [Fact(DisplayName = "SF-047 (BDD): RBAC en creación de lotes: Vendedor 403 Forbidden y Administrador 201 Created")]
    public async Task SeguridadRbac_CrearLote_VendedorDebeRecibir403YAdminDebeCrear()
    {
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var adminToken = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");

        var nuevoLote = new
        {
            Clave = "RO-C-099",
            Seccion = "Robles",
            AndadorFila = "Andador C · Fila 9",
            Gavetas = 4,
            PrecioContado = 180000m
        };

        using var reqVendedor = CrearRequest(HttpMethod.Post, "/api/v1/lotes", vendedorToken, nuevoLote);
        var resVendedor = await _client.SendAsync(reqVendedor);
        resVendedor.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using var reqAdmin = CrearRequest(HttpMethod.Post, "/api/v1/lotes", adminToken, nuevoLote);
        var resAdmin = await _client.SendAsync(reqAdmin);
        resAdmin.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
