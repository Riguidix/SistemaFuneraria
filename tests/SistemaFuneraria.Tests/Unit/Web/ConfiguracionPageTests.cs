using System.Diagnostics;
using FluentAssertions;
using SistemaFuneraria.Web.Pages.Configuracion;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ConfiguracionPageTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string RutaWeb(params string[] segmentos) =>
        Path.Combine(new[] { RepoRoot, "src", "SistemaFuneraria.Web" }.Concat(segmentos).ToArray());

    [Fact]
    public void Configuracion_OnGet_DebeInicializarLimpioYConParametrosRealesDelDominio()
    {
        var pageModel = new IndexModel();

        pageModel.OnGet();

        pageModel.NombreComercial.Should().BeEmpty();
        pageModel.Correo.Should().BeEmpty();
        pageModel.Usuarios.Should().BeEmpty();
        pageModel.SeccionActiva.Should().Be("DatosSucursal");

        pageModel.PlanesLotes.Should().HaveCount(3);
        pageModel.PlanesLotes.Select(p => p.PrecioLista).Should().ContainInOrder(120000m, 150000m, 180000m);
        pageModel.TasasPlazos.Should().HaveCount(4);
        pageModel.TasasPlazos.Select(t => t.TasaAnual).Should().ContainInOrder(16.0m, 18.0m, 20.0m, 24.0m);
    }

    [Fact]
    public void ConfiguracionVistaYJs_DebenSoportarModalUsuarios403YSeccionesInformativas()
    {
        var html = File.ReadAllText(RutaWeb("Pages", "Configuracion", "Index.cshtml"));
        var js = File.ReadAllText(RutaWeb("wwwroot", "js", "pages", "configuracionPage.js"));

        html.Should().NotContain("+ Nuevo usuario");
        html.Should().NotContain("Andrea Morales");
        html.Should().Contain("id=\"configAccesoRestringido\"");
        html.Should().Contain("id=\"alertaErrorConfig\"");
        html.Should().Contain("id=\"btnNuevoUsuarioConfig\"");
        html.Should().Contain("id=\"modalUsuarioConfig\"");
        html.Should().Contain("id=\"btnGuardarUsuarioConfig\"");
        html.Should().Contain("Regla de dominio fija (RN-10)");
        html.Should().Contain("Solo informativo · Próximamente");

        js.Should().Contain("getToken");
        js.Should().Contain("returnUrl");
        js.Should().Contain("validarFormularioUsuario");
        js.Should().Contain("crearUsuario");
        js.Should().Contain("actualizarUsuario");
    }

    [Fact]
    public async Task ConfiguracionPageJs_RbacBotonesYUsuarios_DebenPasarEnNode()
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"sf_config_page_{Guid.NewGuid():N}.cjs");
        var jsPath = RutaWeb("wwwroot", "js", "pages", "configuracionPage.js");

        var script = """
            const assert = require("node:assert/strict");
            const cfg = require(process.argv[2]);

            assert.match(cfg.validarFormularioUsuario({ nombre: "", correo: "a@b.mx", rol: "Vendedor", password: "Password1" }, false), /obligatorio/);
            assert.match(cfg.validarFormularioUsuario({ nombre: "Ana", correo: "mala", rol: "Vendedor", password: "Password1" }, false), /correo/);
            assert.equal(cfg.validarFormularioUsuario({ nombre: "Ana", correo: "", rol: "Cajero", password: "" }, true), null);

            function mkEl(initial = {}) {
                const classes = new Set(initial.classes || []);
                return {
                    id: initial.id || "",
                    value: initial.value ?? "",
                    textContent: "",
                    innerHTML: "",
                    disabled: Boolean(initial.disabled),
                    readOnly: false,
                    required: false,
                    attrs: { ...(initial.attrs || {}) },
                    classList: {
                        add: (...c) => c.forEach((x) => classes.add(x)),
                        remove: (...c) => c.forEach((x) => classes.delete(x)),
                        toggle: (c, force) => (force === undefined ? (classes.has(c) ? classes.delete(c) : classes.add(c)) : (force ? classes.add(c) : classes.delete(c))),
                        contains: (c) => classes.has(c)
                    },
                    addEventListener() {},
                    setAttribute(k, v) { this.attrs[k] = v; },
                    getAttribute(k) { return this.attrs[k] || null; },
                    removeAttribute(k) { delete this.attrs[k]; },
                    querySelector() { return null; },
                    querySelectorAll() { return []; }
                };
            }

            (async () => {
                // 1. Cajero recibe 403 en Configuracion
                const els403 = {
                    configContenidoPrincipal: mkEl(),
                    configAccesoRestringido: mkEl({ classes: ["d-none"] }),
                    alertaErrorConfig: mkEl({ classes: ["d-none"] }),
                    btnVolver403Config: mkEl()
                };
                cfg.init({
                    document: { getElementById: (id) => els403[id] || null, querySelectorAll: () => [] },
                    window: { location: { pathname: "/Configuracion", search: "" } },
                    authApi: {
                        getToken: () => "jwt",
                        getSession: () => ({ rol: "Cajero" }),
                        isTokenExpired: () => false,
                        getDefaultRouteForRole: () => "/Caja"
                    },
                    apiClient: { TOKEN_STORAGE_KEY: "sf_token" }
                });
                assert.equal(els403.configContenidoPrincipal.classList.contains("d-none"), true);
                assert.equal(els403.configAccesoRestringido.classList.contains("d-none"), false);
                assert.equal(els403.btnVolver403Config.attrs.href, "/Caja");

                // 2. Administrador: botón Guardar cambios contextual + filas con editar y toggle
                const panelSucursal = mkEl({ id: "panel-datos-sucursal" });
                const panelUsuarios = mkEl({ id: "panel-usuarios-roles", classes: ["d-none"] });
                const elsAdmin = {
                    configContenidoPrincipal: mkEl(),
                    configAccesoRestringido: mkEl({ classes: ["d-none"] }),
                    alertaErrorConfig: mkEl({ classes: ["d-none"] }),
                    btnGuardarCambios: mkEl(),
                    tbodyUsuariosConfig: mkEl(),
                    inputNombreComercial: mkEl(),
                    inputTelefono: mkEl(),
                    inputCorreo: mkEl(),
                    inputDomicilio: mkEl(),
                    inputCiudad: mkEl(),
                    inputCodigoPostal: mkEl(),
                    switchRecordatorios: mkEl(),
                    switchResumenCaja: mkEl()
                };
                const instance = cfg.init({
                    document: {
                        getElementById: (id) => elsAdmin[id] || null,
                        querySelectorAll: (sel) => sel === ".sf-config-panel" ? [panelSucursal, panelUsuarios] : []
                    },
                    window: { location: { pathname: "/Configuracion", search: "" } },
                    authApi: { getToken: () => "jwt", getSession: () => ({ rol: "Administrador" }), isTokenExpired: () => false },
                    apiClient: { TOKEN_STORAGE_KEY: "sf_token" },
                    configuracionApi: {
                        obtenerSucursal: async () => ({ nombreComercial: "Funeraria Test", correoOperativo: "admin@test.mx", recordatoriosPago: true }),
                        listarUsuarios: async () => ([{ id: "u1", nombre: "Sofía Ramírez", correo: "sofia@test.mx", rol: "Vendedor", activo: true, bloqueado: false }])
                    }
                });

                await new Promise((r) => setTimeout(r, 10));
                assert.match(elsAdmin.tbodyUsuariosConfig.innerHTML, /btn-editar-usuario/);
                assert.match(elsAdmin.tbodyUsuariosConfig.innerHTML, /btn-toggle-usuario/);

                instance.activarSeccion("panel-usuarios-roles");
                assert.equal(elsAdmin.btnGuardarCambios.classList.contains("d-none"), true);
                assert.equal(panelSucursal.classList.contains("d-none"), true);
                assert.equal(panelUsuarios.classList.contains("d-none"), false);

                instance.activarSeccion("panel-datos-sucursal");
                assert.equal(elsAdmin.btnGuardarCambios.classList.contains("d-none"), false);
            })().catch((err) => {
                console.error(err);
                process.exit(1);
            });
            """;

        await File.WriteAllTextAsync(scriptPath, script);
        try
        {
            var psi = new ProcessStartInfo("node")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            psi.ArgumentList.Add(scriptPath);
            psi.ArgumentList.Add(jsPath);

            using var proc = Process.Start(psi)!;
            var stderr = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();
            proc.ExitCode.Should().Be(0, stderr);
        }
        finally
        {
            if (File.Exists(scriptPath)) File.Delete(scriptPath);
        }
    }
}
