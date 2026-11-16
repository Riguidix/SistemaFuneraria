using FluentAssertions;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Pruebas unitarias para la entidad de dominio Lote y sus transiciones de estado (SF-039).
/// </summary>
public class LoteTests
{
    private const string ClaveValida = "JN-A-042";
    private const string SeccionValida = "Jardín Norte";
    private const string AndadorFilaValido = "Andador A · Fila 4";
    private const int GavetasValidas = 2;
    private static readonly Dinero PrecioValido = new(120000m);

    private static Lote CrearLoteValido(
        string clave = ClaveValida,
        string seccion = SeccionValida,
        string andadorFila = AndadorFilaValido,
        int gavetas = GavetasValidas,
        Dinero? precioContado = null) =>
        Lote.Crear(clave, seccion, andadorFila, gavetas, precioContado ?? PrecioValido);

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void Crear_ConDatosValidos_DebeInstanciarLoteDisponible()
    {
        var lote = CrearLoteValido();

        lote.Should().NotBeNull();
        lote.Id.Should().NotBeEmpty();
        lote.Clave.Should().Be(ClaveValida);
        lote.Seccion.Should().Be(SeccionValida);
        lote.AndadorFila.Should().Be(AndadorFilaValido);
        lote.Gavetas.Should().Be(GavetasValidas);
        lote.PrecioContado.Should().Be(PrecioValido);
        lote.Estado.Should().Be(EstadoLote.Disponible);
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void Crear_ConClaveConEspaciosYMinusculas_DebeNormalizarClaveYTextos()
    {
        var lote = Lote.Crear("  jn-a-042  ", "  Jardín Norte  ", "  Andador A · Fila 4  ", 3, new Dinero(150000m));

        lote.Clave.Should().Be("JN-A-042");
        lote.Seccion.Should().Be("Jardín Norte");
        lote.AndadorFila.Should().Be("Andador A · Fila 4");
    }

    [Theory]
    [Trait("Ticket", "SF-039")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("J-A-042")]
    [InlineData("JARD-A-042")]
    [InlineData("JN-AA-042")]
    [InlineData("JN-A-42")]
    [InlineData("JN-A-0421")]
    [InlineData("JN_A_042")]
    [InlineData("12-A-042")]
    public void Crear_ConClaveInvalida_DebeLanzarReglaNegocioException(string? claveInvalida)
    {
        Action accion = () => Lote.Crear(claveInvalida!, SeccionValida, AndadorFilaValido, GavetasValidas, PrecioValido);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Theory]
    [Trait("Ticket", "SF-039")]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Crear_ConGavetasPermitidas_DebeCrearLote(int gavetas)
    {
        var lote = CrearLoteValido(gavetas: gavetas);

        lote.Gavetas.Should().Be(gavetas);
    }

    [Theory]
    [Trait("Ticket", "SF-039")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(-2)]
    public void Crear_ConGavetasFueraDeRango_DebeLanzarReglaNegocioException(int gavetasInvalidas)
    {
        Action accion = () => CrearLoteValido(gavetas: gavetasInvalidas);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void Crear_ConPrecioContadoCero_DebeLanzarReglaNegocioException()
    {
        Action accion = () => CrearLoteValido(precioContado: Dinero.Zero);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Theory]
    [Trait("Ticket", "SF-039")]
    [InlineData(null, AndadorFilaValido)]
    [InlineData("", AndadorFilaValido)]
    [InlineData("   ", AndadorFilaValido)]
    [InlineData(SeccionValida, null)]
    [InlineData(SeccionValida, "")]
    [InlineData(SeccionValida, "   ")]
    public void Crear_ConSeccionOAndadorFilaVacios_DebeLanzarReglaNegocioException(string? seccion, string? andadorFila)
    {
        Action accion = () => Lote.Crear(ClaveValida, seccion!, andadorFila!, GavetasValidas, PrecioValido);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void Reservar_DesdeDisponible_DebeCambiarEstadoAReservado()
    {
        var lote = CrearLoteValido();

        lote.Reservar();

        lote.Estado.Should().Be(EstadoLote.Reservado);
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void Reservar_CuandoYaEstaReservado_DebeLanzarReglaNegocioException()
    {
        var lote = CrearLoteValido();
        lote.Reservar();

        Action accion = () => lote.Reservar();

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void Reservar_CuandoEstaVendido_DebeLanzarReglaNegocioException()
    {
        var lote = CrearLoteValido();
        lote.MarcarVendido();

        Action accion = () => lote.Reservar();

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void Liberar_DesdeReservado_DebeCambiarEstadoADisponible()
    {
        var lote = CrearLoteValido();
        lote.Reservar();

        lote.Liberar();

        lote.Estado.Should().Be(EstadoLote.Disponible);
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void Liberar_DesdeDisponibleOVendido_DebeLanzarReglaNegocioException()
    {
        var loteDisponible = CrearLoteValido();
        Action liberarDisponible = () => loteDisponible.Liberar();
        liberarDisponible.Should().Throw<ReglaNegocioException>();

        var loteVendido = CrearLoteValido(clave: "LM-A-014");
        loteVendido.MarcarVendido();
        Action liberarVendido = () => loteVendido.Liberar();
        liberarVendido.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void MarcarVendido_DesdeDisponible_DebeCambiarEstadoAVendido()
    {
        var lote = CrearLoteValido();

        lote.MarcarVendido();

        lote.Estado.Should().Be(EstadoLote.Vendido);
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void MarcarVendido_DesdeReservado_DebeCambiarEstadoAVendido()
    {
        var lote = CrearLoteValido();
        lote.Reservar();

        lote.MarcarVendido();

        lote.Estado.Should().Be(EstadoLote.Vendido);
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void MarcarVendido_CuandoYaEstaVendido_DebeLanzarReglaNegocioException()
    {
        var lote = CrearLoteValido(clave: "LM-A-014");
        lote.MarcarVendido();

        Action accion = () => lote.MarcarVendido();

        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("El lote ya se encuentra vendido");
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void MarcarVendido_DesdeMantenimiento_DebeLanzarReglaNegocioException()
    {
        var lote = CrearLoteValido();
        lote.ActualizarDatosAdmin(SeccionValida, AndadorFilaValido, GavetasValidas, PrecioValido, EstadoLote.Mantenimiento);

        Action accion = () => lote.MarcarVendido();

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void ActualizarDatosAdmin_ConDatosValidos_DebeActualizarPropiedades()
    {
        var lote = CrearLoteValido();
        var nuevoPrecio = new Dinero(180000m);

        lote.ActualizarDatosAdmin("  Robles  ", "  Andador B · Fila 2  ", 4, nuevoPrecio, EstadoLote.Mantenimiento);

        lote.Seccion.Should().Be("Robles");
        lote.AndadorFila.Should().Be("Andador B · Fila 2");
        lote.Gavetas.Should().Be(4);
        lote.PrecioContado.Should().Be(nuevoPrecio);
        lote.Estado.Should().Be(EstadoLote.Mantenimiento);
    }

    [Fact]
    [Trait("Ticket", "SF-039")]
    public void ActualizarDatosAdmin_CuandoLoteEstaVendido_DebeLanzarReglaNegocioException()
    {
        var lote = CrearLoteValido();
        lote.MarcarVendido();

        Action accion = () => lote.ActualizarDatosAdmin("Robles", "Andador B · Fila 2", 4, new Dinero(180000m), EstadoLote.Disponible);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Theory]
    [Trait("Ticket", "SF-039")]
    [InlineData("", "Andador B", 2, 120000)]
    [InlineData("Robles", "", 2, 120000)]
    [InlineData("Robles", "Andador B", 5, 120000)]
    [InlineData("Robles", "Andador B", 2, 0)]
    public void ActualizarDatosAdmin_ConInvariantesInvalidos_DebeLanzarReglaNegocioException(
        string seccion,
        string andadorFila,
        int gavetas,
        decimal precio)
    {
        var lote = CrearLoteValido();

        Action accion = () => lote.ActualizarDatosAdmin(seccion, andadorFila, gavetas, new Dinero(precio), EstadoLote.Disponible);

        accion.Should().Throw<ReglaNegocioException>();
    }
}
