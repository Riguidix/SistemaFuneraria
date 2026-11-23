using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.DTOs.Configuracion;
using SistemaFuneraria.Application.Servicios.Configuracion;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador administrativo para configuración de sucursal y gestión de usuarios/roles (SF-091).
/// </summary>
[ApiController]
[Route("api/v1/configuracion")]
[Authorize(Roles = "Administrador")]
public class ConfiguracionController(
    IConfiguracionSucursalService sucursalService,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) : ApiControllerBase
{
    private static readonly string[] RolesPermitidos =
    [
        ParametrosNegocio.RolAdministrador,
        ParametrosNegocio.RolVendedor,
        ParametrosNegocio.RolCajero
    ];

    private readonly IConfiguracionSucursalService _sucursalService = sucursalService;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    [HttpGet("sucursal")]
    [ProducesResponseType(typeof(ConfiguracionSucursalDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerSucursal(CancellationToken ct = default)
    {
        var resultado = await _sucursalService.ObtenerSucursalAsync(ct);
        return FromResult(resultado);
    }

    [HttpPut("sucursal")]
    [ProducesResponseType(typeof(ConfiguracionSucursalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActualizarSucursal(
        [FromBody] ActualizarConfiguracionSucursalRequest request,
        CancellationToken ct = default)
    {
        var resultado = await _sucursalService.ActualizarSucursalAsync(request, ct);
        return FromResult(resultado);
    }

    [HttpGet("usuarios")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioSistemaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarUsuarios(CancellationToken ct = default)
    {
        var usuarios = await _userManager.Users.AsNoTracking().ToListAsync(ct);
        var lista = new List<UsuarioSistemaDto>(usuarios.Count);

        foreach (var u in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var claims = await _userManager.GetClaimsAsync(u);
            var nombre = claims.FirstOrDefault(c => c.Type == "nombre_completo")?.Value ?? u.Email ?? u.UserName ?? "-";
            bool bloqueado = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow;
            bool activo = !(u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(10));

            lista.Add(new UsuarioSistemaDto(
                Id: u.Id,
                Nombre: nombre,
                Correo: u.Email ?? u.UserName ?? "-",
                Rol: roles.FirstOrDefault() ?? "Sin rol",
                Activo: activo,
                Bloqueado: bloqueado));
        }

        return Ok(lista);
    }

    [HttpPost("usuarios")]
    [ProducesResponseType(typeof(UsuarioSistemaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioConfiguracionRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Password) || !RolesPermitidos.Contains(request.Rol))
        {
            return Problem(
                detail: "Correo, contraseña y un rol válido (Administrador, Vendedor o Cajero) son obligatorios.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Datos inválidos");
        }

        var correoNorm = request.Correo.Trim().ToLowerInvariant();
        if (await _userManager.FindByEmailAsync(correoNorm) is not null)
        {
            return Problem(
                detail: $"Ya existe una cuenta registrada con el correo '{correoNorm}'.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Usuario duplicado");
        }

        if (!await _roleManager.RoleExistsAsync(request.Rol))
        {
            await _roleManager.CreateAsync(new IdentityRole(request.Rol));
        }

        var user = new IdentityUser
        {
            UserName = correoNorm,
            Email = correoNorm,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return Problem(
                detail: string.Join("; ", createResult.Errors.Select(e => e.Description)),
                statusCode: StatusCodes.Status400BadRequest,
                title: "Error al crear usuario");
        }

        await _userManager.AddToRoleAsync(user, request.Rol);
        if (!string.IsNullOrWhiteSpace(request.Nombre))
        {
            await _userManager.AddClaimAsync(user, new Claim("nombre_completo", request.Nombre.Trim()));
        }

        var dto = new UsuarioSistemaDto(user.Id, request.Nombre?.Trim() ?? correoNorm, correoNorm, request.Rol, true, false);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [HttpPut("usuarios/{id}")]
    [ProducesResponseType(typeof(UsuarioSistemaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarUsuario(string id, [FromBody] ActualizarUsuarioConfiguracionRequest request)
    {
        if (request is null || !RolesPermitidos.Contains(request.Rol))
        {
            return Problem(detail: "Debe indicar un rol válido.", statusCode: StatusCodes.Status400BadRequest, title: "Datos inválidos");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return Problem(detail: "Usuario no encontrado.", statusCode: StatusCodes.Status404NotFound, title: "Recurso no encontrado");
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(currentUserId, user.Id, StringComparison.Ordinal) && request.Rol != ParametrosNegocio.RolAdministrador)
        {
            return Problem(
                detail: "No puedes removerte el rol Administrador a tu propia cuenta en sesión.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Operación no permitida");
        }

        var rolesActuales = await _userManager.GetRolesAsync(user);
        if (rolesActuales.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, rolesActuales);
        }
        await _userManager.AddToRoleAsync(user, request.Rol);

        if (!string.IsNullOrWhiteSpace(request.NuevaPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetRes = await _userManager.ResetPasswordAsync(user, token, request.NuevaPassword);
            if (!resetRes.Succeeded)
            {
                return Problem(
                    detail: string.Join("; ", resetRes.Errors.Select(e => e.Description)),
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Contraseña inválida");
            }
        }

        var dto = new UsuarioSistemaDto(user.Id, request.Nombre ?? user.Email ?? "-", user.Email ?? "-", request.Rol, true, false);
        return Ok(dto);
    }

    [HttpPatch("usuarios/{id}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstadoUsuario(string id, [FromBody] CambiarEstadoUsuarioRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return Problem(detail: "Usuario no encontrado.", statusCode: StatusCodes.Status404NotFound, title: "Recurso no encontrado");
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(currentUserId, user.Id, StringComparison.Ordinal) && !request.Activo)
        {
            return Problem(
                detail: "No puedes desactivar tu propia cuenta de Administrador en sesión.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Auto-bloqueo prevenido");
        }

        if (request.Activo)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
        }
        else
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }

        return Ok();
    }
}
