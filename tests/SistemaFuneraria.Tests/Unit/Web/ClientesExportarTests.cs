using FluentAssertions;
using SistemaFuneraria.Web.Pages.Clientes;
using System.Text;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ClientesExportarTests
{
    [Fact]
    public void GenerarNombreArchivo_DebeTenerFormatoCorrecto()
    {
        // Arrange
        var fecha = new DateTime(2026, 11, 18);

        // Act
        var nombre = ClientesExportadorCsv.GenerarNombreArchivo(fecha);

        // Assert
        nombre.Should().Be("directorio_clientes_20261118.csv");
    }

    [Fact]
    public void GenerarCsvBytes_DebeIncluirBomUtf8YCabeceraEsperada()
    {
        // Arrange
        var listaVacia = Enumerable.Empty<ClienteExportDto>();

        // Act
        var bytes = ClientesExportadorCsv.GenerarCsvBytes(listaVacia);
        var contenido = Encoding.UTF8.GetString(bytes);

        // Assert
        bytes.Should().StartWith(new byte[] { 0xEF, 0xBB, 0xBF }); // UTF-8 BOM
        contenido.Should().Contain("Cliente,Teléfono,CURP,Contratos,Saldo,Último Pago,Estado");
    }

    [Fact]
    public void GenerarCsvBytes_ConClientes_DebeFormatearFilasCorrectamente()
    {
        // Arrange
        var clientes = new List<ClienteExportDto>
        {
            new("Laura Castañeda", "55 1284 9021", "CAL951118MDFRR01", "1 activo", "$127,500.00", "18 nov. 2025", "Al corriente"),
            new("Roberto Méndez", "55 4390 1178", "MER920315HDFNN02", "1 activo", "$58,743.20", "18 nov. 2025", "Al corriente")
        };

        // Act
        var bytes = ClientesExportadorCsv.GenerarCsvBytes(clientes);
        var contenido = Encoding.UTF8.GetString(bytes);

        // Assert
        contenido.Should().Contain("Laura Castañeda,55 1284 9021,CAL951118MDFRR01,1 activo,\"$127,500.00\",18 nov. 2025,Al corriente");
        contenido.Should().Contain("Roberto Méndez,55 4390 1178,MER920315HDFNN02,1 activo,\"$58,743.20\",18 nov. 2025,Al corriente");
    }

    [Fact]
    public void GenerarCsvBytes_ConComasOComillas_DebeEscaparCampos()
    {
        // Arrange
        var clientes = new List<ClienteExportDto>
        {
            new("Pérez, Juan \"El Grande\"", "55 0000 1111", "PEJ900101MDF001", "2 activos", "$10,000.00", "01 nov. 2025", "En revisión")
        };

        // Act
        var bytes = ClientesExportadorCsv.GenerarCsvBytes(clientes);
        var contenido = Encoding.UTF8.GetString(bytes);

        // Assert
        contenido.Should().Contain("\"Pérez, Juan \"\"El Grande\"\"\",55 0000 1111,PEJ900101MDF001,2 activos,\"$10,000.00\",01 nov. 2025,En revisión");
    }
}
