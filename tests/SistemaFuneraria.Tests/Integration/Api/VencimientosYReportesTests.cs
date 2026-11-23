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
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Application.DTOs.Cobranza;
using SistemaFuneraria.Application.DTOs.Configuracion;
using SistemaFuneraria.Application.DTOs.Dashboard;
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

    [Fact(DisplayName = "SF-094 / CP-09: FakeTimeProvider marca cuota Vencida con $0.00 de mora y actualiza Dashboard/Caja")]
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

        var resResumenCaja = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/cobranza/resumen-caja", cajeroToken));
        resResumenCaja.StatusCode.Should().Be(HttpStatusCode.OK);
        var resumenCajaDto = (await resResumenCaja.Content.ReadFromJsonAsync<CorteDelDiaDto>())!;
        resumenCajaDto.PagosVencidosCount.Should().BeGreaterThanOrEqualTo(1);

        var resDashboard = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/dashboard/resumen", vendedorToken));
        resDashboard.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboardDto = (await resDashboard.Content.ReadFromJsonAsync<DashboardResumenDto>())!;
        dashboardDto.Kpis.PagosVencidosCount.Should().BeGreaterThanOrEqualTo(1);

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

    [Fact(DisplayName = "SF-088 / SF-094: Dashboard /resumen y Reportes /indicadores retornan métricas reales y validan RBAC")]
    public async Task DashboardYReportes_DebenRetornarMetricasRealesYValidarRbac()
    {
        var adminToken = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");
        var vendedorToken = await AutenticarComoAsync("vendedor@sistemafunerario.mx", "Sistema Funerario2026!");

        var resDash = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/dashboard/resumen", vendedorToken));
        resDash.StatusCode.Should().Be(HttpStatusCode.OK);
        var dash = (await resDash.Content.ReadFromJsonAsync<DashboardResumenDto>())!;
        dash.VentasYCobranza.Should().HaveCount(6);
        dash.Disponibilidad.TotalLotes.Should().BeGreaterThan(0);
        dash.ActividadReciente.Should().NotBeEmpty();

        var resRepVendedor = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/reportes/indicadores", vendedorToken));
        resRepVendedor.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var resRepAdmin = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/reportes/indicadores?periodo=Noviembre+2025&sucursal=Jardines+del+Valle", adminToken));
        resRepAdmin.StatusCode.Should().Be(HttpStatusCode.OK);
        var rep = (await resRepAdmin.Content.ReadFromJsonAsync<ReportesIndicadoresDto>())!;
        rep.Sucursal.Should().Be("Jardines del Valle");
        rep.VentasPorTipoLote.Should().HaveCount(3);
        rep.MetaMensual.ObjetivoMensual.Should().Be(600_000m);
    }

    [Fact(DisplayName = "SF-091 / SF-094: Configuracion sucursal, creación de Cajero con login inmediato y prevención auto-bloqueo Admin")]
    public async Task ConfiguracionSucursalYUsuarios_DebePersistirYPrevenirAutoBloqueo()
    {
        var adminToken = await AutenticarComoAsync("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");
        var cajeroToken = await AutenticarComoAsync("cajero@sistemafunerario.mx", "Sistema Funerario2026!");

        var resCajeroDenied = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/configuracion/sucursal", cajeroToken));
        resCajeroDenied.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var resSuc = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/configuracion/sucursal", adminToken));
        resSuc.StatusCode.Should().Be(HttpStatusCode.OK);
        var suc = (await resSuc.Content.ReadFromJsonAsync<ConfiguracionSucursalDto>())!;
        suc.NombreComercial.Should().Be("Jardines del Valle");

        var putSuc = JsonSerializer.Serialize(new ActualizarConfiguracionSucursalRequest(
            NombreComercial: "Jardines del Valle Matriz",
            TelefonoAtencion: "55 4120 9900",
            CorreoOperativo: "matriz@sistemafunerario.mx",
            Direccion: "Av. Paseo de los Cipreses 420, Zapopan",
            Ciudad: "Zapopan",
            CodigoPostal: "45010",
            RecordatoriosPago: true,
            ResumenDiarioCaja: false));
        var resPut = await _client.SendAsync(CrearRequest(HttpMethod.Put, "/api/v1/configuracion/sucursal", adminToken, putSuc));
        resPut.StatusCode.Should().Be(HttpStatusCode.OK);
        var sucAct = (await resPut.Content.ReadFromJsonAsync<ConfiguracionSucursalDto>())!;
        sucAct.NombreComercial.Should().Be("Jardines del Valle Matriz");
        sucAct.ResumenDiarioCaja.Should().BeFalse();

        // Crear nuevo usuario Cajero e iniciar sesión inmediatamente
        var nuevoCorreo = $"cajero.{Guid.NewGuid():N}"[..16] + "@sistemafunerario.mx";
        var crearUserBody = JsonSerializer.Serialize(new CrearUsuarioConfiguracionRequest(
            Nombre: "Mario Nuevo",
            Correo: nuevoCorreo,
            Password: "CajeroNuevo2026!",
            Rol: "Cajero"));
        var resCrearUser = await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/configuracion/usuarios", adminToken, crearUserBody));
        resCrearUser.StatusCode.Should().Be(HttpStatusCode.Created);

        var nuevoToken = await AutenticarComoAsync(nuevoCorreo, "CajeroNuevo2026!");
        nuevoToken.Should().NotBeNullOrWhiteSpace();

        // Prevención de auto-bloqueo del propio Administrador en sesión
        var resUsuarios = await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/configuracion/usuarios", adminToken));
        var listaUsuarios = (await resUsuarios.Content.ReadFromJsonAsync<List<UsuarioSistemaDto>>())!;
        var adminUser = listaUsuarios.First(u => u.Correo == "andrea.morales@sistemafunerario.mx");

        var resSelfDisable = await _client.SendAsync(CrearRequest(
            HttpMethod.Patch,
            $"/api/v1/configuracion/usuarios/{adminUser.Id}/estado",
            adminToken,
            JsonSerializer.Serialize(new CambiarEstadoUsuarioRequest(Activo: false))));
        resSelfDisable.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
