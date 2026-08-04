using FluentAssertions;
using SolarSystem.Domain.Common;

namespace SolarSystem.Tests.Domain;

public class BrazilianStatesTests
{
    [Fact]
    public void All_tem_as_27_unidades_federativas()
    {
        BrazilianStates.All.Should().HaveCount(27);
    }

    [Theory]
    [InlineData("SP")]
    [InlineData("sp")]
    [InlineData("Rj")]
    [InlineData("DF")]
    public void IsValid_aceita_uf_existente_em_qualquer_caixa(string uf)
    {
        BrazilianStates.IsValid(uf).Should().BeTrue();
    }

    [Theory]
    [InlineData("ZZ")]  // 2 letras, mas nao existe
    [InlineData("XXX")]
    [InlineData("S")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void IsValid_rejeita_uf_inexistente(string? uf)
    {
        BrazilianStates.IsValid(uf).Should().BeFalse();
    }
}
