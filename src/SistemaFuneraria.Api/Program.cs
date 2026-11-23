using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Application.Servicios.Cobranza;
using SistemaFuneraria.Application.Servicios.Contratos;
using SistemaFuneraria.Application.Servicios.Lotes;
using SistemaFuneraria.Application.Servicios.Simulador;
using SistemaFuneraria.Application.Servicios.Ventas;
using SistemaFuneraria.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);
const string corsPolicy = "FunerariaWebPolicy";

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("La clave de firma 'Jwt:Key' no está configurada.");
var jwtIssuer = jwtSection["Issuer"] ?? "SistemaFuneraria.Api";
var jwtAudience = jwtSection["Audience"] ?? "SistemaFuneraria.Web";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<ISimuladorService, SimuladorService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<ICarteraContratosService, CarteraContratosService>();
builder.Services.AddScoped<ICobranzaService, CobranzaService>();

var allowedOriginsRaw = builder.Configuration["Cors:AllowedOrigins"];
var allowedOrigins = string.IsNullOrWhiteSpace(allowedOriginsRaw)
    ? new[] { "http://localhost:5080", "https://localhost:7080" }
    : allowedOriginsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithHeaders("Authorization", "Content-Type", "X-Idempotency-Key")
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Sistema Funeraria API",
        Version = "v1",
    });
});

var app = builder.Build();

var autoMigrate = builder.Configuration.GetValue<bool?>("Database:AutoMigrate")
    ?? app.Environment.IsDevelopment();
var seedDemoUsers = builder.Configuration.GetValue<bool?>("Database:SeedDemoUsers")
    ?? app.Environment.IsDevelopment();

if (autoMigrate || seedDemoUsers)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (autoMigrate && db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }

        if (seedDemoUsers)
        {
            await IdentitySeed.SeedAsync(app.Services);
            await LoteSeed.SeedAsync(app.Services);
            await CarteraDemoSeed.SeedAsync(app.Services);
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al migrar o sembrar la base de datos al iniciar la API.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = true;
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Funeraria");
    });
}

app.UseCors(corsPolicy);
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program { }
