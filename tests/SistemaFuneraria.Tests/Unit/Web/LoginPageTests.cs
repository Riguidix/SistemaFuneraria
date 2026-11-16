using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaFuneraria.Web.Pages.Cuenta;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class LoginPageTests
{
    [Fact]
    public void Login_OnGet_DebeInicializarEstadoLimpio()
    {
        // Arrange
        var pageModel = new LoginModel();

        // Act
        pageModel.OnGet();

        // Assert
        pageModel.IntentosFallidos.Should().Be(0);
        pageModel.EstaBloqueado.Should().BeFalse();
        pageModel.MinutosBloqueoRestantes.Should().Be(0);
        pageModel.Input.Should().NotBeNull();
    }

    [Fact]
    public void Login_OnPost_ConModeloInvalido_DebeRetornarPaginaConErrores()
    {
        // Arrange
        var pageModel = new LoginModel();
        pageModel.ModelState.AddModelError("Input.Email", "El correo es obligatorio");

        // Act
        var result = pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        pageModel.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Login_OnPost_ConCredencialesValidas_DebeRedirigirAResumen()
    {
        // Arrange
        var pageModel = new LoginModel();
        pageModel.Input.Email = "andrea@serenia.mx";
        pageModel.Input.Password = "Password123!";

        // Act
        var result = pageModel.OnPost();

        // Assert
        result.Should().BeOfType<RedirectToPageResult>();
        var redirect = (RedirectToPageResult)result;
        redirect.PageName.Should().Be("/Resumen/Index");
    }

    [Fact]
    public void OlvideContrasena_OnPost_DebeMostrarMensajeUniformeSeguro()
    {
        // Arrange
        var pageModel = new OlvideContrasenaModel
        {
            Email = "usuario@ejemplo.com"
        };

        // Act
        var result = pageModel.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
        pageModel.MensajeConfirmacion.Should().Be("Si el correo ingresado se encuentra registrado, recibirás un enlace de restablecimiento en breve");
    }
}
