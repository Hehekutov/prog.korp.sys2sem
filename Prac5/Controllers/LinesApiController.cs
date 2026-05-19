using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prac5.Data;
using Prac5.Models;
using Prac5.Services;

namespace Prac5.Controllers;

[ApiController]
[Route("api/lines")]
public class LinesApiController : ControllerBase
{
    private readonly ProductionDbContext _context;
    private readonly ProductionWorkflowService _workflow;

    public LinesApiController(ProductionDbContext context, ProductionWorkflowService workflow)
    {
        _context = context;
        _workflow = workflow;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool available = false)
    {
        var query = _context.ProductionLines
            .AsNoTracking()
            .Include(item => item.WorkOrders)
            .ThenInclude(item => item.Product)
            .AsQueryable();

        if (available)
        {
            query = query.Where(item => item.Status == ProductionLineStatus.Active && item.CurrentWorkOrderId == null);
        }

        var lines = await query.OrderBy(item => item.Name).ToListAsync();
        return Ok(lines.Select(ToLineDto));
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, LineStatusRequest request)
    {
        var result = await _workflow.UpdateLineStatusAsync(id, request.Status);
        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        return Ok(new { result.Message });
    }

    [HttpGet("{id:int}/schedule")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        var exists = await _context.ProductionLines.AnyAsync(item => item.Id == id);
        if (!exists)
        {
            return NotFound();
        }

        var orders = await _context.WorkOrders
            .AsNoTracking()
            .Include(item => item.Product)
            .Where(item => item.ProductionLineId == id)
            .OrderBy(item => item.StartDate)
            .Select(item => new
            {
                item.Id,
                Product = item.Product == null ? null : item.Product.Name,
                item.Quantity,
                item.Status,
                item.ProgressPercent,
                item.StartDate,
                item.EstimatedEndDate
            })
            .ToListAsync();

        return Ok(orders);
    }

    private static object ToLineDto(ProductionLine line)
    {
        var currentOrder = line.WorkOrders.FirstOrDefault(item => item.Id == line.CurrentWorkOrderId);
        return new
        {
            line.Id,
            line.Name,
            line.Status,
            line.EfficiencyFactor,
            line.CurrentWorkOrderId,
            CurrentProduct = currentOrder?.Product?.Name,
            CurrentProgress = currentOrder?.ProgressPercent ?? 0
        };
    }
}

public sealed class LineStatusRequest
{
    [JsonPropertyName("status")]
    public string Status { get; init; } = ProductionLineStatus.Active;
}
