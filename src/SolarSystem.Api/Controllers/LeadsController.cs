using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarSystem.Application.Leads.Commands;
using SolarSystem.Application.Leads.Queries;

namespace SolarSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeadsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? uf = null,
        [FromQuery] string? type = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetLeadsQuery(page, pageSize, status, uf, type, search), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetLeadByIdQuery(id), ct);
        if (result.IsFailure)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeadRequest request, CancellationToken ct = default)
    {
        var command = new CreateLeadCommand(
            request.Name,
            request.Phone,
            request.Email,
            request.City,
            request.Uf,
            request.LeadType,
            request.LeadSource,
            request.ConsumptionEstimate,
            request.Notes);

        var result = await _mediator.Send(command, ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadRequest request, CancellationToken ct = default)
    {
        var command = new UpdateLeadCommand(
            id,
            request.Name,
            request.Phone,
            request.Email,
            request.City,
            request.Uf,
            request.LeadType,
            request.LeadSource,
            request.ConsumptionEstimate);

        var result = await _mediator.Send(command, ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] AddNoteRequest request, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new AddLeadNoteCommand(id, request.Note), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ChangeLeadStatusCommand(id, request.Status), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }
}

public class CreateLeadRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Uf { get; set; }
    public string? LeadType { get; set; }
    public string? LeadSource { get; set; }
    public int? ConsumptionEstimate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLeadRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Uf { get; set; }
    public string? LeadType { get; set; }
    public string? LeadSource { get; set; }
    public int? ConsumptionEstimate { get; set; }
}

public record AddNoteRequest(string Note);
public record ChangeStatusRequest(string Status);
