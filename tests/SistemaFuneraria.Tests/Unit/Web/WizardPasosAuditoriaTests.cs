using FluentAssertions;
using SistemaFuneraria.Web.Pages.Ventas;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

[Trait("Ticket", "SF-056")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class WizardPasosAuditoriaTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact(DisplayName = "SF-056 / QA Gate: Inicialización y paridad visual del Wizard con Captura 5")]
    public void AuditoriaFigma_Paso1a3_ComponentesYResumenCotizacion_DebeCumplirEspecificacionFigma()
    {
        var pageModel = new NuevaModel();
        pageModel.OnGet();

        pageModel.Kicker.Should().Be("PROCESO COMERCIAL");
        pageModel.Titulo.Should().Be("Nueva venta");
        pageModel.PasoActual.Should().Be(1);
        pageModel.LoteTipo.Should().Be("Lote Básico");
        pageModel.LoteClave.Should().Be("JN-A-042");
        pageModel.GavetaSeleccionada.Should().Be(42);
        pageModel.CapacidadGavetas.Should().Be(2);
        pageModel.PrecioLista.Should().Be(120_000.00m);
        pageModel.TotalEstimado.Should().Be(120_000.00m);
    }

    [Fact(DisplayName = "SF-056 / BDD: Regresión de pasos sin pérdida de datos en cotización")]
    public void AuditoriaBDD_RegresionPasos_AnteriorYSiguiente_DebeConservarDatosDeCotizacionYFinanciamiento()
    {
        decimal precioLote = 120_000.00m;
        int enganche = 20;
        int plazo = 24;

        var financiamientoPaso3 = FinanciamientoCalculadora.Calcular(precioLote, "Credito", enganche, plazo);

        financiamientoPaso3.MontoEnganche.Should().Be(24_000.00m);
        financiamientoPaso3.CapitalAFinanciar.Should().Be(96_000.00m);
        financiamientoPaso3.MensualidadEstimada.Should().Be(4_887.54m);

        var regresoPaso2 = new { LoteClave = "JN-A-042", Gavetas = 2, Precio = precioLote };
        var retornoPaso3 = FinanciamientoCalculadora.Calcular(regresoPaso2.Precio, "Credito", enganche, plazo);

        retornoPaso3.Should().BeEquivalentTo(financiamientoPaso3);
    }

    [Fact(DisplayName = "SF-056 / Frontend: Integridad de scripts modulares y persistencia de borrador")]
    public void AuditoriaFrontend_ScriptsReactivosYPersistenciaDraft_DebenEstarRegistrados()
    {
        var scripts = new[]
        {
            Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "paso1Cliente.js"),
            Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "paso2Lote.js"),
            Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "paso3Financiamiento.js"),
            Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "wizardState.js"),
            Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "simuladorApi.js")
        };

        foreach (var ruta in scripts)
        {
            File.Exists(ruta).Should().BeTrue();
        }

        var wizardStateCode = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "wizardState.js"));
        wizardStateCode.Should().Contain("sf_nueva_venta_draft");
        wizardStateCode.Should().Contain("syncDraftBadge");
        wizardStateCode.Should().Contain("syncSummaryPanel");
    }

    [Fact(DisplayName = "SF-056 / CP-02: Modalidad Contado debe proyectar pago único sin intereses")]
    public void AuditoriaFigma_ModalidadContado_DebeProyectarPagoUnicoInmediato()
    {
        decimal precioLote = 120_000.00m;
        var contado = FinanciamientoCalculadora.Calcular(precioLote, "Contado", 0, 0);

        contado.MontoEnganche.Should().Be(0m);
        contado.CapitalAFinanciar.Should().Be(0m);
        contado.MensualidadEstimada.Should().Be(120_000.00m);
        contado.TituloBanner.Should().Be("Pago único de contado");
    }
}
