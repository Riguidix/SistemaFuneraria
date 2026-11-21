using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFuneraria.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddPagosCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "seq_pagos_folio");

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    folio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    venta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    financiamiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mensualidad_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cajero_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    tipo_pago = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    metodo_pago = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    referencia = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    monto_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    monto_capital = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    monto_interes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_posterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    clave_idempotencia = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    fecha_pago = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.id);
                    table.CheckConstraint("pagos_metodo_pago_check", "metodo_pago IN ('Efectivo', 'TransferenciaBancaria', 'Tarjeta')");
                    table.CheckConstraint("pagos_monto_capital_check", "monto_capital >= 0");
                    table.CheckConstraint("pagos_monto_interes_check", "monto_interes >= 0");
                    table.CheckConstraint("pagos_monto_total_check", "monto_total > 0");
                    table.CheckConstraint("pagos_saldo_anterior_check", "saldo_anterior >= 0");
                    table.CheckConstraint("pagos_saldo_posterior_check", "saldo_posterior >= 0");
                    table.CheckConstraint("pagos_tipo_pago_check", "tipo_pago IN ('Enganche', 'Contado', 'Mensualidad', 'AbonoCapital', 'Liquidacion')");
                    table.ForeignKey(
                        name: "FK_pagos_AspNetUsers_cajero_id",
                        column: x => x.cajero_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_financiamientos_financiamiento_id",
                        column: x => x.financiamiento_id,
                        principalTable: "financiamientos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_mensualidades_mensualidad_id",
                        column: x => x.mensualidad_id,
                        principalTable: "mensualidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_ventas_venta_id",
                        column: x => x.venta_id,
                        principalTable: "ventas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pagos_cajero_id",
                table: "pagos",
                column: "cajero_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagos_clave_idempotencia",
                table: "pagos",
                column: "clave_idempotencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pagos_fecha",
                table: "pagos",
                column: "fecha_pago",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_pagos_financiamiento_id",
                table: "pagos",
                column: "financiamiento_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagos_folio",
                table: "pagos",
                column: "folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pagos_mensualidad_id",
                table: "pagos",
                column: "mensualidad_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagos_venta_id",
                table: "pagos",
                column: "venta_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropSequence(
                name: "seq_pagos_folio");
        }
    }
}
