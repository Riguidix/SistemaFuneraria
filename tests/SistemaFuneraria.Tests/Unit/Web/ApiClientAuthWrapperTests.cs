using FluentAssertions;

namespace SistemaFuneraria.Tests.Unit.Web;

/// <summary>
/// Pruebas unitarias de contrato para el Fetch API Wrapper (apiClient.js)
/// y el cliente de autenticación con RBAC en UI (authApi.js) (SF-024).
/// </summary>
[Trait("Ticket", "SF-024")]
[Trait("Capa", "Web")]
public class ApiClientAuthWrapperTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string LeerScriptApi(string nombreArchivo)
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", nombreArchivo);
        File.Exists(path).Should().BeTrue($"debe existir el archivo '{path}'");
        return File.ReadAllText(path);
    }

    [Fact(DisplayName = "SF-024: apiClient.js expone get, post, put, delete con Bearer JWT y ProblemDetails")]
    public void ApiClient_DebeExponerVerbosHttpBearerYProblemDetails()
    {
        var contenido = LeerScriptApi("apiClient.js");

        contenido.Should().Contain("get:");
        contenido.Should().Contain("post:");
        contenido.Should().Contain("put:");
        contenido.Should().Contain("delete:");
        contenido.Should().Contain("Authorization");
        contenido.Should().Contain("Bearer");
        contenido.Should().Contain("ApiError");
        contenido.Should().Contain("detail");
        contenido.Should().Contain("status === 204");
        contenido.Should().Contain("status === 401");
        contenido.Should().Contain("isLoginPath(");
        contenido.Should().Contain("buildLoginRedirectUrl(");
        contenido.Should().Contain("toLowerCase().startsWith(\"/cuenta/login\")");
        contenido.Should().Contain("returnUrl=");
    }

    [Fact(DisplayName = "SF-024 (BDD): authApi.js expone login, olvideContrasena, logout, getSession y applyRolePermissions")]
    public void AuthApi_DebeExponerMetodosDeSesionYRbac()
    {
        var contenido = LeerScriptApi("authApi.js");

        contenido.Should().Contain("async login(");
        contenido.Should().Contain("async olvideContrasena(");
        contenido.Should().Contain("logout(");
        contenido.Should().Contain("getSession(");
        contenido.Should().Contain("applyRolePermissions(");
        contenido.Should().Contain("getDefaultRouteForRole(");
        contenido.Should().Contain("sanitizeReturnUrl(");
        contenido.Should().Contain("resolvePostLoginUrl(");
        contenido.Should().Contain("buildRequireAuthRedirectUrl(");
        contenido.Should().Contain("\"/Caja\"");
        contenido.Should().Contain("\"/Resumen\"");
        contenido.Should().Contain("/api/v1/auth/login");
        contenido.Should().Contain("/api/v1/auth/olvide-contrasena");
        contenido.Should().Contain("data-roles");
        contenido.Should().Contain("d-none");
        contenido.Should().Contain("Administrador");
    }
}
