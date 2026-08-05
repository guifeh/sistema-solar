using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.Application.Dimensioning.Commands;
using SolarSystem.Application.Dimensioning.Queries;

namespace SolarSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dimensionamento")]
public class DimensionamentoController : ControllerBase
{
    private readonly IMediator _mediator;

    public DimensionamentoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>US-020 — estimativa de consumo por perfil de imóvel.</summary>
    [HttpGet("estimativa-consumo")]
    public async Task<IActionResult> GetConsumptionEstimate(
        [FromQuery] string propertyType,
        [FromQuery] int? numRooms = null,
        [FromQuery] bool hasAc = false,
        [FromQuery] bool hasWaterHeater = false,
        [FromQuery] bool hasPool = false,
        [FromQuery] string? uf = null,
        [FromQuery] string? stateGroup = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetConsumptionEstimateQuery(propertyType, numRooms, hasAc, hasWaterHeater, hasPool, uf, stateGroup), ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>US-021 e US-022 — dimensiona por consumo, com ajuste manual opcional.</summary>
    [HttpPost("calcular")]
    public async Task<IActionResult> Calculate(
        [FromBody] CalculateDimensioningRequest request, CancellationToken ct = default)
    {
        var command = new CalculateDimensioningCommand(
            request.ConsumptionKwhMonth,
            request.Uf,
            request.LossFactor,
            request.RoofOrientation,
            request.ModulePowerW,
            request.ManualModuleQuantity,
            request.ManualPowerKwp);

        var result = await _mediator.Send(command, ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>Irradiação média de uma UF (base do cálculo).</summary>
    [HttpGet("irradiacao/{uf}")]
    public async Task<IActionResult> GetIrradiation(string uf, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetIrradiationQuery(uf), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>Irradiação de todas as UFs — alimenta o dropdown da tela.</summary>
    [HttpGet("irradiacao")]
    public async Task<IActionResult> ListIrradiations(CancellationToken ct = default)
        => Ok(await _mediator.Send(new ListIrradiationsQuery(), ct));
}

public class CalculateDimensioningRequest
{
    public int ConsumptionKwhMonth { get; set; }
    public string Uf { get; set; } = string.Empty;
    public decimal? LossFactor { get; set; }
    public string? RoofOrientation { get; set; }
    public int? ModulePowerW { get; set; }
    public int? ManualModuleQuantity { get; set; }
    public decimal? ManualPowerKwp { get; set; }
}
