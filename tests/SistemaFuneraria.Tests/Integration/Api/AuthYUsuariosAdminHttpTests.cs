using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Tests.Unit.Api;

namespace SistemaFuneraria.Tests.Integration.Api;

/// <summary>
/// Suite de certificación HTTP Gate v0.2.0 (SF-030):
/// /health, Login, bloqueo al 5.º intento (423), jitter en /olvide-contrasena y RBAC en /api/v1/admin/usuarios.
/// </summary>
[Trait("Ticket", "SF-030")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Integration")]
public class AuthYUsuariosAdminHttpTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly AuthHttpPipelineTests.ApiFactory _factory;
    private readonly HttpClient _client;

    public AuthYUsuariosAdminHttpTests(AuthHttpPipelineTests.ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<LoginResponseDto> AutenticarAsync(HttpClient client, string correo, string password)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(correo, password));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        dto.Should().NotBeNull();
        return dto!;
    }

    [Fact(DisplayName = "SF-030: GET /health debe responder 200 OK")]
    public async Task Health_DebeRetornar200Ok()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SF-030 (BDD): Login Administrador exitoso y control RBAC en /api/v1/admin/usuarios")]
    public async Task AdminUsuarios_DebePermitirAdministradorYRechazarVendedorYCajero()
    {
        var adminLogin = await AutenticarAsync(_client, "andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!");
        adminLogin.Rol.Should().Be("Administrador");

        using var adminReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/usuarios");
        adminReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        var adminRes = await _client.SendAsync(adminReq);
        adminRes.StatusCode.Should().Be(HttpStatusCode.OK);

        foreach (var correoNoAdmin in new[] { "vendedor@sistemafunerario.mx", "cajero@sistemafunerario.mx" })
        {
            var userLogin = await AutenticarAsync(_client, correoNoAdmin, "Sistema Funerario2026!");
            using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/usuarios");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userLogin.Token);
            var res = await _client.SendAsync(req);

            res.StatusCode.Should().Be(HttpStatusCode.Forbidden,
                $"el usuario '{correoNoAdmin}' no tiene rol Administrador");
        }
    }

    [Fact(DisplayName = "SF-030 (BDD): 5 intentos fallidos bloquean la cuenta con 423 Locked (15 minutos)")]
    public async Task Login_AlQuintoIntentoFallido_DebeRetornar423Locked()
    {
        using var isolatedFactory = new AuthHttpPipelineTests.ApiFactory();
        using var client = isolatedFactory.CreateClient();

        for (int intento = 1; intento <= 4; intento++)
        {
            var res = await client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new LoginRequest("cajero@sistemafunerario.mx", "PasswordIncorrecto!"));

            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var dto = await res.Content.ReadFromJsonAsync<LoginResponseDto>();
            dto.Should().NotBeNull();
            dto!.IntentosFallidos.Should().Be(intento);

            if (intento == 3)
            {
                dto.AdvertenciaSeguridad.Should().NotBeNullOrWhiteSpace();
            }
        }

        var lockedRes = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("cajero@sistemafunerario.mx", "PasswordIncorrecto!"));

        lockedRes.StatusCode.Should().Be(HttpStatusCode.Locked);
        var lockedDto = await lockedRes.Content.ReadFromJsonAsync<LoginResponseDto>();
        lockedDto.Should().NotBeNull();
        lockedDto!.MinutosBloqueoRestantes.Should().Be(15);
    }

    [Fact(DisplayName = "SF-030 (BDD): /api/v1/auth/olvide-contrasena aplica jitter anti-timing >= 200ms")]
    public async Task OlvideContrasena_DebeAplicarJitterMinimo200Ms()
    {
        // Warmup de ruta para no medir compilación JIT
        await _client.PostAsJsonAsync("/api/v1/auth/olvide-contrasena", new OlvideContrasenaRequest("warmup@sf.mx"));

        var sw = Stopwatch.StartNew();
        var res = await _client.PostAsJsonAsync(
            "/api/v1/auth/olvide-contrasena",
            new OlvideContrasenaRequest("andrea.morales@sistemafunerario.mx"));
        sw.Stop();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(190,
            "el endpoint debe aplicar jitter pseudoaleatorio de 200ms a 600ms");
    }
}
