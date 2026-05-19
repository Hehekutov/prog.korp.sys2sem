using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prac5.Data;
using Prac5.Models;
using Prac5.Services;

namespace Prac5.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersApiController : ControllerBase
{
    private readonly ProductionDbContext _context;
    private readonly ProductionWorkflowService _workflow;

    public OrdersApiController(ProductionDbContext context, ProductionWorkflowService workflow)
    {
        _context = context;
        _workflow = workflow;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status = null, [FromQuery] string? date = null)
    {
        var query = _context.WorkOrders
            .AsNoTracking()
            .Include(item => item.Product)
            .Include(item => item.ProductionLine)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = status.Equals("active", StringComparison.OrdinalIgnoreCase)
                ? query.Where(item => item.Status == WorkOrderStatus.Pending || item.Status == WorkOrderStatus.InProgress)
                : query.Where(item => item.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(date))
        {
            var targetDate = date.Equals("today", StringComparison.OrdinalIgnoreCase)
                ? DateTime.Today
                : DateTime.TryParse(date, out var parsedDate) ? parsedDate.Date : (DateTime?)null;

            if (targetDate is not null)
            {
                query = query.Where(item => item.StartDate.Date == targetDate.Value.Date);
            }
        }

        var orders = await query
            .OrderByDescending(item => item.StartDate)
            .ToListAsync();

        return Ok(orders.Select(ToOrderDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request)
    {
        var result = await _workflow.CreateOrderAsync(request.ProductId, request.Quantity, request.LineId, request.StartDate);
        if (!result.Success || result.Order is null)
        {
            return BadRequest(new { result.Message, result.Shortages });
        }

        var order = await LoadOrder(result.Order.Id);
        return Created($"/api/orders/{result.Order.Id}/details", ToOrderDto(order!));
    }

    [HttpPut("{id:int}/progress")]
    public async Task<IActionResult> UpdateProgress(int id, ProgressRequest request)
    {
        var result = await _workflow.UpdateProgressAsync(id, request.Percent);
        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        var order = await LoadOrder(id);
        return Ok(ToOrderDto(order!));
    }

    [HttpGet("{id:int}/details")]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.WorkOrders
            .AsNoTracking()
            .Include(item => item.Product)
            .ThenInclude(item => item!.ProductMaterials)
            .ThenInclude(item => item.Material)
            .Include(item => item.ProductionLine)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            Order = ToOrderDto(order),
            Materials = order.Product?.ProductMaterials.Select(item => new
            {
                item.MaterialId,
                Name = item.Material?.Name,
                Required = item.QuantityNeeded * order.Quantity,
                Unit = item.Material?.UnitOfMeasure
            })
        });
    }

    private async Task<WorkOrder?> LoadOrder(int id)
    {
        return await _context.WorkOrders
            .AsNoTracking()
            .Include(item => item.Product)
            .Include(item => item.ProductionLine)
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    private static object ToOrderDto(WorkOrder order)
    {
        return new
        {
            order.Id,
            ProductId = order.ProductId,
            Product = order.Product?.Name,
            ProductionLineId = order.ProductionLineId,
            ProductionLine = order.ProductionLine?.Name,
            order.Quantity,
            order.StartDate,
            order.EstimatedEndDate,
            order.Status,
            order.ProgressPercent
        };
    }
}

public sealed class CreateOrderRequest
{
    [JsonPropertyName("product_id")]
    public int ProductId { get; init; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; init; }

    [JsonPropertyName("line_id")]
    public int? LineId { get; init; }

    [JsonPropertyName("start_date")]
    public DateTime? StartDate { get; init; }
}

public sealed class ProgressRequest
{
    [JsonPropertyName("percent")]
    public int Percent { get; init; }
}
