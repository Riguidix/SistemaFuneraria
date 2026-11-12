using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFuneraria.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    curp = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    correo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    direccion = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id);
                    table.CheckConstraint("clientes_telefono_check", "telefono ~ '^[0-9]{10}$'");
                });

            migrationBuilder.CreateIndex(
                name: "ix_clientes_apellidos_nombre",
                table: "clientes",
                columns: new[] { "apellidos", "nombre" })
                .Annotation("Npgsql:IndexOperators", new[] { "text_pattern_ops", "text_pattern_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_clientes_correo",
                table: "clientes",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clientes_curp",
                table: "clientes",
                column: "curp",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clientes");
        }
    }
}
