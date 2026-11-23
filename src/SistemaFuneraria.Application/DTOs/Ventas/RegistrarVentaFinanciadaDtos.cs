namespace SistemaFuneraria.Application.DTOs.Ventas;

/// <summary>
/// Payload HTTP para registrar una venta en modalidad financiada a crédito (SF-067).
/// </summary>
public sealed record RegistrarVentaFinanciadaRequest(
    Guid ClienteId,
    Guid LoteId,
    int PorcentajeEnganche,
    int PlazoMeses,
    DateOnly? FechaPrimerPago = null
);

/// <summary>
/// Comando de aplicación para registrar atómicamente una venta financiada con sus N mensualidades (SF-066).
/// </summary>
public sealed record RegistrarVentaFinanciadaCommand(
    Guid ClienteId,
    Guid LoteId,
    string VendedorId,
    int PorcentajeEnganche,
    int PlazoMeses,
    DateOnly? FechaPrimerPago = null
);

/// <summary>
/// Representa una cuota persistida en la tabla de amortización de un contrato financiado (SF-067).
/// </summary>
public sealed record CuotaAmortizacionContratoDto(
    int NumeroCuota,
    DateOnly FechaVencimiento,
    decimal SaldoInicial,
    decimal Capital,
    decimal Interes,
    decimal CuotaTotal,
    decimal SaldoFinal,
    string Estado,
    DateTimeOffset? FechaPago
);

/// <summary>
/// Resumen financiero devuelto tras registrar una venta financiada a crédito (SF-066 / SF-067).
/// </summary>
public sealed record ContratoFinanciadoResumenDto(
    Guid VentaId,
    Guid FinanciamientoId,
    string FolioContrato,
    Guid ClienteId,
    string ClienteNombre,
    Guid LoteId,
    string LoteClave,
    string Modalidad,
    decimal PrecioPactado,
    decimal PorcentajeEnganche,
    decimal MontoEnganche,
    decimal CapitalFinanciado,
    int PlazoMeses,
    decimal TasaInteresAnual,
    decimal CuotaMensualActual,
    decimal SaldoInsoluto,
    string Estado,
    DateTimeOffset FechaVenta,
    int TotalMensualidades
);
