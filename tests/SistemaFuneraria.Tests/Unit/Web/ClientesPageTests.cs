using FluentAssertions;
using SistemaFuneraria.Web.Pages.Clientes;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ClientesPageTests
{
    [Fact]
    public void Clientes_OnGet_DebeCargarCorrectamente()
    {
        // Arrange
        var pageModel = new IndexModel();

        // Act
        pageModel.OnGet();

        // Assert
        pageModel.Should().NotBeNull();
    }
}
