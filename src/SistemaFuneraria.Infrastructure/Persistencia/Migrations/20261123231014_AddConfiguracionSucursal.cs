using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFuneraria.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracionSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuracion_sucursal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_comercial = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    telefono_atencion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    correo_operativo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    direccion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    codigo_postal = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    recordatorios_pago = table.Column<bool>(type: "boolean", nullable: false),
                    resumen_diario_caja = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_sucursal", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion_sucursal");
        }
    }
}
