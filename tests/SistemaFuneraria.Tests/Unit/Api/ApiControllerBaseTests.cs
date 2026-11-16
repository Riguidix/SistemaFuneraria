using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Api.Controllers.V1;
using SistemaFuneraria.Application.Comun;

namespace SistemaFuneraria.Tests.Unit.Api;

public class ApiControllerBaseTests
{
    private sealed class TestableApiController : ApiControllerBase
    {
        public IActionResult InvocarFromResult<T>(Resultado<T> resultado) => FromResult(resultado);
        public IActionResult InvocarFromResult(Resultado resultado) => FromResult(resultado);
    }

    private readonly TestableApiController _controller = new();

    [Fact(DisplayName = "SF-020: FromResult con Resultado<T>.Exito retorna 200 OK con el valor")]
    public void FromResult_ConExitoTipado_DebeRetornar200OkConValor()
    {
        // Arrange
        var payload = new { Id = 1, Nombre = "Plan Memorial" };
        var resultado = Resultado<object>.Exito(payload);

        // Act
        var actionResult = _controller.InvocarFromResult(resultado);

        
        // Assert
        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(payload);
    }

    [Fact(DisplayName = "SF-020: FromResult con Resultado.Exito retorna 200 OK")]
    public void FromResult_ConExitoNoTipado_DebeRetornar200Ok()
    {
        // Arrange
        var resultado = Resultado.Exito();

        // Act
        var actionResult = _controller.InvocarFromResult(resultado);

        // Assert
        var okResult = actionResult.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact(DisplayName = "SF-020 (BDD): FromResult con error de negocio retorna 400 Bad Request ProblemDetails")]
    public void FromResult_ConErrorDeNegocio_DebeRetornar400ProblemDetails()
    {
        // Arrange
        const string mensaje = "Enganche no permitido. Solo se acepta 10%, 20% o 30%";
        var resultado = Resultado<string>.Fallo(mensaje);

        // Act
        var actionResult = _controller.InvocarFromResult(resultado);

        // Assert
        var objectResult = actionResult.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Detail.Should().Be(mensaje);
    }

    [Fact(DisplayName = "SF-020: FromResult con prefijo [CONFLICTO] retorna 409 Conflict ProblemDetails")]
    public void FromResult_ConPrefijoConflicto_DebeRetornar409ProblemDetails()
    {
        // Arrange
        const string mensaje = "[CONFLICTO] Ya existe un cliente registrado con la CURP proporcionada.";
        var resultado = Resultado<string>.Fallo(mensaje);

        // Act
        var actionResult = _controller.InvocarFromResult(resultado);

        // Assert
        var objectResult = actionResult.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Status.Should().Be(StatusCodes.Status409Conflict);
        problem.Detail.Should().Contain("Ya existe un cliente registrado");
    }

    [Fact(DisplayName = "SF-020: FromResult con prefijo [NO_ENCONTRADO] retorna 404 Not Found ProblemDetails")]
    public void FromResult_ConPrefijoNoEncontrado_DebeRetornar404ProblemDetails()
    {
        // Arrange
        const string mensaje = "[NO_ENCONTRADO] El cliente solicitado no existe.";
        var resultado = Resultado.Fallo(mensaje);

        // Act
        var actionResult = _controller.InvocarFromResult(resultado);

        // Assert
        var objectResult = actionResult.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Status.Should().Be(StatusCodes.Status404NotFound);
        problem.Detail.Should().Contain("El cliente solicitado no existe");
    }
}
