using SolarSystem.Domain.Common;

namespace SolarSystem.Domain.Dimensioning;

public enum PropertyType
{
    Apartment = 0,
    House = 1,
    Commercial = 2
}

/// <summary>
/// Agrupamento regional usado pelos perfis de consumo. Regioes diferentes tem padrao de
/// consumo distinto — sobretudo por climatizacao.
/// </summary>
public enum StateGroup
{
    North = 0,
    Northeast = 1,
    CenterWest = 2,
    Southeast = 3,
    South = 4
}

/// <summary>
/// Faixa de consumo estimada para um perfil de imovel. Tabela global (nao multi-tenant):
/// e dado de referencia, igual para todos os integradores.
/// </summary>
public class ConsumptionProfile : Entity
{
    public PropertyType PropertyType { get; private set; }
    public int? NumRooms { get; private set; }
    public bool HasAc { get; private set; }
    public bool HasWaterHeater { get; private set; }
    public bool HasPool { get; private set; }
    public StateGroup StateGroup { get; private set; }
    public int ConsumptionMin { get; private set; }
    public int ConsumptionMax { get; private set; }
    public int ConsumptionAvg { get; private set; }

    private ConsumptionProfile() { }

    public static ConsumptionProfile Create(
        PropertyType propertyType,
        int? numRooms,
        bool hasAc,
        bool hasWaterHeater,
        bool hasPool,
        StateGroup stateGroup,
        int consumptionMin,
        int consumptionMax,
        int consumptionAvg)
    {
        if (consumptionMin <= 0)
            throw new DomainException("Consumo mínimo deve ser positivo.");
        if (consumptionMax < consumptionMin)
            throw new DomainException("Consumo máximo não pode ser menor que o mínimo.");
        if (consumptionAvg < consumptionMin || consumptionAvg > consumptionMax)
            throw new DomainException("Consumo médio deve estar entre o mínimo e o máximo.");

        return new ConsumptionProfile
        {
            PropertyType = propertyType,
            NumRooms = numRooms,
            HasAc = hasAc,
            HasWaterHeater = hasWaterHeater,
            HasPool = hasPool,
            StateGroup = stateGroup,
            ConsumptionMin = consumptionMin,
            ConsumptionMax = consumptionMax,
            ConsumptionAvg = consumptionAvg
        };
    }
}
