namespace SistemaFuneraria.Application.DTOs.Dashboard;

public sealed record DashboardKpisDto(
    decimal VentasDelMes,
    decimal CobranzaMensual,
    int ContratosActivos,
    int ContratosNuevosMes,
    int PagosVencidosCount,
    decimal MontoPorRecuperar,
    decimal? VariacionVentasPct = null,
    decimal? VariacionCobranzaPct = null
);

public sealed record SerieMensualDto(
    string Mes,
    decimal Ventas,
    decimal Cobranza
);

public sealed record DisponibilidadInventarioDto(
    int TotalLotes,
    int TotalDisponibles,
    int Gavetas2,
    int Gavetas3,
    int Gavetas4,
    decimal PorcentajeOcupacion
);

public sealed record ActividadRecienteDto(
    string Cliente,
    string Email,
    string Iniciales,
    string Movimiento,
    string Referencia,
    decimal Importe,
    string Estado,
    string FechaRelativa,
    string? FolioContrato = null
);

public sealed record DashboardResumenDto(
    DashboardKpisDto Kpis,
    IReadOnlyList<SerieMensualDto> VentasYCobranza,
    DisponibilidadInventarioDto Disponibilidad,
    IReadOnlyList<ActividadRecienteDto> ActividadReciente
);

public sealed record VentaPorTipoLoteDto(
    string TipoLote,
    int Gavetas,
    int Unidades,
    decimal MontoTotal,
    decimal Porcentaje
);

public sealed record MetaMensualDto(
    decimal ObjetivoMensual,
    decimal Acumulado,
    decimal PorcentajeAvance
);

public sealed record ReportesIndicadoresDto(
    string Periodo,
    string Sucursal,
    decimal ConversionCotizacion,
    decimal TicketPromedio,
    decimal IndiceMorosidad,
    IReadOnlyList<VentaPorTipoLoteDto> VentasPorTipoLote,
    MetaMensualDto MetaMensual
);
