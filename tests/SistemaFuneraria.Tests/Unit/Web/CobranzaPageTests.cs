using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Web.Pages.Cobranza;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class CobranzaPageTests
{
    [Fact]
    public void Cobranza_OnGet_DebeRedirigirACajaIndex()
    {
        var pageModel = new IndexModel();

        var resultado = pageModel.OnGet();

        var redirect = resultado.Should().BeOfType<RedirectToPageResult>().Subject;
        redirect.PageName.Should().Be("/Caja/Index");
    }
}
