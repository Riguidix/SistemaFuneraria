using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Infrastructure.Persistencia;

namespace SistemaFuneraria.Tests.Unit.Api;

/// <summary>
/// Verifica el pipeline HTTP completo de autenticación JWT Bearer (SF-023):
/// POST /api/v1/auth/login -> JWT -> GET /api/v1/auth/me.
/// </summary>
[Trait("Ticket", "SF-023")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Unit")]
public class AuthHttpPipelineTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public AuthHttpPipelineTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "SF-023 HTTP: GET /api/v1/auth/me sin token debe responder 401 Unauthorized")]
    public async Task Me_SinToken_DebeRetornar401Unauthorized()
    {
        var response = await _client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "SF-023 HTTP: Login emite JWT válido aceptado por GET /api/v1/auth/me")]
    public async Task Login_Y_Me_ConBearerJwtReal_DebeRetornar200OkConPerfil()
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("andrea.morales@sistemafunerario.mx", "Sistema Funerario2026!"));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginDto = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginDto.Should().NotBeNull();
        loginDto!.Token.Should().NotBeNullOrWhiteSpace();
        loginDto.Rol.Should().Be("Administrador");

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginDto.Token);

        var meResponse = await _client.SendAsync(meRequest);
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var perfil = await meResponse.Content.ReadFromJsonAsync<UsuarioPerfilDto>();
        perfil.Should().NotBeNull();
        perfil!.Correo.Should().Be("andrea.morales@sistemafunerario.mx");
        perfil.NombreCompleto.Should().Be("Andrea Morales");
        perfil.Rol.Should().Be("Administrador");
    }

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"AuthHttpPipeline_{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<AppDbContext>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));
            });
        }
    }
}
