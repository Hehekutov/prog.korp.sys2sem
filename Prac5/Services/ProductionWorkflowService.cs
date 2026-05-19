using Microsoft.EntityFrameworkCore;
using Prac5.Data;
using Prac5.Models;

namespace Prac5.Services;

public class ProductionWorkflowService
{
    private readonly ProductionDbContext _context;

    public ProductionWorkflowService(ProductionDbContext context)
    {
        _context = context;
    }

    public async Task<ProductionCalculation?> CalculateAsync(int productId, int quantity, int? lineId = null)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product is null || quantity <= 0)
        {
            return null;
        }

        var efficiency = 1.0f;
        if (lineId is not null)
        {
            var line = await _context.ProductionLines.FindAsync(lineId);
            efficiency = line?.EfficiencyFactor ?? efficiency;
        }

        var minutes = CalculateMinutes(product.ProductionTimePerUnit, quantity, efficiency);
        return new ProductionCalculation(product.Id, product.Name, quantity, efficiency, minutes);
    }

    public async Task<OrderOperationResult> CreateOrderAsync(
        int productId,
        int quantity,
        int? lineId,
        DateTime? startDate = null)
    {
        if (quantity <= 0)
        {
            return OrderOperationResult.Fail("Количество должно быть больше нуля.");
        }

        var product = await _context.Products
            .Include(item => item.ProductMaterials)
            .ThenInclude(item => item.Material)
            .FirstOrDefaultAsync(item => item.Id == productId);

        if (product is null)
        {
            return OrderOperationResult.Fail("Продукт не найден.");
        }

        ProductionLine? line = null;
        if (lineId is not null)
        {
            line = await _context.ProductionLines.FindAsync(lineId);
            if (line is null)
            {
                return OrderOperationResult.Fail("Производственная линия не найдена.");
            }

            if (line.Status != ProductionLineStatus.Active)
            {
                return OrderOperationResult.Fail("Выбранная линия остановлена.");
            }
        }

        var shortages = GetShortages(product, quantity);
        if (shortages.Count > 0)
        {
            return OrderOperationResult.Fail("Недостаточно материалов для создания заказа.", shortages);
        }

        foreach (var item in product.ProductMaterials)
        {
            if (item.Material is not null)
            {
                item.Material.Quantity -= item.QuantityNeeded * quantity;
            }
        }

        var start = startDate ?? DateTime.Now;
        var efficiency = line?.EfficiencyFactor ?? 1.0f;
        var minutes = CalculateMinutes(product.ProductionTimePerUnit, quantity, efficiency);
        var order = new WorkOrder
        {
            ProductId = product.Id,
            ProductionLineId = line?.Id,
            Quantity = quantity,
            StartDate = start,
            EstimatedEndDate = start.AddMinutes(minutes),
            Status = WorkOrderStatus.Pending,
            ProgressPercent = 0
        };

        _context.WorkOrders.Add(order);
        await _context.SaveChangesAsync();

        return OrderOperationResult.Ok("Заказ создан, материалы зарезервированы.", order);
    }

    public async Task<OrderOperationResult> StartOrderAsync(int orderId)
    {
        var order = await _context.WorkOrders
            .Include(item => item.Product)
            .FirstOrDefaultAsync(item => item.Id == orderId);

        if (order is null)
        {
            return OrderOperationResult.Fail("Заказ не найден.");
        }

        if (order.Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
        {
            return OrderOperationResult.Fail("Нельзя запустить завершенный или отмененный заказ.");
        }

        var line = order.ProductionLineId is null
            ? await _context.ProductionLines
                .Where(item => item.Status == ProductionLineStatus.Active && item.CurrentWorkOrderId == null)
                .OrderByDescending(item => item.EfficiencyFactor)
                .FirstOrDefaultAsync()
            : await _context.ProductionLines.FindAsync(order.ProductionLineId);

        if (line is null)
        {
            return OrderOperationResult.Fail("Нет доступной производственной линии.");
        }

        if (line.Status != ProductionLineStatus.Active || (line.CurrentWorkOrderId is not null && line.CurrentWorkOrderId != order.Id))
        {
            return OrderOperationResult.Fail("Линия занята или остановлена.");
        }

        order.ProductionLineId = line.Id;
        order.Status = WorkOrderStatus.InProgress;
        order.StartDate = DateTime.Now;
        order.ProgressPercent = Math.Max(order.ProgressPercent, 1);

        if (order.Product is not null)
        {
            var minutes = CalculateMinutes(order.Product.ProductionTimePerUnit, order.Quantity, line.EfficiencyFactor);
            order.EstimatedEndDate = order.StartDate.AddMinutes(minutes);
        }

        line.CurrentWorkOrderId = order.Id;
        await _context.SaveChangesAsync();

        return OrderOperationResult.Ok("Заказ запущен в производство.", order);
    }

    public async Task<OrderOperationResult> CancelOrderAsync(int orderId)
    {
        var order = await _context.WorkOrders
            .Include(item => item.Product)
            .ThenInclude(item => item!.ProductMaterials)
            .ThenInclude(item => item.Material)
            .FirstOrDefaultAsync(item => item.Id == orderId);

        if (order is null)
        {
            return OrderOperationResult.Fail("Заказ не найден.");
        }

        if (order.Status == WorkOrderStatus.Completed)
        {
            return OrderOperationResult.Fail("Завершенный заказ нельзя отменить.");
        }

        if (order.Status != WorkOrderStatus.Cancelled && order.Product is not null)
        {
            foreach (var item in order.Product.ProductMaterials)
            {
                if (item.Material is not null)
                {
                    item.Material.Quantity += item.QuantityNeeded * order.Quantity;
                }
            }
        }

        var line = order.ProductionLineId is null ? null : await _context.ProductionLines.FindAsync(order.ProductionLineId);
        if (line?.CurrentWorkOrderId == order.Id)
        {
            line.CurrentWorkOrderId = null;
        }

        order.Status = WorkOrderStatus.Cancelled;
        order.ProgressPercent = 0;
        await _context.SaveChangesAsync();

        return OrderOperationResult.Ok("Заказ отменен, зарезервированные материалы возвращены.", order);
    }

    public async Task<OrderOperationResult> UpdateProgressAsync(int orderId, int percent)
    {
        var order = await _context.WorkOrders.FindAsync(orderId);
        if (order is null)
        {
            return OrderOperationResult.Fail("Заказ не найден.");
        }

        if (order.Status == WorkOrderStatus.Cancelled)
        {
            return OrderOperationResult.Fail("Отмененный заказ нельзя обновить.");
        }

        order.ProgressPercent = Math.Clamp(percent, 0, 100);
        if (order.ProgressPercent >= 100)
        {
            order.Status = WorkOrderStatus.Completed;
            var line = order.ProductionLineId is null ? null : await _context.ProductionLines.FindAsync(order.ProductionLineId);
            if (line?.CurrentWorkOrderId == order.Id)
            {
                line.CurrentWorkOrderId = null;
            }
        }
        else if (order.ProgressPercent > 0 && order.Status == WorkOrderStatus.Pending)
        {
            order.Status = WorkOrderStatus.InProgress;
        }

        await _context.SaveChangesAsync();
        return OrderOperationResult.Ok("Прогресс заказа обновлен.", order);
    }

    public async Task<OrderOperationResult> RescheduleOrderAsync(int orderId, DateTime startDate)
    {
        var order = await _context.WorkOrders
            .Include(item => item.Product)
            .Include(item => item.ProductionLine)
            .FirstOrDefaultAsync(item => item.Id == orderId);

        if (order is null)
        {
            return OrderOperationResult.Fail("Заказ не найден.");
        }

        if (order.Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
        {
            return OrderOperationResult.Fail("Срок можно переносить только у активных заказов.");
        }

        var efficiency = order.ProductionLine?.EfficiencyFactor ?? 1.0f;
        var minutes = CalculateMinutes(order.Product?.ProductionTimePerUnit ?? 0, order.Quantity, efficiency);
        order.StartDate = startDate;
        order.EstimatedEndDate = startDate.AddMinutes(minutes);
        await _context.SaveChangesAsync();

        return OrderOperationResult.Ok("Срок заказа перенесен.", order);
    }

    public async Task<OrderOperationResult> UpdateLineStatusAsync(int lineId, string status)
    {
        if (status is not ProductionLineStatus.Active and not ProductionLineStatus.Stopped)
        {
            return OrderOperationResult.Fail("Некорректный статус линии.");
        }

        var line = await _context.ProductionLines.FindAsync(lineId);
        if (line is null)
        {
            return OrderOperationResult.Fail("Производственная линия не найдена.");
        }

        if (status == ProductionLineStatus.Stopped && line.CurrentWorkOrderId is not null)
        {
            var order = await _context.WorkOrders.FindAsync(line.CurrentWorkOrderId);
            if (order is not null && order.Status == WorkOrderStatus.InProgress)
            {
                order.Status = WorkOrderStatus.Pending;
            }

            line.CurrentWorkOrderId = null;
        }

        line.Status = status;
        await _context.SaveChangesAsync();

        return OrderOperationResult.Ok("Статус линии обновлен.");
    }

    public async Task<OrderOperationResult> UpdateLineEfficiencyAsync(int lineId, float efficiencyFactor)
    {
        var line = await _context.ProductionLines.FindAsync(lineId);
        if (line is null)
        {
            return OrderOperationResult.Fail("Производственная линия не найдена.");
        }

        line.EfficiencyFactor = Math.Clamp(efficiencyFactor, 0.5f, 2.0f);
        await _context.SaveChangesAsync();

        return OrderOperationResult.Ok("Коэффициент эффективности обновлен.");
    }

    private static int CalculateMinutes(int productionTimePerUnit, int quantity, float efficiencyFactor)
    {
        var safeEfficiency = Math.Clamp(efficiencyFactor, 0.5f, 2.0f);
        return (int)Math.Ceiling(quantity * productionTimePerUnit / safeEfficiency);
    }

    private static List<MaterialRequirement> GetShortages(Product product, int quantity)
    {
        return product.ProductMaterials
            .Where(item => item.Material is not null)
            .Select(item =>
            {
                var required = item.QuantityNeeded * quantity;
                var material = item.Material!;
                return new MaterialRequirement(
                    material.Id,
                    material.Name,
                    required,
                    material.Quantity,
                    material.UnitOfMeasure,
                    material.Quantity >= required);
            })
            .Where(item => !item.IsEnough)
            .ToList();
    }
}

public sealed record ProductionCalculation(
    int ProductId,
    string ProductName,
    int Quantity,
    float EfficiencyFactor,
    int TotalMinutes);

public sealed record MaterialRequirement(
    int MaterialId,
    string Name,
    decimal Required,
    decimal Available,
    string UnitOfMeasure,
    bool IsEnough);

public sealed class OrderOperationResult
{
    private OrderOperationResult(bool success, string message, WorkOrder? order, IReadOnlyList<MaterialRequirement> shortages)
    {
        Success = success;
        Message = message;
        Order = order;
        Shortages = shortages;
    }

    public bool Success { get; }

    public string Message { get; }

    public WorkOrder? Order { get; }

    public IReadOnlyList<MaterialRequirement> Shortages { get; }

    public static OrderOperationResult Ok(string message, WorkOrder? order = null)
    {
        return new OrderOperationResult(true, message, order, Array.Empty<MaterialRequirement>());
    }

    public static OrderOperationResult Fail(string message, IReadOnlyList<MaterialRequirement>? shortages = null)
    {
        return new OrderOperationResult(false, message, null, shortages ?? Array.Empty<MaterialRequirement>());
    }
}
