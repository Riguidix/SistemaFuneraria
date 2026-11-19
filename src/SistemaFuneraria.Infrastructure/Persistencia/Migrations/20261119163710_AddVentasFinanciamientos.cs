using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFuneraria.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddVentasFinanciamientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "seq_contratos_folio",
                startValue: 190L);

            migrationBuilder.CreateTable(
                name: "ventas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    folio_contrato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendedor_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    modalidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    precio_pactado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_venta = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    motivo_cancelacion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventas", x => x.id);
                    table.CheckConstraint("ventas_estado_check", "estado IN ('Activo', 'Liquidado', 'EnRevision', 'Cancelado')");
                    table.CheckConstraint("ventas_modalidad_check", "modalidad IN ('Contado', 'Financiado')");
                    table.CheckConstraint("ventas_precio_pactado_check", "precio_pactado > 0");
                    table.ForeignKey(
                        name: "FK_ventas_AspNetUsers_vendedor_id",
                        column: x => x.vendedor_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ventas_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ventas_lotes_lote_id",
                        column: x => x.lote_id,
                        principalTable: "lotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "financiamientos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    venta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    porcentaje_enganche = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    monto_enganche = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    capital_financiado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    plazo_meses = table.Column<int>(type: "integer", nullable: false),
                    tasa_interes_anual = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    cuota_mensual_actual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_insoluto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financiamientos", x => x.id);
                    table.CheckConstraint("financiamientos_capital_check", "capital_financiado > 0");
                    table.CheckConstraint("financiamientos_cuota_check", "cuota_mensual_actual > 0");
                    table.CheckConstraint("financiamientos_enganche_pct_check", "porcentaje_enganche IN (10, 20, 30)");
                    table.CheckConstraint("financiamientos_estado_check", "estado IN ('Activo', 'EnMora', 'Liquidado', 'Cancelado')");
                    table.CheckConstraint("financiamientos_monto_enganche_check", "monto_enganche > 0");
                    table.CheckConstraint("financiamientos_plazo_check", "plazo_meses IN (12, 18, 24, 48)");
                    table.CheckConstraint("financiamientos_saldo_check", "saldo_insoluto >= 0");
                    table.CheckConstraint("financiamientos_tasa_check", "tasa_interes_anual IN (0.1600, 0.1800, 0.2000, 0.2400)");
                    table.ForeignKey(
                        name: "FK_financiamientos_ventas_venta_id",
                        column: x => x.venta_id,
                        principalTable: "ventas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mensualidades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    financiamiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_cuota = table.Column<int>(type: "integer", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    saldo_inicial_periodo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    capital_programado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interes_programado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    monto_total_cuota = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_final_periodo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_pago = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensualidades", x => x.id);
                    table.CheckConstraint("mensualidades_capital_check", "capital_programado >= 0");
                    table.CheckConstraint("mensualidades_cuota_check", "monto_total_cuota > 0");
                    table.CheckConstraint("mensualidades_estado_check", "estado IN ('Pendiente', 'Pagada', 'Vencida')");
                    table.CheckConstraint("mensualidades_interes_check", "interes_programado >= 0");
                    table.CheckConstraint("mensualidades_numero_check", "numero_cuota >= 1");
                    table.CheckConstraint("mensualidades_saldo_fin_check", "saldo_final_periodo >= 0");
                    table.CheckConstraint("mensualidades_saldo_ini_check", "saldo_inicial_periodo >= 0");
                    table.ForeignKey(
                        name: "FK_mensualidades_financiamientos_financiamiento_id",
                        column: x => x.financiamiento_id,
                        principalTable: "financiamientos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "uq_financiamientos_venta_id",
                table: "financiamientos",
                column: "venta_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mensualidades_vencimiento",
                table: "mensualidades",
                column: "fecha_vencimiento");

            migrationBuilder.CreateIndex(
                name: "uq_mensualidades_cuota",
                table: "mensualidades",
                columns: new[] { "financiamiento_id", "numero_cuota" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ventas_cliente_id",
                table: "ventas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_ventas_fecha",
                table: "ventas",
                column: "fecha_venta",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_ventas_folio",
                table: "ventas",
                column: "folio_contrato",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ventas_vendedor_id",
                table: "ventas",
                column: "vendedor_id");

            migrationBuilder.CreateIndex(
                name: "uq_ventas_lote_activo",
                table: "ventas",
                column: "lote_id",
                unique: true,
                filter: "estado <> 'Cancelado'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mensualidades");

            migrationBuilder.DropTable(
                name: "financiamientos");

            migrationBuilder.DropTable(
                name: "ventas");

            migrationBuilder.DropSequence(
                name: "seq_contratos_folio");
        }
    }
}
