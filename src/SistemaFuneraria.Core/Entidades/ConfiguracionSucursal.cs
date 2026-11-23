using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Entidad de configuración operativa de la sucursal (SF-091 / Captura 12).
/// </summary>
public class ConfiguracionSucursal
{
    public Guid Id { get; private set; }
    public string NombreComercial { get; private set; } = string.Empty;
    public string TelefonoAtencion { get; private set; } = string.Empty;
    public string CorreoOperativo { get; private set; } = string.Empty;
    public string Direccion { get; private set; } = string.Empty;
    public string Ciudad { get; private set; } = string.Empty;
    public string CodigoPostal { get; private set; } = string.Empty;
    public bool RecordatoriosPago { get; private set; }
    public bool ResumenDiarioCaja { get; private set; }
    public DateTimeOffset FechaActualizacion { get; private set; }

    protected ConfiguracionSucursal() { }

    private ConfiguracionSucursal(
        Guid id,
        string nombreComercial,
        string telefonoAtencion,
        string correoOperativo,
        string direccion,
        string ciudad,
        string codigoPostal,
        bool recordatoriosPago,
        bool resumenDiarioCaja,
        DateTimeOffset fechaActualizacion)
    {
        Id = id;
        NombreComercial = nombreComercial;
        TelefonoAtencion = telefonoAtencion;
        CorreoOperativo = correoOperativo;
        Direccion = direccion;
        Ciudad = ciudad;
        CodigoPostal = codigoPostal;
        RecordatoriosPago = recordatoriosPago;
        ResumenDiarioCaja = resumenDiarioCaja;
        FechaActualizacion = fechaActualizacion;
    }

    public static ConfiguracionSucursal CrearPorDefecto() => new(
        id: Guid.NewGuid(),
        nombreComercial: "Jardines del Valle",
        telefonoAtencion: "55 4120 9800",
        correoOperativo: "operacion@sistemafunerario.mx",
        direccion: "Av. Paseo de los Cipreses 420, Zapopan, Jalisco",
        ciudad: "Zapopan, Jalisco",
        codigoPostal: "45010",
        recordatoriosPago: true,
        resumenDiarioCaja: true,
        fechaActualizacion: DateTimeOffset.UtcNow);

    public void Actualizar(
        string nombreComercial,
        string telefonoAtencion,
        string correoOperativo,
        string direccion,
        string? ciudad,
        string? codigoPostal,
        bool recordatoriosPago,
        bool resumenDiarioCaja)
    {
        if (string.IsNullOrWhiteSpace(nombreComercial))
            throw new ReglaNegocioException("El nombre comercial de la sucursal es obligatorio.");

        if (string.IsNullOrWhiteSpace(telefonoAtencion))
            throw new ReglaNegocioException("El teléfono de atención es obligatorio.");

        if (string.IsNullOrWhiteSpace(correoOperativo))
            throw new ReglaNegocioException("El correo operativo es obligatorio.");

        if (string.IsNullOrWhiteSpace(direccion))
            throw new ReglaNegocioException("La dirección de la sucursal es obligatoria.");

        NombreComercial = nombreComercial.Trim();
        TelefonoAtencion = telefonoAtencion.Trim();
        CorreoOperativo = correoOperativo.Trim();
        Direccion = direccion.Trim();
        Ciudad = string.IsNullOrWhiteSpace(ciudad) ? Ciudad : ciudad.Trim();
        CodigoPostal = string.IsNullOrWhiteSpace(codigoPostal) ? CodigoPostal : codigoPostal.Trim();
        RecordatoriosPago = recordatoriosPago;
        ResumenDiarioCaja = resumenDiarioCaja;
        FechaActualizacion = DateTimeOffset.UtcNow;
    }
}
