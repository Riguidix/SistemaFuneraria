using FluentAssertions;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Suite de pruebas unitarias para la entidad de dominio Cliente (SF-015).
/// Valida el encapsulamiento DDD, constructor protegido para EF Core e invariantes del ERD v1.1.
/// </summary>
public class ClienteEntidadTests
{
    private const string CurpValido = "LURG850101HDFRRN09";
    private const string TelefonoValido = "5512345678";
    private const string CorreoValido = "gustavo.lucio@example.com";
    private const string NombreValido = "Gustavo";
    private const string ApellidosValido = "Lucio Rodriguez";
    private const string DireccionValida = "Av. Insurgentes Sur 1234, CDMX";

    private static Action InvocarCrear(
        string? nombre = NombreValido,
        string? apellidos = ApellidosValido,
        string? curp = CurpValido,
        string? telefono = TelefonoValido,
        string? correo = CorreoValido,
        string? direccion = DireccionValida) =>
        () => Cliente.Crear(nombre!, apellidos!, curp!, telefono!, correo!, direccion!);

    #region Creación Exitosa (Happy Path)

    [Fact]
    [Trait("Ticket", "SF-015")]
    [Trait("CP", "CP-02")]
    public void Crear_ConDatosValidos_DebeInstanciarClienteConInvariantesSatisfechos()
    {
        var fechaAntes = DateTimeOffset.UtcNow.AddSeconds(-1);
        var cliente = Cliente.Crear(NombreValido, ApellidosValido, CurpValido, TelefonoValido, CorreoValido, DireccionValida);
        var fechaDespues = DateTimeOffset.UtcNow.AddSeconds(1);

        cliente.Should().NotBeNull();
        cliente.Id.Should().NotBeEmpty("el Id debe autogenerarse con un Guid no vacío");
        cliente.Nombre.Should().Be(NombreValido);
        cliente.Apellidos.Should().Be(ApellidosValido);
        cliente.Curp.Should().Be(CurpValido);
        cliente.Telefono.Should().Be(TelefonoValido);
        cliente.Correo.Should().Be(CorreoValido.ToLowerInvariant());
        cliente.Direccion.Should().Be(DireccionValida);
        cliente.CreatedAt.Should().BeOnOrAfter(fechaAntes).And.BeOnOrBefore(fechaDespues);
    }

    [Fact]
    [Trait("Ticket", "SF-015")]
    public void Crear_ConDatosConEspaciosYMinúsculas_DebeNormalizarCurpYCorreo()
    {
        var cliente = Cliente.Crear("  Gustavo  ", "  Lucio Rodriguez  ", "  lurg850101hdfrrn09  ", TelefonoValido, "  Gustavo.Lucio@EXAMPLE.COM  ", "  Av. Insurgentes Sur 1234  ");

        cliente.Curp.Should().Be("LURG850101HDFRRN09", "el CURP debe normalizarse en mayúsculas y sin espacios");
        cliente.Correo.Should().Be("gustavo.lucio@example.com", "el correo debe normalizarse en minúsculas y sin espacios");
        cliente.Nombre.Should().Be("Gustavo");
        cliente.Apellidos.Should().Be("Lucio Rodriguez");
        cliente.Direccion.Should().Be("Av. Insurgentes Sur 1234");
    }

    #endregion

    #region Invariantes de CURP y Teléfono

    [Theory]
    [Trait("Ticket", "SF-015")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("LURG850101HDFRRN")]      // 16 caracteres
    [InlineData("LURG850101HDFRRN0")]     // 17 caracteres
    [InlineData("LURG850101HDFRRN099")]   // 19 caracteres
    [InlineData("LURG850101HDFRR-09")]    // Carácter no alfanumérico
    [InlineData("LURG850101HDFRRN0!")]    // Símbolo especial
    public void Crear_ConCurpInvalido_DebeLanzarReglaNegocioException(string? curpInvalido) =>
        InvocarCrear(curp: curpInvalido).Should().Throw<ReglaNegocioException>();

    [Theory]
    [Trait("Ticket", "SF-015")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789")]       // 9 dígitos
    [InlineData("12345678901")]     // 11 dígitos
    [InlineData("551234567a")]      // Con letra
    [InlineData("55-1234-56")]      // Con guiones
    [InlineData("55 1234567")]      // Con espacios
    public void Crear_ConTelefonoInvalido_DebeLanzarReglaNegocioException(string? telefonoInvalido) =>
        InvocarCrear(telefono: telefonoInvalido).Should().Throw<ReglaNegocioException>();

    #endregion

    #region Invariantes de Correo Electrónico

    [Theory]
    [Trait("Ticket", "SF-015")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("correo-sin-arroba")]
    [InlineData("@sin-usuario.com")]
    [InlineData("usuario@")]
    public void Crear_ConCorreoInvalido_DebeLanzarReglaNegocioException(string? correoInvalido) =>
        InvocarCrear(correo: correoInvalido).Should().Throw<ReglaNegocioException>();

    [Fact]
    [Trait("Ticket", "SF-015")]
    public void Crear_ConCorreoExcediendo255Caracteres_DebeLanzarReglaNegocioException() =>
        InvocarCrear(correo: new string('a', 244) + "@example.com").Should().Throw<ReglaNegocioException>();

    #endregion

    #region Invariantes de Nombre, Apellidos y Dirección

    [Theory]
    [Trait("Ticket", "SF-015")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConNombreInvalido_DebeLanzarReglaNegocioException(string? nombreInvalido) =>
        InvocarCrear(nombre: nombreInvalido).Should().Throw<ReglaNegocioException>();

    [Fact]
    [Trait("Ticket", "SF-015")]
    public void Crear_ConNombreExcediendo100Caracteres_DebeLanzarReglaNegocioException() =>
        InvocarCrear(nombre: new string('a', 101)).Should().Throw<ReglaNegocioException>();

    [Theory]
    [Trait("Ticket", "SF-015")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConApellidosInvalido_DebeLanzarReglaNegocioException(string? apellidosInvalido) =>
        InvocarCrear(apellidos: apellidosInvalido).Should().Throw<ReglaNegocioException>();

    [Fact]
    [Trait("Ticket", "SF-015")]
    public void Crear_ConApellidosExcediendo100Caracteres_DebeLanzarReglaNegocioException() =>
        InvocarCrear(apellidos: new string('a', 101)).Should().Throw<ReglaNegocioException>();

    [Theory]
    [Trait("Ticket", "SF-015")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConDireccionInvalida_DebeLanzarReglaNegocioException(string? direccionInvalida) =>
        InvocarCrear(direccion: direccionInvalida).Should().Throw<ReglaNegocioException>();

    #endregion
}
