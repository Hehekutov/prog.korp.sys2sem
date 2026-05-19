using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prac5.Data;
using Prac5.Models;
using Prac5.Services;

namespace Prac5.Controllers;

public class ProductionController : Controller
{
    private readonly ProductionDbContext _context;
    private readonly ProductionWorkflowService _workflow;

    public ProductionController(ProductionDbContext context, ProductionWorkflowService workflow)
    {
        _context = context;
        _workflow = workflow;
    }

    public async Task<IActionResult> Index(string? category, string? search)
    {
        var materials = await _context.Materials
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync();

        var allProducts = await _context.Products
            .AsNoTracking()
            .Include(item => item.ProductMaterials)
            .ThenInclude(item => item.Material)
            .OrderBy(item => item.Name)
            .ToListAsync();

        var products = allProducts.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(category))
        {
            products = products.Where(item => item.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            products = products.Where(item =>
                item.Name.Contains(search.Trim(), StringComparison.CurrentCultureIgnoreCase));
        }

        var orders = await _context.WorkOrders
            .AsNoTracking()
            .Include(item => item.Product)
            .Include(item => item.ProductionLine)
            .OrderByDescending(item => item.StartDate)
            .ToListAsync();

        var lines = await _context.ProductionLines
            .AsNoTracking()
            .Include(item => item.WorkOrders)
            .ThenInclude(item => item.Product)
            .OrderBy(item => item.Name)
            .ToListAsync();

        var model = new DashboardViewModel
        {
            Materials = materials,
            AllProducts = allProducts,
            Products = products.ToList(),
            WorkOrders = orders,
            Lines = lines,
            Categories = allProducts.Select(item => item.Category).Distinct().OrderBy(item => item).ToList(),
            CategoryFilter = category,
            Search = search
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMaterial(string name, decimal quantity, string unitOfMeasure, decimal minimalStock)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(unitOfMeasure) || quantity < 0 || minimalStock < 0)
        {
            TempData["Error"] = "Проверьте данные материала.";
            return RedirectToAction(nameof(Index));
        }

        _context.Materials.Add(new Material
        {
            Name = name.Trim(),
            Quantity = quantity,
            UnitOfMeasure = unitOfMeasure.Trim(),
            MinimalStock = minimalStock
        });

        await _context.SaveChangesAsync();
        TempData["Message"] = "Материал добавлен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefillMaterial(int id, decimal amount)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material is null || amount <= 0)
        {
            TempData["Error"] = "Не удалось пополнить материал.";
            return RedirectToAction(nameof(Index));
        }

        material.Quantity += amount;
        await _context.SaveChangesAsync();
        TempData["Message"] = "Остаток материала обновлен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(
        string name,
        string category,
        int productionTimePerUnit,
        int minimalStock,
        string? description,
        string? specifications,
        [FromForm] Dictionary<int, decimal> materialQuantities)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category) || productionTimePerUnit <= 0)
        {
            TempData["Error"] = "Проверьте данные продукта.";
            return RedirectToAction(nameof(Index));
        }

        var existingMaterialIds = await _context.Materials
            .Where(item => materialQuantities.Keys.Contains(item.Id))
            .Select(item => item.Id)
            .ToListAsync();

        var product = new Product
        {
            Name = name.Trim(),
            Category = category.Trim(),
            ProductionTimePerUnit = productionTimePerUnit,
            MinimalStock = Math.Max(0, minimalStock),
            Description = description?.Trim() ?? string.Empty,
            Specifications = string.IsNullOrWhiteSpace(specifications) ? "{}" : specifications.Trim()
        };

        foreach (var item in materialQuantities.Where(item => item.Value > 0 && existingMaterialIds.Contains(item.Key)))
        {
            product.ProductMaterials.Add(new ProductMaterial
            {
                MaterialId = item.Key,
                QuantityNeeded = item.Value
            });
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        TempData["Message"] = "Продукт добавлен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(int productId, int quantity, int? productionLineId, DateTime? startDate)
    {
        var result = await _workflow.CreateOrderAsync(productId, quantity, productionLineId, startDate);
        SetOperationMessage(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartOrder(int id)
    {
        var result = await _workflow.StartOrderAsync(id);
        SetOperationMessage(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var result = await _workflow.CancelOrderAsync(id);
        SetOperationMessage(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProgress(int id, int percent)
    {
        var result = await _workflow.UpdateProgressAsync(id, percent);
        SetOperationMessage(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveOrder(int id, DateTime startDate)
    {
        var result = await _workflow.RescheduleOrderAsync(id, startDate);
        SetOperationMessage(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLineStatus(int id, string status)
    {
        var result = await _workflow.UpdateLineStatusAsync(id, status);
        SetOperationMessage(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEfficiency(int id, float efficiencyFactor)
    {
        var result = await _workflow.UpdateLineEfficiencyAsync(id, efficiencyFactor);
        SetOperationMessage(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartLine(int lineId, int productId, int quantity)
    {
        var createResult = await _workflow.CreateOrderAsync(productId, quantity, lineId, DateTime.Now);
        if (!createResult.Success || createResult.Order is null)
        {
            SetOperationMessage(createResult);
            return RedirectToAction(nameof(Index));
        }

        var startResult = await _workflow.StartOrderAsync(createResult.Order.Id);
        SetOperationMessage(startResult);
        return RedirectToAction(nameof(Index));
    }

    private void SetOperationMessage(OrderOperationResult result)
    {
        if (result.Success)
        {
            TempData["Message"] = result.Message;
            return;
        }

        var details = result.Shortages.Count == 0
            ? string.Empty
            : " " + string.Join("; ", result.Shortages.Select(item =>
                $"{item.Name}: нужно {item.Required:N2} {item.UnitOfMeasure}, доступно {item.Available:N2}"));

        TempData["Error"] = result.Message + details;
    }
}
