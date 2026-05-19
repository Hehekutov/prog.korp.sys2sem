namespace Prac5.Models;

public class DashboardViewModel
{
    public IReadOnlyList<Material> Materials { get; init; } = Array.Empty<Material>();

    public IReadOnlyList<Product> Products { get; init; } = Array.Empty<Product>();

    public IReadOnlyList<Product> AllProducts { get; init; } = Array.Empty<Product>();

    public IReadOnlyList<WorkOrder> WorkOrders { get; init; } = Array.Empty<WorkOrder>();

    public IReadOnlyList<ProductionLine> Lines { get; init; } = Array.Empty<ProductionLine>();

    public IReadOnlyList<string> Categories { get; init; } = Array.Empty<string>();

    public string? CategoryFilter { get; init; }

    public string? Search { get; init; }

    public int LowStockCount => Materials.Count(material => material.Quantity <= material.MinimalStock);

    public int ActiveOrderCount => WorkOrders.Count(order =>
        order.Status is WorkOrderStatus.Pending or WorkOrderStatus.InProgress);

    public int AvailableLineCount => Lines.Count(line =>
        line.Status == ProductionLineStatus.Active && line.CurrentWorkOrderId is null);
}
