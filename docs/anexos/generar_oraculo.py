"""Oráculo de referencia independiente (Python Decimal) para validar el motor financiero en C#.

Convenciones (ver ADR-001 en FUN-007):
- Mensualidad calculada con precisión completa y redondeada UNA vez a 2 decimales (HALF_UP = MidpointRounding.AwayFromZero).
- Interés del periodo = round2(saldo_inicial * tasa_mensual).
- Capital del periodo = mensualidad - interés; la última mensualidad paga el saldo exacto restante (+ su interés).

Uso: python3 generar_oraculo.py  -> regenera los CSV de esta carpeta.
"""
from decimal import Decimal as D, getcontext, ROUND_HALF_UP

getcontext().prec = 40
TASAS = {12: 16, 18: 18, 24: 20, 48: 24}
LOTES = [("Lote basico", 2, 120000), ("Lote familiar", 3, 150000), ("Lote ampliado", 4, 180000)]


def r(x):
    return x.quantize(D("0.01"), rounding=ROUND_HALF_UP)


def tabla(capital, tasa_anual, n):
    i = D(tasa_anual) / D(100) / D(12)
    f = (1 + i) ** n
    cuota = r(capital * (i * f) / (f - 1))
    saldo, filas = capital, []
    for k in range(1, n + 1):
        interes = r(saldo * i)
        cap, pago = (saldo, saldo + interes) if k == n else (cuota - interes, cuota)
        filas.append((k, saldo, interes, cap, pago, saldo - cap))
        saldo -= cap
    return cuota, filas


def escribir_tabla(nombre, filas, desfase=0):
    with open(nombre, "w") as f:
        f.write("periodo,saldo_inicial,interes,capital,pago,saldo_final\n")
        for k, s, i, cap, p, sf in filas:
            f.write(f"{k + desfase},{s:.2f},{i:.2f},{cap:.2f},{p:.2f},{sf:.2f}\n")


if __name__ == "__main__":
    with open("oraculo-36-combinaciones.csv", "w") as f:
        f.write("lote,gavetas,precio,enganche_pct,enganche,capital_financiado,plazo_meses,tasa_anual_pct,"
                "mensualidad,ultima_mensualidad,intereses_totales,total_pagado\n")
        for nombre, gavetas, precio in LOTES:
            for pct in (10, 20, 30):
                for n, t in TASAS.items():
                    p = D(precio)
                    eng = r(p * D(pct) / 100)
                    cap = p - eng
                    cuota, filas = tabla(cap, t, n)
                    intereses = sum(x[2] for x in filas)
                    assert filas[-1][5] == 0
                    f.write(f"{nombre},{gavetas},{p:.2f},{pct},{eng:.2f},{cap:.2f},{n},{t},{cuota:.2f},"
                            f"{filas[-1][4]:.2f},{intereses:.2f},{eng + cap + intereses:.2f}\n")

    _, base = tabla(D(96000), 16, 12)
    escribir_tabla("tabla-cp06-120k-20pct-12m.csv", base)

    # CP-07: abono de $10,000 a capital después de pagar el periodo 3; se recalcula sobre 9 periodos restantes.
    _, recalculada = tabla(base[2][5] - D(10000), 16, 9)
    escribir_tabla("tabla-cp07-abono-10000-tras-periodo-3.csv", recalculada, desfase=3)
