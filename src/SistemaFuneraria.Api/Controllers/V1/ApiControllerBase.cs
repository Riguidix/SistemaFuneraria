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
        var (statusCode, title) = resultado.TipoError switch
        {
            TipoError.Conflicto => (StatusCodes.Status409Conflict, "Conflicto de negocio"),
            TipoError.NoEncontrado => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            TipoError.NoAutorizado => (StatusCodes.Status401Unauthorized, "No autorizado"),
            TipoError.Prohibido => (StatusCodes.Status403Forbidden, "Acceso denegado"),
            _ => (StatusCodes.Status400BadRequest, "Error de validación o regla de negocio")
        };

        return Problem(
            detail: resultado.Error,
            statusCode: statusCode,
            title: title);
    }
}
