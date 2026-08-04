using FluentAssertions;
using SolarSystem.Infrastructure.Services;

namespace SolarSystem.Tests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_gera_hash_diferente_para_a_mesma_senha()
    {
        var a = _hasher.HashPassword("Senha1234");
        var b = _hasher.HashPassword("Senha1234");

        // Salt por usuario: dois cadastros com a mesma senha nao compartilham hash,
        // que e o que inviabiliza rainbow table.
        a.Should().NotBe(b);
    }

    [Fact]
    public void HashPassword_nao_guarda_a_senha_em_claro()
    {
        var hash = _hasher.HashPassword("Senha1234");

        hash.Should().NotContain("Senha1234");
        hash.Should().StartWith("pbkdf2$sha256$");
    }

    [Fact]
    public void VerifyPassword_aceita_a_senha_correta()
    {
        var hash = _hasher.HashPassword("Senha1234");

        _hasher.VerifyPassword("Senha1234", hash).Should().BeTrue();
    }

    [Theory]
    [InlineData("senha1234")]
    [InlineData("Senha123")]
    [InlineData("")]
    public void VerifyPassword_rejeita_senha_errada(string tentativa)
    {
        var hash = _hasher.HashPassword("Senha1234");

        _hasher.VerifyPassword(tentativa, hash).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-um-hash")]
    [InlineData("pbkdf2$sha256$sem-iteracoes$salt$hash")]
    [InlineData("a4ayc/80/OGda4BO/1o/V0etpOqiLx1JwB5S3beHW0s=")] // SHA256 puro do formato antigo
    public void VerifyPassword_rejeita_hash_em_formato_invalido(string hash)
    {
        _hasher.VerifyPassword("Senha1234", hash).Should().BeFalse();
    }

    [Fact]
    public void HashPassword_recusa_senha_vazia()
    {
        var act = () => _hasher.HashPassword("");

        act.Should().Throw<ArgumentException>();
    }
}
