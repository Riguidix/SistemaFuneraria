using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SistemaFuneraria.Application.DTOs.Admin;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Application.DTOs.Cobranza;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Integration.Api;

[Trait("Ticket", "SF-094")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Integration")]
public class VencimientosYReportesTests : IClassFixture<VencimientosYReportesTests.TimeControlledApiFactory>
{
    private readonly TimeControlledApiFactory _factory;
    private readonly HttpClient _client;

    public VencimientosYReportesTests(TimeControlledApiFactory factory)
    {
        _factory = factory;
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
            Nombre: "Mariana",
            Apellidos: "Solís Vega",
            Curp: $"SF94{sufijo}"[..18],
            Telefono: "5544332211",
            Correo: $"mariana.{sufijo[..8].ToLowerInvariant()}@ejemplo.mx",
            Direccion: "Av. Insurgentes Sur 450, Benito Juárez, CDMX");

        var resCli = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/clientes", vendedorToken, JsonSerializer.Serialize(cliReq)));
        resCli.StatusCode.Should().Be(HttpStatusCode.Created);
        using var cliDoc = JsonDocument.Parse(await resCli.Content.ReadAsStringAsync());
        var clienteId = cliDoc.RootElement.GetProperty("id").GetGuid();

        var ventaBody = JsonSerializer.Serialize(new { clienteId, loteId, porcentajeEnganche = 20, plazoMeses = 12 });
        var resVenta = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/ventas/financiada", vendedorToken, ventaBody));
        resVenta.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resVenta.Content.ReadFromJsonAsync<ContratoFinanciadoResumenDto>())!;
    }

    [Fact(DisplayName = "SF-094 / CP-09: Cuota atrasada evaluada con FakeTimeProvider debe marcarse Vencida con $0.00 de mora")]
    public async Task CP09_CuotaAtrasada_ConFakeTimeProvider_DebeMarcarVencidaYSinRecargoMoratorio()
    {
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");
        var cajeroToken = await AutenticarComoAsync("cajero@sistemafunerario.mx", "Sistema Funerario2026!");

        var contrato = await CrearContratoFinanciadoAsync(vendedorToken);

        var resCajaInicial = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{contrato.FolioContrato}", cajeroToken));
        resCajaInicial.StatusCode.Should().Be(HttpStatusCode.OK);
        var cajaInicialDto = (await resCajaInicial.Content.ReadFromJsonAsync<ContratoCajaDto>())!;
        cajaInicialDto.CuotasVencidas.Should().Be(0);
        cajaInicialDto.EstadoVisual.Should().Be("Al corriente");
        decimal cuotaOriginal = cajaInicialDto.MontoMensualidad;
        cuotaOriginal.Should().BeGreaterThan(0m);

        _factory.AvanzarDias(45);

        var resCajaVencida = await _client.SendAsync(CrearRequest(HttpMethod.Get, $"/api/v1/cobranza/contrato/{contrato.FolioContrato}", cajeroToken));
        resCajaVencida.StatusCode.Should().Be(HttpStatusCode.OK);
        var cajaVencidaDto = (await resCajaVencida.Content.ReadFromJsonAsync<ContratoCajaDto>())!;

        cajaVencidaDto.CuotasVencidas.Should().BeGreaterThanOrEqualTo(1);
        cajaVencidaDto.EstadoVisual.Should().Contain("vencido");
        cajaVencidaDto.MontoMensualidad.Should().Be(cuotaOriginal);

        var resResumen = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/cobranza/resumen-caja", cajeroToken));
        resResumen.StatusCode.Should().Be(HttpStatusCode.OK);
        var resumenDto = (await resResumen.Content.ReadFromJsonAsync<CorteDelDiaDto>())!;
        resumenDto.PagosVencidosCount.Should().BeGreaterThanOrEqualTo(1);

        var bodyPago = JsonSerializer.Serialize(new
        {
            folioContrato = contrato.FolioContrato,
            importeRecibido = cuotaOriginal,
            metodoPago = "Efectivo",
            numeroCuota = 1
        });

        var resPago = await _client.SendAsync(CrearRequest(
            HttpMethod.Post,
            "/api/v1/cobranza/mensualidad",
            cajeroToken,
            bodyPago,
            Guid.NewGuid().ToString()));

        resPago.StatusCode.Should().Be(HttpStatusCode.OK);
        var recibo = await resPago.Content.ReadFromJsonAsync<ReciboPagoDto>();
        recibo.Should().NotBeNull();
        recibo!.MontoTotal.Should().Be(cuotaOriginal);
    }

    [Fact(DisplayName = "SF-094: Consulta de cartera y contratos activos retorna 200 OK")]
    public async Task Cartera_ConsultaContratos_DebeListarConEstadoCorrecto()
    {
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");

        var resContratos = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/contratos", vendedorToken));
        resContratos.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SF-094: Administración de usuarios y roles requiere rol Administrador (403 Forbidden para Cajero)")]
    public async Task AdminUsuarios_ControlDeAcceso_DebePermitirAdminYRechazarCajero()
    {
        var adminToken = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");
        var cajeroToken = await AutenticarComoAsync("cajero@sistemafunerario.mx", "Sistema Funerario2026!");

        var resAdmin = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/admin/usuarios", adminToken));
        resAdmin.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuarios = await resAdmin.Content.ReadFromJsonAsync<List<UsuarioSistemaDto>>();
        usuarios.Should().NotBeNull();
        usuarios!.Should().NotBeEmpty();

        var resCajero = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/admin/usuarios", cajeroToken));
        resCajero.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public sealed class TimeControlledApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"VencimientosApi_{Guid.NewGuid():N}";
        private readonly MutableTimeProvider _timeProvider = new(DateTimeOffset.UtcNow);

        public void AvanzarDias(int dias) => _timeProvider.AvanzarDias(dias);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<AppDbContext>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));

                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(_timeProvider);
            });
        }
    }

    public sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        public void AvanzarDias(int dias)
        {
            _utcNow = _utcNow.AddDays(dias);
        }
    }
}
