namespace SolarSystem.Domain.Dimensioning;

/// <summary>
/// Irradiacao global horizontal media diaria por UF, em kWh/m²/dia. Dataset estatico
/// (ADR-004), derivado da API NASA POWER — CC BY 4.0, que permite uso comercial.
/// Ver a migration AddDimensioningReferenceData para o metodo e o porque de nao usar o
/// Atlas do LABREN/INPE.
/// </summary>
public class IrradiationByUf
{
    public string Uf { get; private set; } = string.Empty;
    public string StateName { get; private set; } = string.Empty;
    public decimal AverageIrradiation { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }

    private IrradiationByUf() { }

    public static IrradiationByUf Create(
        string uf,
        string stateName,
        decimal averageIrradiation,
        string source = "NASA POWER (CERES/MERRA-2)")
    {
        if (string.IsNullOrWhiteSpace(uf) || uf.Length != 2)
            throw new Common.DomainException("UF deve ter 2 caracteres.");
        if (string.IsNullOrWhiteSpace(stateName))
            throw new Common.DomainException("Nome do estado é obrigatório.");
        if (averageIrradiation <= 0)
            throw new Common.DomainException("Irradiação deve ser positiva.");

        return new IrradiationByUf
        {
            Uf = uf.ToUpper(),
            StateName = stateName,
            AverageIrradiation = averageIrradiation,
            Source = source,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
