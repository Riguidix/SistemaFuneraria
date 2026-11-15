using System.Diagnostics;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SistemaFuneraria.Api.Controllers.V1;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Api;

/// <summary>
/// Pruebas unitarias de la máquina de estados de autenticación, bloqueo temporal al 5.º intento
/// y mitigación de enumeración (SF-023).
/// </summary>
[Trait("Ticket", "SF-023")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Unit")]
public class IniciarSesionUsuarioTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _dbContext;

    public IniciarSesionUsuarioTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(b => b.AddDebug());
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestAuthDb_{Guid.NewGuid():N}"));

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

    private AuthController CrearController(ClaimsPrincipal? user = null)
    {
        var controller = new AuthController(_userManager);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user ?? new ClaimsPrincipal()
            }
        };
        return controller;
    }

    private async Task<IdentityUser> CrearUsuarioDemoAsync(string email, string password = "Password123!")
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        createResult.Succeeded.Should().BeTrue();

        if (!await _roleManager.RoleExistsAsync("Administrador"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Administrador"));
        }

        await _userManager.AddToRoleAsync(user, "Administrador");
        await _userManager.AddClaimsAsync(user,
        [
            new Claim("NombreCompleto", "Andrea Morales"),
            new Claim("Iniciales", "AM"),
            new Claim("Sucursal", "Jardines del Valle")
        ]);

        return user;
    }

    [Fact(DisplayName = "SF-023: Login con credenciales válidas debe retornar 200 OK con token y perfil")]
    public async Task Login_ConCredencialesValidas_DebeRetornar200OkConTokenYDatosUsuario()
    {
        // Arrange
        const string email = "andrea.morales@sistemafunerario.mx";
        const string password = "Password123!";
        var user = await CrearUsuarioDemoAsync(email, password);
        var controller = CrearController();

        // Act
        var result = await controller.Login(new LoginRequest(email, password));

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var dto = okResult.Value.Should().BeOfType<LoginResponseDto>().Subject;
        dto.Token.Should().NotBeNullOrWhiteSpace();
        dto.UsuarioId.Should().Be(user.Id);
        dto.Correo.Should().Be(email);
        dto.NombreCompleto.Should().Be("Andrea Morales");
        dto.Iniciales.Should().Be("AM");
        dto.Rol.Should().Be("Administrador");
        dto.Sucursal.Should().Be("Jardines del Valle");
        dto.IntentosFallidos.Should().Be(0);
        dto.MinutosBloqueoRestantes.Should().BeNull();
        dto.AdvertenciaSeguridad.Should().BeNull();
    }

    [Fact(DisplayName = "SF-023: Login con contraseña incorrecta en 1er intento debe retornar 401 Unauthorized")]
    public async Task Login_ConContrasenaIncorrectaPrimerIntento_DebeRetornar401Unauthorized()
    {
        // Arrange
        const string email = "andrea.morales@sistemafunerario.mx";
        await CrearUsuarioDemoAsync(email);
        var controller = CrearController();

        // Act
        var result = await controller.Login(new LoginRequest(email, "PasswordIncorrecta"));

        // Assert
        var unauthResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var dto = unauthResult.Value.Should().BeOfType<LoginResponseDto>().Subject;
        dto.IntentosFallidos.Should().Be(1);
        dto.MinutosBloqueoRestantes.Should().BeNull();
        dto.AdvertenciaSeguridad.Should().BeNull();
    }

    [Fact(DisplayName = "SF-023: Login con 3 intentos fallidos debe retornar 401 Unauthorized y advertencia preventiva")]
    public async Task Login_ConTresIntentosFallidos_DebeRetornar401ConAdvertenciaDeSeguridad()
    {
        // Arrange
        const string email = "andrea.morales@sistemafunerario.mx";
        var user = await CrearUsuarioDemoAsync(email);
        await _userManager.AccessFailedAsync(user);
        await _userManager.AccessFailedAsync(user); // Ya van 2 intentos previos
        var controller = CrearController();

        // Act (3er intento consecutivo)
        var result = await controller.Login(new LoginRequest(email, "PasswordIncorrecta"));

        // Assert
        var unauthResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var dto = unauthResult.Value.Should().BeOfType<LoginResponseDto>().Subject;
        dto.IntentosFallidos.Should().Be(3);
        dto.MinutosBloqueoRestantes.Should().BeNull();
        dto.AdvertenciaSeguridad.Should().Be("Advertencia: Si alcanzas 5 intentos fallidos tu cuenta será bloqueada temporalmente por seguridad");
    }

    [Fact(DisplayName = "SF-023 (BDD): Quinto intento fallido consecutivo debe bloquear la cuenta por 15 minutos y retornar 423 Locked")]
    public async Task Login_ConQuintoIntentoFallido_DebeBloquearCuentaYRetornar423Locked()
    {
        // Arrange
        const string email = "andrea.morales@sistemafunerario.mx";
        var user = await CrearUsuarioDemoAsync(email);
        // Simular 4 intentos previos
        for (int i = 0; i < 4; i++)
        {
            await _userManager.AccessFailedAsync(user);
        }
        var controller = CrearController();

        // Act (5.º intento consecutivo)
        var result = await controller.Login(new LoginRequest(email, "PasswordIncorrecta"));

        // Assert
        var lockedResult = result.Should().BeOfType<ObjectResult>().Subject;
        lockedResult.StatusCode.Should().Be(StatusCodes.Status423Locked);

        var dto = lockedResult.Value.Should().BeOfType<LoginResponseDto>().Subject;
        dto.IntentosFallidos.Should().Be(5);
        dto.MinutosBloqueoRestantes.Should().Be(15);
        dto.AdvertenciaSeguridad.Should().Be("Cuenta bloqueada temporalmente. Intenta nuevamente en 15 minutos.");

        // Verificar persistencia del bloqueo en Identity
        var usuarioActualizado = await _userManager.FindByEmailAsync(email);
        usuarioActualizado.Should().NotBeNull();
        usuarioActualizado!.LockoutEnd.Should().NotBeNull();
        usuarioActualizado.LockoutEnd!.Value.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(14));
    }

    [Fact(DisplayName = "SF-023: Usuario bloqueado que intenta login debe recibir 423 Locked con minutos restantes")]
    public async Task Login_ConCuentaYaBloqueada_DebeRetornar423LockedSinProcesarContrasena()
    {
        // Arrange
        const string email = "andrea.morales@sistemafunerario.mx";
        var user = await CrearUsuarioDemoAsync(email);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(10));
        var controller = CrearController();

        // Act (Incluso con contraseña correcta, la cuenta está bloqueada)
        var result = await controller.Login(new LoginRequest(email, "Password123!"));

        // Assert
        var lockedResult = result.Should().BeOfType<ObjectResult>().Subject;
        lockedResult.StatusCode.Should().Be(StatusCodes.Status423Locked);

        var dto = lockedResult.Value.Should().BeOfType<LoginResponseDto>().Subject;
        dto.MinutosBloqueoRestantes.Should().BeInRange(9, 10);
        dto.AdvertenciaSeguridad.Should().Contain("Cuenta bloqueada temporalmente.");
    }

    [Fact(DisplayName = "SF-023: Login con usuario inexistente debe retornar 401 Unauthorized sin revelar existencia")]
    public async Task Login_ConUsuarioInexistente_DebeRetornar401Unauthorized()
    {
        // Arrange
        var controller = CrearController();

        // Act
        var result = await controller.Login(new LoginRequest("noexiste@sistemafunerario.mx", "CualquierPass!"));

        // Assert
        var unauthResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact(DisplayName = "SF-023: OlvideContrasena debe responder 200 OK con jitter anti-enumeración entre 200ms y 600ms")]
    public async Task OlvideContrasena_DebeResponder200OkConJitterAntiEnumeracion()
    {
        // Arrange
        var controller = CrearController();
        var sw = Stopwatch.StartNew();

        // Act
        var result = await controller.OlvideContrasena(new OlvideContrasenaRequest("cualquiera@sistemafunerario.mx"));
        sw.Stop();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var dto = okResult.Value.Should().BeOfType<OlvideContrasenaResponseDto>().Subject;
        dto.Mensaje.Should().Be("Si el correo ingresado se encuentra registrado, recibirás un enlace de restablecimiento en breve");

        // El jitter debe retrasar la respuesta al menos 190ms (tolerancia de reloj para 200-600ms)
        sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(190, "debe aplicar un retardo anti-timing attack");
    }

    [Fact(DisplayName = "SF-023: Me debe retornar perfil del usuario autenticado en sesión")]
    public async Task Me_ConUsuarioAutenticado_DebeRetornarPerfil200Ok()
    {
        // Arrange
        const string email = "andrea.morales@sistemafunerario.mx";
        var user = await CrearUsuarioDemoAsync(email);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "Administrador")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var controller = CrearController(principal);

        // Act
        var result = await controller.Me();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var perfil = okResult.Value.Should().BeOfType<UsuarioPerfilDto>().Subject;
        perfil.UsuarioId.Should().Be(user.Id);
        perfil.Correo.Should().Be(email);
        perfil.NombreCompleto.Should().Be("Andrea Morales");
        perfil.Iniciales.Should().Be("AM");
        perfil.Rol.Should().Be("Administrador");
        perfil.Sucursal.Should().Be("Jardines del Valle");
    }
}
