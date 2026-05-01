using SolarSystem.Domain.Common;

namespace SolarSystem.Domain.Leads;

public class Lead : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? City { get; private set; }
    public string? Uf { get; private set; }
    public LeadType LeadType { get; private set; } = LeadType.Residential;
    public LeadSource LeadSource { get; private set; } = LeadSource.Referral;
    public LeadStatus Status { get; private set; } = LeadStatus.New;
    public string? Notes { get; private set; }
    public int? ConsumptionEstimate { get; private set; }

    private Lead() { }

    public static Lead Create(
        Guid tenantId,
        Guid createdBy,
        string name,
        string phone,
        string? email = null,
        string? city = null,
        string? uf = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Telefone é obrigatório.");
        if (!string.IsNullOrWhiteSpace(uf) && uf.Length != 2)
            throw new DomainException("UF deve ter 2 caracteres.");

        return new Lead
        {
            TenantId = tenantId,
            CreatedBy = createdBy,
            Name = name,
            Phone = phone,
            Email = email,
            City = city,
            Uf = uf?.ToUpper()
        };
    }

    public void Update(
        string? name = null,
        string? phone = null,
        string? email = null,
        string? city = null,
        string? uf = null,
        LeadType? leadType = null,
        LeadSource? leadSource = null,
        int? consumptionEstimate = null)
    {
        if (name != null) Name = name;
        if (phone != null) Phone = phone;
        if (email != null) Email = email;
        if (city != null) City = city;
        if (uf != null)
        {
            if (uf.Length != 2)
                throw new DomainException("UF deve ter 2 caracteres.");
            Uf = uf.ToUpper();
        }
        if (leadType.HasValue) LeadType = leadType.Value;
        if (leadSource.HasValue) LeadSource = leadSource.Value;
        if (consumptionEstimate.HasValue) ConsumptionEstimate = consumptionEstimate.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNote(string note)
    {
        Notes = string.IsNullOrEmpty(Notes)
            ? note
            : $"{Notes}\n\n[{DateTime.UtcNow:dd/MM/yyyy HH:mm}] {note}";
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(LeadStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum LeadType
{
    Residential = 0,
    Commercial = 1,
    Industrial = 2
}

public enum LeadSource
{
    Referral = 0,
    Inbound = 1,
    Prospecting = 2
}

public enum LeadStatus
{
    New = 0,
    Contacting = 1,
    ProposalSent = 2,
    Won = 3,
    Lost = 4
}
