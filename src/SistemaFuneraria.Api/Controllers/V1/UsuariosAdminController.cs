using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.DTOs.Admin;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador de administración de usuarios y roles exclusivo para el rol Administrador (SF-029).
/// </summary>
[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/v1/admin/usuarios")]
public class UsuariosAdminController(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) : ApiControllerBase
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    private static readonly HashSet<string> RolesValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Administrador",
        "Vendedor",
        "Cajero"
    };

    /// <summary>
    /// Lista todos los usuarios operativos del sistema con sus roles y estado de bloqueo (SF-029).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var users = await _userManager.Users.ToListAsync();
        var dtoList = new List<UsuarioSistemaDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            string rol = roles.FirstOrDefault() ?? "Vendedor";

            var claims = await _userManager.GetClaimsAsync(user);
            string nombreCompleto = claims.FirstOrDefault(c => c.Type == "NombreCompleto")?.Value
                ?? user.UserName
                ?? "Usuario";
            string iniciales = claims.FirstOrDefault(c => c.Type == "Iniciales")?.Value
                ?? ObtenerIniciales(nombreCompleto);
            string sucursal = claims.FirstOrDefault(c => c.Type == "Sucursal")?.Value
                ?? "Jardines del Valle";

            bool bloqueado = await _userManager.IsLockedOutAsync(user);

            dtoList.Add(new UsuarioSistemaDto(
                Id: user.Id,
                NombreCompleto: nombreCompleto,
                Iniciales: iniciales,
                Correo: user.Email ?? string.Empty,
                Rol: rol,
                Sucursal: sucursal,
                Bloqueado: bloqueado
            ));
        }

        return Ok(dtoList);
    }

    /// <summary>
    /// Registra un nuevo usuario con rol asignado y claims de perfil (SF-029).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.NombreCompleto) ||
            string.IsNullOrWhiteSpace(request.Correo) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Rol))
        {
            return BadRequest(Problem(
                detail: "Todos los campos (NombreCompleto, Correo, Password, Rol) son requeridos.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Datos de usuario incompletos"));
        }

        if (!RolesValidos.Contains(request.Rol))
        {
            return BadRequest(Problem(
                detail: $"El rol '{request.Rol}' no es válido. Roles autorizados: Administrador, Vendedor, Cajero.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Rol no permitido"));
        }

        var user = new IdentityUser
        {
            UserName = request.Correo,
            Email = request.Correo,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            string errorDetalle = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest(Problem(
                detail: errorDetalle,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Error al crear usuario"));
        }

        if (!await _roleManager.RoleExistsAsync(request.Rol))
        {
            await _roleManager.CreateAsync(new IdentityRole(request.Rol));
        }
        await _userManager.AddToRoleAsync(user, request.Rol);

        string sucursal = string.IsNullOrWhiteSpace(request.Sucursal) ? "Jardines del Valle" : request.Sucursal;
        string iniciales = ObtenerIniciales(request.NombreCompleto);

        await _userManager.AddClaimsAsync(user,
        [
            new Claim("NombreCompleto", request.NombreCompleto),
            new Claim("Iniciales", iniciales),
            new Claim("Sucursal", sucursal)
        ]);

        var dto = new UsuarioSistemaDto(
            Id: user.Id,
            NombreCompleto: request.NombreCompleto,
            Iniciales: iniciales,
            Correo: user.Email,
            Rol: request.Rol,
            Sucursal: sucursal,
            Bloqueado: false
        );

        return StatusCode(StatusCodes.Status201Created, dto);
    }

    /// <summary>
    /// Actualiza información de perfil, rol o desbloquea una cuenta de usuario (SF-029).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(string id, [FromBody] ActualizarUsuarioRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(Problem(
                detail: $"No se encontró el usuario con ID '{id}'.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Usuario no encontrado"));
        }

        if (!string.IsNullOrWhiteSpace(request.Rol) && !RolesValidos.Contains(request.Rol))
        {
            return BadRequest(Problem(
                detail: $"El rol '{request.Rol}' no es válido. Roles autorizados: Administrador, Vendedor, Cajero.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Rol no permitido"));
        }

        var claimsActuales = await _userManager.GetClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(request.NombreCompleto))
        {
            var claimNombre = claimsActuales.FirstOrDefault(c => c.Type == "NombreCompleto");
            if (claimNombre != null) await _userManager.RemoveClaimAsync(user, claimNombre);
            await _userManager.AddClaimAsync(user, new Claim("NombreCompleto", request.NombreCompleto));

            string iniciales = ObtenerIniciales(request.NombreCompleto);
            var claimIniciales = claimsActuales.FirstOrDefault(c => c.Type == "Iniciales");
            if (claimIniciales != null) await _userManager.RemoveClaimAsync(user, claimIniciales);
            await _userManager.AddClaimAsync(user, new Claim("Iniciales", iniciales));
        }

        if (!string.IsNullOrWhiteSpace(request.Sucursal))
        {
            var claimSucursal = claimsActuales.FirstOrDefault(c => c.Type == "Sucursal");
            if (claimSucursal != null) await _userManager.RemoveClaimAsync(user, claimSucursal);
            await _userManager.AddClaimAsync(user, new Claim("Sucursal", request.Sucursal));
        }

        if (!string.IsNullOrWhiteSpace(request.Rol))
        {
            var rolesActuales = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, rolesActuales);
            if (!await _roleManager.RoleExistsAsync(request.Rol))
            {
                await _roleManager.CreateAsync(new IdentityRole(request.Rol));
            }
            await _userManager.AddToRoleAsync(user, request.Rol);
        }

        if (request.Desbloquear)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        var claimsFinales = await _userManager.GetClaimsAsync(user);
        var rolesFinales = await _userManager.GetRolesAsync(user);
        string nombre = claimsFinales.FirstOrDefault(c => c.Type == "NombreCompleto")?.Value ?? user.UserName ?? "Usuario";
        string inic = claimsFinales.FirstOrDefault(c => c.Type == "Iniciales")?.Value ?? ObtenerIniciales(nombre);
        string sucursal = claimsFinales.FirstOrDefault(c => c.Type == "Sucursal")?.Value ?? "Jardines del Valle";
        string rolFinal = rolesFinales.FirstOrDefault() ?? "Vendedor";
        bool bloqueado = await _userManager.IsLockedOutAsync(user);

        var dto = new UsuarioSistemaDto(
            Id: user.Id,
            NombreCompleto: nombre,
            Iniciales: inic,
            Correo: user.Email ?? string.Empty,
            Rol: rolFinal,
            Sucursal: sucursal,
            Bloqueado: bloqueado
        );

        return Ok(dto);
    }

    /// <summary>
    /// Elimina una cuenta de usuario impidiendo la auto-eliminación de la administradora activa (SF-029).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(currentUserId) && currentUserId.Equals(id, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(Problem(
                detail: "No puedes eliminar tu propia cuenta de administrador en sesión.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Auto-eliminación prohibida"));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(Problem(
                detail: $"No se encontró el usuario con ID '{id}'.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Usuario no encontrado"));
        }

        await _userManager.DeleteAsync(user);
        return NoContent();
    }

    private static string ObtenerIniciales(string nombreCompleto)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto)) return "US";
        var partes = nombreCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length >= 2)
        {
            return $"{char.ToUpperInvariant(partes[0][0])}{char.ToUpperInvariant(partes[1][0])}";
        }
        return partes[0].Length >= 2 ? partes[0][..2].ToUpperInvariant() : partes[0].ToUpperInvariant();
    }
}
