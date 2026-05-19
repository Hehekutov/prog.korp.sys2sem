using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prac5.Data;
using Prac5.Models;

namespace Prac5.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly ProductionDbContext _context;

    public ProductsApiController(ProductionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? category = null)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(item => item.ProductMaterials)
            .ThenInclude(item => item.Material)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(item => item.Category == category);
        }

        var products = await query
            .OrderBy(item => item.Name)
            .ToListAsync();

        return Ok(products.Select(ToProductDto));
    }

    [HttpGet("{id:int}/materials")]
    public async Task<IActionResult> GetMaterials(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(item => item.ProductMaterials)
            .ThenInclude(item => item.Material)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product.ProductMaterials.Select(item => new
        {
            item.MaterialId,
            Name = item.Material?.Name,
            item.QuantityNeeded,
            Unit = item.Material?.UnitOfMeasure
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Category) || request.ProductionTimePerUnit <= 0)
        {
            return BadRequest("Некорректные данные продукта.");
        }

        var materialIds = request.Materials
            .Where(item => item.MaterialId > 0 && item.QuantityNeeded > 0)
            .Select(item => item.MaterialId)
            .Distinct()
            .ToList();

        var existingMaterialIds = await _context.Materials
            .Where(item => materialIds.Contains(item.Id))
            .Select(item => item.Id)
            .ToListAsync();

        if (materialIds.Any(item => !existingMaterialIds.Contains(item)))
        {
            return BadRequest("Один или несколько материалов не найдены.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Specifications = string.IsNullOrWhiteSpace(request.Specifications) ? "{}" : request.Specifications.Trim(),
            Category = request.Category.Trim(),
            MinimalStock = Math.Max(0, request.MinimalStock),
            ProductionTimePerUnit = request.ProductionTimePerUnit
        };

        foreach (var item in request.Materials.Where(item => item.QuantityNeeded > 0))
        {
            product.ProductMaterials.Add(new ProductMaterial
            {
                MaterialId = item.MaterialId,
                QuantityNeeded = item.QuantityNeeded
            });
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return Created($"/api/products/{product.Id}", new
        {
            product.Id,
            product.Name,
            product.Category,
            product.ProductionTimePerUnit
        });
    }

    private static object ToProductDto(Product product)
    {
        return new
        {
            product.Id,
            product.Name,
            product.Description,
            product.Specifications,
            product.Category,
            product.MinimalStock,
            product.ProductionTimePerUnit,
            Materials = product.ProductMaterials.Select(item => new
            {
                item.MaterialId,
                Name = item.Material?.Name,
                item.QuantityNeeded,
                Unit = item.Material?.UnitOfMeasure
            })
        };
    }
}

public sealed class CreateProductRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("specifications")]
    public string? Specifications { get; init; }

    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("minimal_stock")]
    public int MinimalStock { get; init; }

    [JsonPropertyName("prod_time")]
    public int ProductionTimePerUnit { get; init; }

    [JsonPropertyName("materials")]
    public List<ProductMaterialRequest> Materials { get; init; } = new();
}

public sealed class ProductMaterialRequest
{
    [JsonPropertyName("material_id")]
    public int MaterialId { get; init; }

    [JsonPropertyName("quantity_needed")]
    public decimal QuantityNeeded { get; init; }
}
