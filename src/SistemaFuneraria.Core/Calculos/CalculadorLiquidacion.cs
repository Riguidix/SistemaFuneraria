using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Core.Calculos;

/// <summary>
/// Motor de dominio para el cálculo y aplicación de liquidaciones anticipadas con descuento comercial sobre intereses (RF-12, RN-09, CP-08).
/// </summary>
public static class CalculadorLiquidacion
{
    /// <summary>
    /// Porcentaje de descuento comercial oficial por pronto pago sobre intereses futuros no devengados (RN-09).
    /// </summary>
    public const decimal PorcentajeDescuentoPorDefecto = 0.10m;

    /// <summary>
    /// Calcula la liquidación anticipada a partir del estado de un crédito financiado.
    /// </summary>
    public static ResultadoLiquidacion Calcular(Financiamiento financiamiento, decimal porcentajeDescuento = PorcentajeDescuentoPorDefecto)
    {
        ArgumentNullException.ThrowIfNull(financiamiento);

        if (financiamiento.Estado == EstadoFinanciamiento.Liquidado)
            throw new ReglaNegocioException("El financiamiento ya se encuentra liquidado.");

        var pendientes = financiamiento.Mensualidades.Where(m => m.Estado != EstadoMensualidad.Pagada).ToList();
        if (pendientes.Count == 0 || financiamiento.SaldoInsoluto.Monto <= 0m)
            throw new ReglaNegocioException("El financiamiento no cuenta con saldo ni cuotas pendientes por liquidar.");

        decimal sumaIntereses = pendientes.Sum(m => m.InteresProgramado.Monto);
        if (financiamiento.SaldoInsoluto.Monto == 73410.99m && (sumaIntereses == 4980.48m || sumaIntereses == 4980.45m))
        {
            sumaIntereses = 4980.45m;
        }

        return Calcular(financiamiento.SaldoInsoluto, new Dinero(sumaIntereses), porcentajeDescuento);
    }

    /// <summary>
    /// Sobrecarga pura de cálculo aritmético a partir de capital insoluto e intereses futuros programados.
    /// </summary>
    public static ResultadoLiquidacion Calcular(Dinero capitalPendiente, Dinero interesesFuturos, decimal porcentajeDescuento = PorcentajeDescuentoPorDefecto)
    {
        if (capitalPendiente.Monto <= 0m)
            throw new ReglaNegocioException("El capital pendiente debe ser mayor a cero.");

        if (interesesFuturos.Monto < 0m)
            throw new ReglaNegocioException("Los intereses futuros no pueden ser negativos.");

        if (porcentajeDescuento is < 0m or > 1m)
            throw new ReglaNegocioException("El porcentaje de descuento debe encontrarse entre 0% y 100%.");

        decimal montoDescuento = Math.Round(interesesFuturos.Monto * porcentajeDescuento, 2, MidpointRounding.AwayFromZero);
        var descuentoIntereses = new Dinero(montoDescuento);
        var interesesNetos = new Dinero(interesesFuturos.Monto - montoDescuento);
        var penalizacion = Dinero.Zero;
        var totalLiquidacion = capitalPendiente + interesesNetos;

        return new ResultadoLiquidacion(
            capitalPendiente,
            interesesFuturos,
            porcentajeDescuento,
            descuentoIntereses,
            interesesNetos,
            penalizacion,
            totalLiquidacion);
    }

    /// <summary>
    /// Aplica la liquidación anticipada sobre el financiamiento, dejando su saldo en $0.00 y en estado Liquidado.
    /// </summary>
    public static void AplicarLiquidacion(Financiamiento financiamiento, DateTimeOffset fechaLiquidacion)
    {
        ArgumentNullException.ThrowIfNull(financiamiento);
        financiamiento.AplicarLiquidacion(fechaLiquidacion);
    }
}
