using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.Comun;

namespace SistemaFuneraria.Api.Controllers.V1;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult<T>(Resultado<T> resultado) => resultado.EsExitoso 
        ? Ok(resultado.Valor) 
        : MapearError(resultado);

    protected IActionResult FromResult(Resultado resultado) => resultado.EsExitoso ? Ok() : MapearError(resultado);

    private IActionResult MapearError(Resultado resultado)
    {
        var error = resultado.Error;

        if (error.StartsWith("[CONFLICTO]", StringComparison.Ordinal))
        {
            return Problem(
                detail: error,
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflicto de negocio");
        }

        if (error.StartsWith("[NO_ENCONTRADO]", StringComparison.Ordinal))
        {
            return Problem(
                detail: error,
                statusCode: StatusCodes.Status404NotFound,
                title: "Recurso no encontrado");
        }

        return Problem(
            detail: error,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Error de validación o regla de negocio");
    }
}
