using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.DTOs.Dashboard;
using SistemaFuneraria.Application.Servicios.Dashboard;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador REST para el Dashboard Resumen y Reportes Analíticos (SF-088).
/// </summary>
[ApiController]
public class DashboardReportesController(IDashboardReportesService dashboardReportesService) : ApiControllerBase
{
    private readonly IDashboardReportesService _dashboardReportesService = dashboardReportesService;

    /// <summary>
    /// Obtiene KPIs, serie de 6 meses, disponibilidad por gavetas y actividad reciente para /Resumen (SF-088).
    /// </summary>
    [HttpGet("api/v1/dashboard/resumen")]
    [Authorize]
    [ProducesResponseType(typeof(DashboardResumenDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerResumenDashboard(
        [FromQuery] string? rango = null,
        CancellationToken ct = default)
    {
        var resultado = await _dashboardReportesService.ObtenerResumenDashboardAsync(rango, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Obtiene indicadores analíticos por periodo y sucursal para /Reportes (SF-088). Exclusivo Administrador.
    /// </summary>
    [HttpGet("api/v1/reportes/indicadores")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ReportesIndicadoresDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObtenerIndicadoresReportes(
        [FromQuery] string? periodo = null,
        [FromQuery] string? sucursal = null,
        CancellationToken ct = default)
    {
        var resultado = await _dashboardReportesService.ObtenerIndicadoresReportesAsync(periodo, sucursal, ct);
        return FromResult(resultado);
    }
}
