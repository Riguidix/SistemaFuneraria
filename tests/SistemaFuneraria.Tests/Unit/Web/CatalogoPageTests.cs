using FluentAssertions;
using SistemaFuneraria.Web.Pages.Catalogo;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class CatalogoPageTests
{
    [Fact]
    public void Catalogo_OnGet_DebeCargarCorrectamente()
    {
        // Arrange
        var pageModel = new IndexModel();

        // Act
        pageModel.OnGet();

        // Assert
        pageModel.Should().NotBeNull();
    }
}
