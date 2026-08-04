using FluentAssertions;
using SolarSystem.Domain.Common;
using SolarSystem.Domain.Leads;

namespace SolarSystem.Tests.Domain;

public class LeadTests
{
    private static Lead NovoLead(string name = "Cliente", string phone = "11999990000", string? uf = null)
        => Lead.Create(Guid.NewGuid(), Guid.NewGuid(), name, phone, uf: uf);

    [Fact]
    public void Create_nasce_com_status_novo()
    {
        NovoLead().Status.Should().Be(LeadStatus.New);
    }

    [Fact]
    public void Create_normaliza_uf_para_maiuscula()
    {
        NovoLead(uf: "sp").Uf.Should().Be("SP");
    }

    [Theory]
    [InlineData("", "11999990000")]
    [InlineData("   ", "11999990000")]
    [InlineData("Cliente", "")]
    public void Create_recusa_nome_ou_telefone_em_branco(string name, string phone)
    {
        var act = () => Lead.Create(Guid.NewGuid(), Guid.NewGuid(), name, phone);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeStatus_troca_o_status_e_marca_atualizacao()
    {
        var lead = NovoLead();

        lead.ChangeStatus(LeadStatus.Won);

        lead.Status.Should().Be(LeadStatus.Won);
        lead.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddNote_preserva_a_nota_anterior()
    {
        var lead = NovoLead();

        lead.AddNote("Primeiro contato");
        lead.AddNote("Cliente pediu retorno");

        lead.Notes.Should().Contain("Primeiro contato");
        lead.Notes.Should().Contain("Cliente pediu retorno");
    }

    [Fact]
    public void Update_so_altera_os_campos_informados()
    {
        var lead = NovoLead(name: "Nome original", phone: "11999990000");

        lead.Update(name: "Nome novo");

        lead.Name.Should().Be("Nome novo");
        lead.Phone.Should().Be("11999990000");
    }
}
