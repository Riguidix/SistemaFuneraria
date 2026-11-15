using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Application.Servicios.Simulador;
using SistemaFuneraria.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);
const string corsPolicy = "FunerariaWebPolicy";

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ISimuladorService, SimuladorService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5080", "https://localhost:7080")
              .WithHeaders("Authorization", "Content-Type")
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolicy);
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
