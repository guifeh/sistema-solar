namespace SolarSystem.Domain.Dimensioning;

public class IrradiationByUf
{
    public string Uf { get; private set; } = string.Empty;
    public decimal AverageIrradiation { get; private set; }
    public string Source { get; private set; } = string.Empty;

    private IrradiationByUf() { }

    public static IrradiationByUf Create(string uf, decimal averageIrradiation, string source = "CRESESB/INMET")
    {
        if (string.IsNullOrWhiteSpace(uf) || uf.Length != 2)
            throw new Common.DomainException("UF deve ter 2 caracteres.");
        if (averageIrradiation <= 0)
            throw new Common.DomainException("Irradiação deve ser positiva.");

        return new IrradiationByUf
        {
            Uf = uf.ToUpper(),
            AverageIrradiation = averageIrradiation,
            Source = source
        };
    }
}
