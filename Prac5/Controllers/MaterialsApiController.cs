using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prac5.Data;
using Prac5.Models;

namespace Prac5.Controllers;

[ApiController]
[Route("api/materials")]
public class MaterialsApiController : ControllerBase
{
    private readonly ProductionDbContext _context;

    public MaterialsApiController(ProductionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery(Name = "low_stock")] bool lowStock = false)
    {
        var query = _context.Materials.AsNoTracking();
        if (lowStock)
        {
            query = query.Where(item => item.Quantity <= item.MinimalStock);
        }

        var materials = await query
            .OrderBy(item => item.Name)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.Quantity,
                Unit = item.UnitOfMeasure,
                MinStock = item.MinimalStock,
                LowStock = item.Quantity <= item.MinimalStock
            })
            .ToListAsync();

        return Ok(materials);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMaterialRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Unit) || request.Quantity < 0)
        {
            return BadRequest("Некорректные данные материала.");
        }

        var material = new Material
        {
            Name = request.Name.Trim(),
            Quantity = request.Quantity,
            UnitOfMeasure = request.Unit.Trim(),
            MinimalStock = Math.Max(0, request.MinStock)
        };

        _context.Materials.Add(material);
        await _context.SaveChangesAsync();

        return Created($"/api/materials/{material.Id}", material);
    }

    [HttpPut("{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(int id, StockUpdateRequest request)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material is null)
        {
            return NotFound();
        }

        material.Quantity = Math.Max(0, material.Quantity + request.Amount);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            material.Id,
            material.Name,
            material.Quantity,
            Unit = material.UnitOfMeasure,
            MinStock = material.MinimalStock,
            LowStock = material.Quantity <= material.MinimalStock
        });
    }
}

public sealed class CreateMaterialRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; init; }

    [JsonPropertyName("unit")]
    public string Unit { get; init; } = string.Empty;

    [JsonPropertyName("min_stock")]
    public decimal MinStock { get; init; }
}

public sealed class StockUpdateRequest
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }
}
