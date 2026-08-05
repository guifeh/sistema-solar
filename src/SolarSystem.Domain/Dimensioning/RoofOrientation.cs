namespace SolarSystem.Domain.Dimensioning;

/// <summary>
/// Orientacao do telhado. No hemisferio sul a face norte recebe o melhor angulo de
/// incidencia ao longo do ano; as demais perdem geracao proporcionalmente.
/// </summary>
public enum RoofOrientation
{
    North = 0,
    NorthEast = 1,
    NorthWest = 2,
    East = 3,
    West = 4,
    South = 5,
    Flat = 6
}

public static class RoofOrientationFactors
{
    /// <summary>
    /// Fator de aproveitamento por orientacao, aplicado sobre a irradiacao da UF.
    /// </summary>
    public static decimal FactorFor(RoofOrientation orientation) => orientation switch
    {
        RoofOrientation.North => 1.00m,
        RoofOrientation.NorthEast => 0.95m,
        RoofOrientation.NorthWest => 0.95m,
        RoofOrientation.East => 0.85m,
        RoofOrientation.West => 0.85m,
        RoofOrientation.South => 0.75m,
        RoofOrientation.Flat => 0.90m,
        _ => 0.80m
    };
}
