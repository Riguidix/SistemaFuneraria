using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFuneraria.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddHardeningIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_ventas_cliente_fecha",
                table: "ventas",
                columns: new[] { "cliente_id", "fecha_venta" });

            migrationBuilder.CreateIndex(
                name: "ix_mensualidades_estado_vencimiento",
                table: "mensualidades",
                columns: new[] { "estado", "fecha_vencimiento" });

            migrationBuilder.CreateIndex(
                name: "ix_lotes_seccion_estado",
                table: "lotes",
                columns: new[] { "seccion", "estado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ventas_cliente_fecha",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "ix_mensualidades_estado_vencimiento",
                table: "mensualidades");

            migrationBuilder.DropIndex(
                name: "ix_lotes_seccion_estado",
                table: "lotes");
        }
    }
}
