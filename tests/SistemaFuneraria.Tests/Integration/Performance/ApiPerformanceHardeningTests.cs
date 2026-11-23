using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Tests.Unit.Api;
using Xunit;

namespace SistemaFuneraria.Tests.Integration.Performance;

[Trait("Ticket", "SF-095")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Performance")]
public class ApiPerformanceHardeningTests : IClassFixture<AuthHttpPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public ApiPerformanceHardeningTests(AuthHttpPipelineTests.ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> AutenticarAsync()
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
        var req = new HttpRequestMessage(metodo, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (jsonBody is not null)
        {
            req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }
        return req;
    }

    [Fact(DisplayName = "SF-095 (RNF-01 / RNF-02): P95 de lectura (/lotes, /contratos, /dashboard/resumen) y simulador < 200ms")]
    public async Task LecturaYSimulador_Percentil95_DebeSerMenorA200Ms()
    {
        var token = await AutenticarAsync();
        var cotizarBody = JsonSerializer.Serialize(new
        {
            gavetas = 3,
            modalidad = "Credito",
            porcentajeEnganche = 20,
            plazoMeses = 24
        });

        // Warm-up inicial para evitar sesgo JIT/EF model build en la medición
        (await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/lotes?pageSize=20", token))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/contratos?pageSize=20", token))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.SendAsync(CrearRequest(HttpMethod.Get, "/api/v1/dashboard/resumen", token))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.SendAsync(CrearRequest(HttpMethod.Post, "/api/v1/simulador/cotizar", token, cotizarBody))).StatusCode.Should().Be(HttpStatusCode.OK);

        var endpoints = new (HttpMethod Metodo, string Url, string? Body)[]
        {
            (HttpMethod.Get, "/api/v1/lotes?pageSize=20", null),
            (HttpMethod.Get, "/api/v1/contratos?pageSize=20", null),
            (HttpMethod.Get, "/api/v1/dashboard/resumen", null),
            (HttpMethod.Post, "/api/v1/simulador/cotizar", cotizarBody)
        };

        var tiemposMs = new List<double>();
        var sw = new Stopwatch();

        for (int i = 0; i < 10; i++)
        {
            foreach (var (metodo, url, body) in endpoints)
            {
                sw.Restart();
                var res = await _client.SendAsync(CrearRequest(metodo, url, token, body));
                sw.Stop();
                res.StatusCode.Should().Be(HttpStatusCode.OK);
                tiemposMs.Add(sw.Elapsed.TotalMilliseconds);
            }
        }

        tiemposMs.Sort();
        int indexP95 = (int)Math.Ceiling(tiemposMs.Count * 0.95) - 1;
        double p95 = tiemposMs[Math.Clamp(indexP95, 0, tiemposMs.Count - 1)];

        p95.Should().BeLessThan(200.0, $"el P95 medido fue {p95:F2}ms");
    }
}
