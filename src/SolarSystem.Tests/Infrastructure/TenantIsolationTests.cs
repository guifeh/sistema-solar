using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Leads;
using SolarSystem.Infrastructure.Persistence;

namespace SolarSystem.Tests.Infrastructure;

/// <summary>
/// US-072 — isolamento multi-tenant. Estes testes existem porque o filtro global ja esteve
/// inativo em producao de desenvolvimento: o contexto era construido sem tenant e qualquer
/// empresa lia o lead de outra pelo id.
/// </summary>
public class TenantIsolationTests : IDisposable
{
    private sealed class FakeCurrentUser : ICurrentUserService
    {
        public Guid UserId { get; init; } = Guid.NewGuid();
        public Guid TenantId { get; init; }
        public string Role => "admin";
        public bool IsAuthenticated => TenantId != Guid.Empty;
    }

    private readonly Guid _tenantA = Guid.NewGuid();
    private readonly Guid _tenantB = Guid.NewGuid();
    private readonly string _databaseName = Guid.NewGuid().ToString();

    private SolarDbContext ContextFor(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<SolarDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new SolarDbContext(options, new FakeCurrentUser { TenantId = tenantId });
    }

    private async Task<Guid> SeedLeadAsync(Guid tenantId, string name)
    {
        await using var context = ContextFor(tenantId);
        var lead = Lead.Create(tenantId, Guid.NewGuid(), name, "11999990000", uf: "SP");
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        return lead.Id;
    }

    [Fact]
    public async Task Listagem_so_devolve_leads_do_proprio_tenant()
    {
        await SeedLeadAsync(_tenantA, "Lead da A");
        await SeedLeadAsync(_tenantB, "Lead da B 1");
        await SeedLeadAsync(_tenantB, "Lead da B 2");

        await using var contextA = ContextFor(_tenantA);
        var leadsDeA = await contextA.Leads.ToListAsync();

        leadsDeA.Should().HaveCount(1);
        leadsDeA.Single().Name.Should().Be("Lead da A");
    }

    [Fact]
    public async Task Busca_por_id_nao_alcanca_lead_de_outro_tenant()
    {
        var leadDeA = await SeedLeadAsync(_tenantA, "Lead da A");

        await using var contextB = ContextFor(_tenantB);
        var encontrado = await contextB.Leads.FirstOrDefaultAsync(l => l.Id == leadDeA);

        encontrado.Should().BeNull("o filtro global precisa valer tambem para busca direta por id");
    }

    [Fact]
    public async Task Contexto_sem_usuario_autenticado_nao_ve_nada()
    {
        await SeedLeadAsync(_tenantA, "Lead da A");
        await SeedLeadAsync(_tenantB, "Lead da B");

        await using var contextAnonimo = ContextFor(Guid.Empty);
        var leads = await contextAnonimo.Leads.ToListAsync();

        leads.Should().BeEmpty("sem tenant no contexto o padrao tem que ser nao enxergar nada");
    }

    [Fact]
    public async Task Cada_tenant_enxerga_o_proprio_conjunto_de_leads()
    {
        await SeedLeadAsync(_tenantA, "Lead da A");
        await SeedLeadAsync(_tenantB, "Lead da B");

        await using var contextA = ContextFor(_tenantA);
        await using var contextB = ContextFor(_tenantB);

        // Mesmo banco, dois contextos vivos ao mesmo tempo: confirma que o tenant nao ficou
        // congelado no modelo cacheado pelo EF.
        (await contextA.Leads.SingleAsync()).Name.Should().Be("Lead da A");
        (await contextB.Leads.SingleAsync()).Name.Should().Be("Lead da B");
    }

    [Fact]
    public async Task Entidade_salva_sem_tenant_herda_o_tenant_do_contexto()
    {
        await using var context = ContextFor(_tenantA);
        var lead = Lead.Create(Guid.Empty, Guid.NewGuid(), "Sem tenant", "11999990000");

        context.Leads.Add(lead);
        await context.SaveChangesAsync();

        lead.TenantId.Should().Be(_tenantA);
    }

    public void Dispose()
    {
        using var context = ContextFor(Guid.Empty);
        context.Database.EnsureDeleted();
    }
}
