using SistemaFuneraria.Application.DTOs.Ventas;

namespace SistemaFuneraria.Application.DTOs.Contratos;

/// <summary>
/// KPIs agregados de la cartera de contratos para la pantalla /Contratos (SF-068).
/// </summary>
public sealed record CarteraKpisDto(
    int TotalActivos,
    decimal MontoEnCartera,
    int TotalAlCorriente,
    decimal PorcentajeAlCorriente,
    int TotalConAtraso,
    decimal MontoPorRecuperar,
    int TotalLiquidadosAnio
);

/// <summary>
/// Fila proyectada para la tabla de cartera de contratos (SF-068 / SF-070).
/// </summary>
public sealed record ContratoCarteraItemDto(
    Guid VentaId,
    string FolioContrato,
    string ClienteNombre,
    string LoteClave,
    string Modalidad,
    decimal SaldoInsoluto,
    DateOnly? ProximoPagoFecha,
    decimal? ProximoPagoMonto,
    string ProximoPagoTexto,
    string Estado,
    string AccionSugerida
);

/// <summary>
/// Respuesta paginada de cartera de contratos con sus 4 KPIs (SF-068 / SF-069).
/// </summary>
public sealed record CarteraContratosResponseDto(
    CarteraKpisDto Kpis,
    IReadOnlyList<ContratoCarteraItemDto> Items,
    int TotalRegistros,
    int Pagina,
    int TamanoPagina
);

/// <summary>
/// Detalle financiero de un contrato a crédito (SF-069 / SF-071).
/// </summary>
public sealed record ContratoFinanciamientoDetalleDto(
    Guid FinanciamientoId,
    decimal PorcentajeEnganche,
    decimal MontoEnganche,
    decimal CapitalFinanciado,
    int PlazoMeses,
    decimal TasaInteresAnual,
    decimal CuotaMensualActual,
    decimal SaldoInsoluto,
    string Estado
);

/// <summary>
/// Expediente completo de un contrato comercial consultado por UUID o folio CTR-YYYY-XXXX (SF-069).
/// </summary>
public sealed record ContratoDetalleDto(
    Guid VentaId,
    string FolioContrato,
    Guid ClienteId,
    string ClienteNombre,
    Guid LoteId,
    string LoteClave,
    string LoteSeccion,
    string LoteFila,
    int LoteGavetas,
    string Modalidad,
    decimal PrecioPactado,
    string Estado,
    string EstadoVisual,
    DateTimeOffset FechaVenta,
    string? FolioPago,
    ContratoFinanciamientoDetalleDto? Financiamiento,
    IReadOnlyList<CuotaAmortizacionContratoDto> Amortizacion,
    string? VendedorNombre = null,
    string? FirmaClienteBase64 = null,
    DateTimeOffset? FechaFirma = null
);
