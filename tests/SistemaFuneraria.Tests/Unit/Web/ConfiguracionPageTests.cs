using FluentAssertions;
using SistemaFuneraria.Web.Pages.Configuracion;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ConfiguracionPageTests
{
    [Fact]
    public void Configuracion_OnGet_DebeCargarDatosDeSucursalYSwitchesConformeFigmaCaptura12()
    {
        // Arrange
        var pageModel = new IndexModel();

        // Act
        pageModel.OnGet();

        // Assert - Datos de la Sucursal (Captura 12 · SF-092)
        pageModel.NombreComercial.Should().Be("Jardines del Valle");
        pageModel.Telefono.Should().Be("55 3128 4400");
        pageModel.Correo.Should().Be("contacto@jardinesdelvalle.mx");
        pageModel.Domicilio.Should().Be("Av. de la Paz 1840, Col. Del Valle");
        pageModel.Ciudad.Should().Be("Ciudad de México");
        pageModel.CodigoPostal.Should().Be("03100");

        // Assert - Switches de Automatización
        pageModel.RecordatoriosPagoActivo.Should().BeTrue("Los recordatorios de pago deben estar activados por defecto");
        pageModel.ResumenDiarioCajaActivo.Should().BeTrue("El resumen diario de caja debe estar activado por defecto");
        pageModel.SeccionActiva.Should().Be("DatosSucursal");

        // Assert - Secciones del Submenú (Usuarios, Planes y Tasas)
        pageModel.Usuarios.Should().HaveCount(3);
        pageModel.Usuarios[0].Nombre.Should().Be("Andrea Morales");
        pageModel.PlanesLotes.Should().HaveCount(3);
        pageModel.PlanesLotes[0].Nombre.Should().Be("Lote Básico");
        pageModel.TasasPlazos.Should().HaveCount(4);
    }
}
