using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Dashboard;

namespace SistemaFuneraria.Application.Servicios.Dashboard;

/// <summary>
/// Servicio de métricas ejecutivas para Dashboard Resumen (SF-089) y Reportes Analíticos (SF-090).
/// </summary>
public interface IDashboardReportesService
{
    Task<Resultado<DashboardResumenDto>> ObtenerResumenDashboardAsync(
        CancellationToken cancellationToken = default);

    Task<Resultado<ReportesIndicadoresDto>> ObtenerIndicadoresReportesAsync(
        string? periodo = null,
        string? sucursal = null,
        CancellationToken cancellationToken = default);
}
