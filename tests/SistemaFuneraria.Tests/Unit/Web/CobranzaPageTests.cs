using FluentAssertions;
using SistemaFuneraria.Web.Pages.Cobranza;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class CobranzaPageTests
{
    [Fact]
    public void Cobranza_OnGet_DebeCargarCorrectamente()
    {
        // Arrange
        var pageModel = new IndexModel();

        // Act
        pageModel.OnGet();

        // Assert
        pageModel.Should().NotBeNull();
    }
}
