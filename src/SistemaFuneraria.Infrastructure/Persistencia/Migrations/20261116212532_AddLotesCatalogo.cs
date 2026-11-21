using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFuneraria.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddLotesCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lotes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clave = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    seccion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    andador_fila = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    gavetas = table.Column<int>(type: "integer", nullable: false),
                    precio_contado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lotes", x => x.id);
                    table.CheckConstraint("lotes_estado_check", "estado IN ('Disponible', 'Reservado', 'Vendido', 'Mantenimiento')");
                    table.CheckConstraint("lotes_gavetas_check", "gavetas IN (2, 3, 4)");
                    table.CheckConstraint("lotes_precio_contado_check", "precio_contado > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_lotes_clave",
                table: "lotes",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lotes_filtros",
                table: "lotes",
                columns: new[] { "seccion", "estado", "gavetas" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lotes");
        }
    }
}
