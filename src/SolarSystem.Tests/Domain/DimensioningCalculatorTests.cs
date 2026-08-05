using FluentAssertions;
using SolarSystem.Domain.Common;
using SolarSystem.Domain.Dimensioning;

namespace SolarSystem.Tests.Domain;

public class DimensioningCalculatorTests
{
    private const decimal IrradiationSp = 4.82m;

    private static DimensioningInput Input(
        int consumption = 500,
        decimal irradiation = IrradiationSp,
        decimal lossFactor = 0.80m,
        RoofOrientation orientation = RoofOrientation.North,
        int modulePowerW = 550,
        int? manualQuantity = null,
        decimal? manualPower = null)
        => new(consumption, irradiation, lossFactor, orientation, modulePowerW, manualQuantity, manualPower);

    [Fact]
    public void Aplica_a_formula_do_epico()
    {
        // Potência = Consumo / (Irradiação × 30 × Fator de perda)
        // 500 / (4.82 × 30 × 0.80) = 500 / 115.68 = 4.32 kWp
        var result = DimensioningCalculator.Calculate(Input());

        result.RequiredPowerKwp.Should().Be(4.32m);
    }

    [Fact]
    public void Arredonda_a_quantidade_de_modulos_para_cima()
    {
        // 4.32 kWp / 550 W = 7.86 módulos -> 8, nunca 7 (não se instala meio módulo)
        var result = DimensioningCalculator.Calculate(Input());

        result.ModuleQuantity.Should().Be(8);
        result.InstalledPowerKwp.Should().Be(4.4m);
    }

    [Fact]
    public void Potencia_instalada_cobre_a_potencia_necessaria()
    {
        var result = DimensioningCalculator.Calculate(Input());

        result.InstalledPowerKwp.Should().BeGreaterThanOrEqualTo(result.RequiredPowerKwp);
    }

    [Fact]
    public void Geracao_estimada_atende_o_consumo_informado()
    {
        var result = DimensioningCalculator.Calculate(Input(consumption: 500));

        result.GenerationMonthlyKwh.Should().BeGreaterThanOrEqualTo(500);
        result.GenerationYearlyKwh.Should().Be(result.GenerationMonthlyKwh * 12);
    }

    [Fact]
    public void Sugere_inversor_aceitando_ate_20_por_cento_de_sobredimensionamento()
    {
        // 4.4 kWp / 1.2 = 3.67 -> menor inversor padrão que atende é o de 5 kW
        var result = DimensioningCalculator.Calculate(Input());

        result.InverterPowerKw.Should().Be(5m);
    }

    [Theory]
    [InlineData(RoofOrientation.North, 1.00)]
    [InlineData(RoofOrientation.NorthEast, 0.95)]
    [InlineData(RoofOrientation.East, 0.85)]
    [InlineData(RoofOrientation.South, 0.75)]
    public void Orientacao_reduz_a_irradiacao_efetiva(RoofOrientation orientation, double factor)
    {
        var result = DimensioningCalculator.Calculate(Input(orientation: orientation));

        var esperado = Math.Round(IrradiationSp * 0.80m * (decimal)factor, 2, MidpointRounding.AwayFromZero);
        result.EffectiveIrradiation.Should().Be(esperado);
    }

    [Fact]
    public void Telhado_sul_exige_mais_modulos_que_telhado_norte()
    {
        var norte = DimensioningCalculator.Calculate(Input(orientation: RoofOrientation.North));
        var sul = DimensioningCalculator.Calculate(Input(orientation: RoofOrientation.South));

        sul.ModuleQuantity.Should().BeGreaterThan(norte.ModuleQuantity);
    }

    [Fact]
    public void Uf_com_menos_irradiacao_exige_mais_modulos()
    {
        var ceara = DimensioningCalculator.Calculate(Input(irradiation: 6.12m));
        var riograndedosul = DimensioningCalculator.Calculate(Input(irradiation: 4.52m));

        riograndedosul.ModuleQuantity.Should().BeGreaterThan(ceara.ModuleQuantity);
    }

    [Fact]
    public void Consumo_maior_exige_sistema_maior()
    {
        var pequeno = DimensioningCalculator.Calculate(Input(consumption: 300));
        var grande = DimensioningCalculator.Calculate(Input(consumption: 1500));

        grande.RequiredPowerKwp.Should().BeGreaterThan(pequeno.RequiredPowerKwp);
        grande.InverterPowerKw.Should().BeGreaterThanOrEqualTo(pequeno.InverterPowerKw);
    }

    [Fact]
    public void Area_de_telhado_acompanha_a_quantidade_de_modulos()
    {
        var result = DimensioningCalculator.Calculate(Input());

        // 8 módulos de 550 W a ~240 W/m² = 18.33 m²
        result.RoofAreaM2.Should().Be(18.33m);
    }

    [Fact]
    public void Calculo_automatico_nao_e_marcado_como_manual()
    {
        DimensioningCalculator.Calculate(Input()).IsManual.Should().BeFalse();
    }

    // --- US-022: ajuste manual ---

    [Fact]
    public void Ajuste_manual_por_quantidade_de_modulos_recalcula_geracao()
    {
        var automatico = DimensioningCalculator.Calculate(Input());
        var manual = DimensioningCalculator.Calculate(Input(manualQuantity: 12));

        manual.IsManual.Should().BeTrue();
        manual.ModuleQuantity.Should().Be(12);
        manual.InstalledPowerKwp.Should().Be(6.6m);
        manual.GenerationMonthlyKwh.Should().BeGreaterThan(automatico.GenerationMonthlyKwh);
    }

    [Fact]
    public void Ajuste_manual_por_potencia_converte_para_modulos()
    {
        // 6 kWp / 550 W = 10.9 -> 11 módulos
        var manual = DimensioningCalculator.Calculate(Input(manualPower: 6m));

        manual.IsManual.Should().BeTrue();
        manual.ModuleQuantity.Should().Be(11);
    }

    [Fact]
    public void Ajuste_manual_nao_altera_a_potencia_necessaria_calculada()
    {
        var manual = DimensioningCalculator.Calculate(Input(manualQuantity: 20));

        // A referência do cálculo continua visível para o vendedor comparar.
        manual.RequiredPowerKwp.Should().Be(4.32m);
    }

    [Fact]
    public void Modulo_de_outra_potencia_muda_a_quantidade()
    {
        var w550 = DimensioningCalculator.Calculate(Input(modulePowerW: 550));
        var w450 = DimensioningCalculator.Calculate(Input(modulePowerW: 450));

        w450.ModuleQuantity.Should().BeGreaterThan(w550.ModuleQuantity);
    }

    // --- Entradas inválidas ---

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Recusa_consumo_nao_positivo(int consumption)
    {
        var act = () => DimensioningCalculator.Calculate(Input(consumption: consumption));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Recusa_irradiacao_nao_positiva()
    {
        var act = () => DimensioningCalculator.Calculate(Input(irradiation: 0));

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0.4)]
    [InlineData(1.5)]
    public void Recusa_fator_de_perda_fora_da_faixa(double lossFactor)
    {
        var act = () => DimensioningCalculator.Calculate(Input(lossFactor: (decimal)lossFactor));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Recusa_quantidade_manual_nao_positiva()
    {
        var act = () => DimensioningCalculator.Calculate(Input(manualQuantity: 0));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Consumo_minimo_ainda_gera_ao_menos_um_modulo()
    {
        var result = DimensioningCalculator.Calculate(Input(consumption: 1));

        result.ModuleQuantity.Should().Be(1);
    }
}

public class StateGroupsTests
{
    [Theory]
    [InlineData("SP", StateGroup.Southeast)]
    [InlineData("rj", StateGroup.Southeast)]
    [InlineData("CE", StateGroup.Northeast)]
    [InlineData("RS", StateGroup.South)]
    [InlineData("DF", StateGroup.CenterWest)]
    [InlineData("AM", StateGroup.North)]
    public void Mapeia_uf_para_regiao(string uf, StateGroup esperado)
    {
        StateGroups.ForUf(uf).Should().Be(esperado);
    }

    [Theory]
    [InlineData("ZZ")]
    [InlineData("")]
    [InlineData(null)]
    public void Retorna_nulo_para_uf_desconhecida(string? uf)
    {
        StateGroups.ForUf(uf).Should().BeNull();
    }

    [Fact]
    public void Cobre_todas_as_27_ufs()
    {
        BrazilianStates.All.Should().OnlyContain(uf => StateGroups.ForUf(uf) != null);
    }
}
