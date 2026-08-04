namespace SolarSystem.Domain.Common;

/// <summary>
/// Lista fechada de UFs. Usada na validacao de leads e, no EP-03, para casar
/// com a tabela de irradiacao por UF — que so tem linha para estes 27 codigos.
/// </summary>
public static class BrazilianStates
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
        "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
        "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public static bool IsValid(string? uf) => !string.IsNullOrWhiteSpace(uf) && All.Contains(uf);
}
