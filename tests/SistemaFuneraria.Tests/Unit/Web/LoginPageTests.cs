using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using SistemaFuneraria.Web.Pages.Cuenta;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class LoginPageTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string LeerArchivoWeb(params string[] segmentos)
    {
        var ruta = Path.Combine(new[] { RepoRoot, "src", "SistemaFuneraria.Web" }.Concat(segmentos).ToArray());
        File.Exists(ruta).Should().BeTrue($"debe existir '{ruta}'");
        return File.ReadAllText(ruta);
    }

    [Fact]
    public void Login_OnGet_DebeInicializarEstadoLimpio()
    {
        var pageModel = new LoginModel();

        pageModel.OnGet();

        pageModel.Input.Should().NotBeNull();
        pageModel.Input.Email.Should().BeEmpty();
        pageModel.Input.Password.Should().BeEmpty();
        pageModel.Input.Recordarme.Should().BeFalse();
    }

    [Fact]
    public void Login_OnPost_NuncaDebeRedirigirSinAutenticacionApi()
    {
        var pageModel = new LoginModel
        {
            Input = new LoginModel.LoginInputModel
            {
                Email = "cualquiera@serenia.mx",
                Password = "demo"
            }
        };

        var result = pageModel.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OlvideContrasena_OnPost_DebeMostrarMensajeUniformeSeguro()
    {
        var pageModel = new OlvideContrasenaModel
        {
            Email = "usuario@ejemplo.com"
        };

        var result = pageModel.OnPost();

        result.Should().BeOfType<PageResult>();
        pageModel.MensajeConfirmacion.Should().Be("Si el correo ingresado se encuentra registrado, recibirás un enlace de restablecimiento en breve");
    }

    [Fact]
    public void Index_OnGet_DebeRedirigirAResumen()
    {
        var pageModel = new SistemaFuneraria.Web.Pages.IndexModel(
            NullLogger<SistemaFuneraria.Web.Pages.IndexModel>.Instance);

        var result = pageModel.OnGet();

        var redirect = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirect.PageName.Should().Be("/Resumen/Index");
    }

    [Fact]
    public void LoginViewYJs_DebenCumplirToggleAccesibleValidacionYRedireccionPorRol()
    {
        var loginHtml = LeerArchivoWeb("Pages", "Cuenta", "Login.cshtml");
        var loginJs = LeerArchivoWeb("wwwroot", "js", "pages", "loginPage.js");
        var forgotHtml = LeerArchivoWeb("Pages", "Cuenta", "OlvideContrasena.cshtml");

        loginHtml.Should().Contain("aria-pressed=\"false\"");
        loginHtml.Should().Contain("aria-label=\"Mostrar contraseña\"");
        loginHtml.Should().Contain("fa-solid fa-eye");

        loginJs.Should().Contain("resolvePostLoginUrl");
        loginJs.Should().Contain("EMAIL_REGEX");
        loginJs.Should().Contain("aria-pressed");
        loginJs.Should().Contain("Accediendo...");
        loginJs.Should().Contain("mensajeBloqueoActual");

        forgotHtml.Should().Contain("emailRegex");
        forgotHtml.Should().Contain("Enviando...");
    }

    [Fact]
    public void LayoutSidebar_DebeTenerEnlacesActivosRbacYNavSemantica()
    {
        var layoutHtml = LeerArchivoWeb("Pages", "Shared", "_Layout.cshtml");

        layoutHtml.Should().NotContain("class=\"funeraria-nav-item inert\"");
        layoutHtml.Should().NotContain("<ul class=\"funeraria-nav-list\" role=\"navigation\"");
        layoutHtml.Should().Contain("<nav aria-label=\"Menú operativo\">");
        layoutHtml.Should().Contain("<nav aria-label=\"Menú de administración\" data-roles=\"Administrador\">");
        layoutHtml.Should().Contain("data-auth-home");
        layoutHtml.Should().Contain("href=\"/Resumen\"");
        layoutHtml.Should().Contain("href=\"/Ventas/Nueva\"");
        layoutHtml.Should().Contain("href=\"/Reportes\"");
        layoutHtml.Should().Contain("href=\"/Configuracion\"");
        layoutHtml.Should().Contain("data-roles=\"Administrador,Vendedor\"");
        layoutHtml.Should().Contain("data-roles=\"Administrador,Vendedor,Cajero\"");
        layoutHtml.Should().Contain("data-roles=\"Administrador,Cajero\"");
    }
}
