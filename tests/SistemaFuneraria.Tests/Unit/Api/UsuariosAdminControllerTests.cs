using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SistemaFuneraria.Api.Controllers.V1;
using SistemaFuneraria.Application.DTOs.Admin;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Api;

/// <summary>
/// Pruebas unitarias para el controlador de administración de usuarios y roles (SF-029).
/// </summary>
[Trait("Ticket", "SF-029")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Unit")]
public class UsuariosAdminControllerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _dbContext;

    public UsuariosAdminControllerTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(b => b.AddDebug());
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestAdminDb_{Guid.NewGuid():N}"));

        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _userManager = _serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        _roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private UsuariosAdminController CrearController(string? currentUserId = null)
    {
        var controller = new UsuariosAdminController(_userManager, _roleManager);
        var identity = new ClaimsIdentity("TestAuth");
        if (!string.IsNullOrEmpty(currentUserId))
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, currentUserId));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrador"));
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
        return controller;
    }

    private async Task<IdentityUser> CrearUsuarioAsync(string email, string rol, string nombreCompleto, string sucursal = "Jardines del Valle")
    {
        if (!await _roleManager.RoleExistsAsync(rol))
        {
            await _roleManager.CreateAsync(new IdentityRole(rol));
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, "Password123!");
        result.Succeeded.Should().BeTrue();

        await _userManager.AddToRoleAsync(user, rol);
        string iniciales = nombreCompleto.Length >= 2 ? nombreCompleto[..2].ToUpperInvariant() : "US";
        await _userManager.AddClaimsAsync(user,
        [
            new Claim("NombreCompleto", nombreCompleto),
            new Claim("Iniciales", iniciales),
            new Claim("Sucursal", sucursal)
        ]);

        return user;
    }

    [Fact(DisplayName = "SF-029: Listar usuarios debe retornar 200 OK con usuarios, roles y estado de bloqueo")]
    public async Task Listar_DebeRetornar200OkConListaDeUsuariosYRoles()
    {
        // Arrange
        var admin = await CrearUsuarioAsync("admin@sistemafunerario.mx", "Administrador", "Andrea Morales");
        var vendedor = await CrearUsuarioAsync("vendedor@sistemafunerario.mx", "Vendedor", "Carlos Vega");
        await _userManager.SetLockoutEndDateAsync(vendedor, DateTimeOffset.UtcNow.AddMinutes(15)); // Bloqueado
        var controller = CrearController(admin.Id);

        // Act
        var result = await controller.Listar();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var usuarios = okResult.Value.Should().BeAssignableTo<IEnumerable<UsuarioSistemaDto>>().Subject.ToList();
        usuarios.Should().HaveCount(2);

        var usuarioAdmin = usuarios.First(u => u.Correo == "admin@sistemafunerario.mx");
        usuarioAdmin.Rol.Should().Be("Administrador");
        usuarioAdmin.NombreCompleto.Should().Be("Andrea Morales");
        usuarioAdmin.Bloqueado.Should().BeFalse();

        var usuarioVendedor = usuarios.First(u => u.Correo == "vendedor@sistemafunerario.mx");
        usuarioVendedor.Rol.Should().Be("Vendedor");
        usuarioVendedor.NombreCompleto.Should().Be("Carlos Vega");
        usuarioVendedor.Bloqueado.Should().BeTrue();
    }

    [Fact(DisplayName = "SF-029 (BDD): Crear usuario con datos válidos debe retornar 201 Created y persistir")]
    public async Task Crear_ConDatosValidos_DebeRetornar201CreatedYPersistirEnIdentity()
    {
        // Arrange
        var admin = await CrearUsuarioAsync("admin@sistemafunerario.mx", "Administrador", "Andrea Morales");
        var controller = CrearController(admin.Id);
        var request = new CrearUsuarioRequest(
            NombreCompleto: "Luis Torres",
            Correo: "cajero@sistemafunerario.mx",
            Password: "Password123!",
            Rol: "Cajero",
            Sucursal: "Jardines del Valle"
        );

        // Act
        var result = await controller.Crear(request);

        // Assert
        var createdResult = result.Should().BeOfType<ObjectResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);

        var dto = createdResult.Value.Should().BeOfType<UsuarioSistemaDto>().Subject;
        dto.Correo.Should().Be("cajero@sistemafunerario.mx");
        dto.Rol.Should().Be("Cajero");
        dto.NombreCompleto.Should().Be("Luis Torres");
        dto.Iniciales.Should().Be("LT");
        dto.Bloqueado.Should().BeFalse();

        var userEnDb = await _userManager.FindByEmailAsync("cajero@sistemafunerario.mx");
        userEnDb.Should().NotBeNull();
        var roles = await _userManager.GetRolesAsync(userEnDb!);
        roles.Should().Contain("Cajero");
    }

    [Theory(DisplayName = "SF-029: Crear usuario con rol inválido debe retornar 400 Bad Request")]
    [InlineData("SuperAdmin")]
    [InlineData("Invitado")]
    [InlineData("")]
    public async Task Crear_ConRolInvalido_DebeRetornar400BadRequest(string rolInvalido)
    {
        // Arrange
        var admin = await CrearUsuarioAsync("admin@sistemafunerario.mx", "Administrador", "Andrea Morales");
        var controller = CrearController(admin.Id);
        var request = new CrearUsuarioRequest(
            NombreCompleto: "Test Usuario",
            Correo: "test@sistemafunerario.mx",
            Password: "Password123!",
            Rol: rolInvalido,
            Sucursal: "Jardines del Valle"
        );

        // Act
        var result = await controller.Crear(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact(DisplayName = "SF-029: Actualizar usuario debe modificar rol, datos y desbloquear cuenta si se solicita")]
    public async Task Actualizar_DebeActualizarDatosRolYDesbloquearCuenta()
    {
        // Arrange
        var admin = await CrearUsuarioAsync("admin@sistemafunerario.mx", "Administrador", "Andrea Morales");
        var usuario = await CrearUsuarioAsync("carlos@sistemafunerario.mx", "Vendedor", "Carlos Vega");
        await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.UtcNow.AddMinutes(15));
        var controller = CrearController(admin.Id);

        var request = new ActualizarUsuarioRequest(
            NombreCompleto: "Carlos Vega Actualizado",
            Rol: "Cajero",
            Sucursal: "Sucursal Norte",
            Desbloquear: true
        );

        // Act
        var result = await controller.Actualizar(usuario.Id, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var dto = okResult.Value.Should().BeOfType<UsuarioSistemaDto>().Subject;
        dto.NombreCompleto.Should().Be("Carlos Vega Actualizado");
        dto.Rol.Should().Be("Cajero");
        dto.Sucursal.Should().Be("Sucursal Norte");
        dto.Bloqueado.Should().BeFalse();

        var usuarioActualizado = await _userManager.FindByIdAsync(usuario.Id);
        usuarioActualizado.Should().NotBeNull();
        var roles = await _userManager.GetRolesAsync(usuarioActualizado!);
        roles.Should().ContainSingle().Which.Should().Be("Cajero");
        (await _userManager.IsLockedOutAsync(usuarioActualizado!)).Should().BeFalse();
    }

    [Fact(DisplayName = "SF-029: Administradora no puede eliminarse a sí misma en sesión (400 Bad Request)")]
    public async Task Eliminar_PropiaCuentaAdministrador_DebeRechazarCon400BadRequest()
    {
        // Arrange
        var admin = await CrearUsuarioAsync("admin@sistemafunerario.mx", "Administrador", "Andrea Morales");
        var controller = CrearController(admin.Id);

        // Act (intenta auto-eliminarse)
        var result = await controller.Eliminar(admin.Id);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact(DisplayName = "SF-029: Eliminar otro usuario operativo debe removerlo de Identity y retornar 204 NoContent")]
    public async Task Eliminar_OtroUsuario_DebeEliminarYRetornar204NoContent()
    {
        // Arrange
        var admin = await CrearUsuarioAsync("admin@sistemafunerario.mx", "Administrador", "Andrea Morales");
        var vendedor = await CrearUsuarioAsync("vendedor@sistemafunerario.mx", "Vendedor", "Carlos Vega");
        var controller = CrearController(admin.Id);

        // Act
        var result = await controller.Eliminar(vendedor.Id);

        // Assert
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);

        var usuarioEliminado = await _userManager.FindByIdAsync(vendedor.Id);
        usuarioEliminado.Should().BeNull();
    }

    [Fact(DisplayName = "SF-029: Eliminar usuario no existente debe retornar 404 Not Found")]
    public async Task Eliminar_UsuarioNoExistente_DebeRetornar404NotFound()
    {
        // Arrange
        var admin = await CrearUsuarioAsync("admin@sistemafunerario.mx", "Administrador", "Andrea Morales");
        var controller = CrearController(admin.Id);

        // Act
        var result = await controller.Eliminar("id-inexistente");

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
