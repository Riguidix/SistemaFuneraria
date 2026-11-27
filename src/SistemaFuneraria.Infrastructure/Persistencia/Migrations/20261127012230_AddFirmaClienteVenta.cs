using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFuneraria.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmaClienteVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "fecha_firma",
                table: "ventas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "firma_cliente_base64",
                table: "ventas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vendedor_nombre",
                table: "ventas",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_firma",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "firma_cliente_base64",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "vendedor_nombre",
                table: "ventas");
        }
    }
}
