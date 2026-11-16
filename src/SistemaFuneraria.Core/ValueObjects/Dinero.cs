using System.Globalization;
using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Core.ValueObjects;

/// <summary>
/// Value Object inmutable que representa una cantidad monetaria no negativa con precisión a 2 decimales (RNF-02).
/// </summary>
public readonly record struct Dinero : IComparable<Dinero>
{
    public decimal Monto { get; }

    public Dinero(decimal monto)
    {
        if (monto < 0) throw new ReglaNegocioException("El monto no puede ser negativo.");

        Monto = RedondeoBancario.Redondear(monto);
    }

    public static Dinero Zero => new(0m);

    public static Dinero operator +(Dinero a, Dinero b) => new (a.Monto + b.Monto);

    public static Dinero operator -(Dinero a, Dinero b) => new  (a.Monto - b.Monto);

    public static Dinero operator *(Dinero a, decimal factor) => new (a.Monto * factor);

    public static Dinero operator *(decimal factor, Dinero a) => new (a.Monto * factor);

    public static Dinero operator /(Dinero a, decimal divisor) => new (a.Monto / divisor);

    public static decimal operator /(Dinero a, Dinero b) => a.Monto / b.Monto;

    public static bool operator <(Dinero a, Dinero b) => a.Monto < b.Monto;

    public static bool operator <=(Dinero a, Dinero b) => a.Monto <= b.Monto;

    public static bool operator >(Dinero a, Dinero b) => a.Monto > b.Monto;

    public static bool operator >=(Dinero a, Dinero b) => a.Monto >= b.Monto;

    public int CompareTo(Dinero other) => Monto.CompareTo(other.Monto);

    public static implicit operator decimal(Dinero dinero) => dinero.Monto;

    public static explicit operator Dinero(decimal monto) => new(monto);

    public override string ToString() => Monto.ToString(CultureInfo.CurrentCulture);
}
