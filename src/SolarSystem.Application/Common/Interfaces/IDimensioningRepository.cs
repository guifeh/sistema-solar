using SolarSystem.Domain.Dimensioning;

namespace SolarSystem.Application.Common.Interfaces;

public interface IIrradiationRepository
{
    Task<IrradiationByUf?> GetByUfAsync(string uf, CancellationToken ct = default);
    Task<IReadOnlyList<IrradiationByUf>> GetAllAsync(CancellationToken ct = default);
}

public interface IConsumptionProfileRepository
{
    /// <summary>
    /// Perfis cadastrados para o tipo de imovel na regiao. A escolha do melhor perfil fica
    /// no handler, para a regra de aproximacao ficar visivel e testavel.
    /// </summary>
    Task<IReadOnlyList<ConsumptionProfile>> GetCandidatesAsync(
        PropertyType propertyType, StateGroup stateGroup, CancellationToken ct = default);
}
