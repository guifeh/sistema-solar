namespace SolarSystem.Domain.Dimensioning;

/// <summary>
/// Mapa UF → regiao. Permite ao vendedor informar so a UF do lead e ainda assim cair no
/// perfil de consumo da regiao certa.
/// </summary>
public static class StateGroups
{
    private static readonly IReadOnlyDictionary<string, StateGroup> Map =
        new Dictionary<string, StateGroup>(StringComparer.OrdinalIgnoreCase)
        {
            ["AC"] = StateGroup.North,
            ["AP"] = StateGroup.North,
            ["AM"] = StateGroup.North,
            ["PA"] = StateGroup.North,
            ["RO"] = StateGroup.North,
            ["RR"] = StateGroup.North,
            ["TO"] = StateGroup.North,

            ["AL"] = StateGroup.Northeast,
            ["BA"] = StateGroup.Northeast,
            ["CE"] = StateGroup.Northeast,
            ["MA"] = StateGroup.Northeast,
            ["PB"] = StateGroup.Northeast,
            ["PE"] = StateGroup.Northeast,
            ["PI"] = StateGroup.Northeast,
            ["RN"] = StateGroup.Northeast,
            ["SE"] = StateGroup.Northeast,

            ["DF"] = StateGroup.CenterWest,
            ["GO"] = StateGroup.CenterWest,
            ["MT"] = StateGroup.CenterWest,
            ["MS"] = StateGroup.CenterWest,

            ["ES"] = StateGroup.Southeast,
            ["MG"] = StateGroup.Southeast,
            ["RJ"] = StateGroup.Southeast,
            ["SP"] = StateGroup.Southeast,

            ["PR"] = StateGroup.South,
            ["RS"] = StateGroup.South,
            ["SC"] = StateGroup.South
        };

    public static StateGroup? ForUf(string? uf)
        => !string.IsNullOrWhiteSpace(uf) && Map.TryGetValue(uf, out var group) ? group : null;
}
