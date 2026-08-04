using FluentAssertions;
using SolarSystem.Application.Auth.Commands;
using SolarSystem.Application.Leads.Commands;

namespace SolarSystem.Tests.Application;

public class CreateLeadCommandValidatorTests
{
    private readonly CreateLeadCommandValidator _validator = new();

    private static CreateLeadCommand Comando(string name = "Cliente", string phone = "11999990000",
        string? email = null, string? uf = null)
        => new(name, phone, email, null, uf, null, null, null, null);

    [Fact]
    public void Aceita_lead_minimo_valido()
    {
        _validator.Validate(Comando()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "11999990000")]
    [InlineData("Cliente", "")]
    public void Exige_nome_e_telefone(string name, string phone)
    {
        _validator.Validate(Comando(name, phone)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("ZZ")]   // 2 letras, mas nao e UF
    [InlineData("XXX")]
    [InlineData("S")]
    public void Rejeita_uf_invalida(string uf)
    {
        var resultado = _validator.Validate(Comando(uf: uf));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == nameof(CreateLeadCommand.Uf));
    }

    [Theory]
    [InlineData("SP")]
    [InlineData("mg")]
    public void Aceita_uf_valida(string uf)
    {
        _validator.Validate(Comando(uf: uf)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Rejeita_email_malformado()
    {
        _validator.Validate(Comando(email: "nao-e-email")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Aceita_lead_sem_email()
    {
        _validator.Validate(Comando(email: null)).IsValid.Should().BeTrue();
    }
}

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    private static RegisterCommand Comando(string password = "Senha1234", string email = "a@b.com")
        => new("Empresa", email, password, "Admin");

    [Fact]
    public void Aceita_cadastro_valido()
    {
        _validator.Validate(Comando()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]            // vazia
    [InlineData("1")]           // curta demais
    [InlineData("Senha")]       // sem numero
    [InlineData("12345678")]    // sem letra
    [InlineData("Abc123")]      // 6 caracteres
    public void Rejeita_senha_fora_da_politica(string password)
    {
        var resultado = _validator.Validate(Comando(password));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void Rejeita_email_malformado()
    {
        _validator.Validate(Comando(email: "sem-arroba")).IsValid.Should().BeFalse();
    }
}
