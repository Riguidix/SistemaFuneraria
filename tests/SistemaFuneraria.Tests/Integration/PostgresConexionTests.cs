using System.Net.Sockets;
using FluentAssertions;

namespace SistemaFuneraria.Tests.Integration;

public class PostgresConexionTests
{
    private const string Host = "localhost";
    private const int Port = 5432;

    [Fact]
    [Trait("Ticket", "SF-002")]
    public async Task PostgresContenedor_DebeAceptarConexionesEnPuerto5432()
    {
        // Arrange & Act
        using var client = new TcpClient();
        var connectTask = client.ConnectAsync(Host, Port);
        var completedTask = await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(5)));

        // Assert
        completedTask.Should().Be(connectTask, "el contenedor PostgreSQL 16 debe responder en menos de 5 segundos");
        client.Connected.Should().BeTrue("el socket debe conectarse exitosamente a localhost:5432 (funeraria-db)");
    }
}
