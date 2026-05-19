using System.ComponentModel.DataAnnotations;

namespace Prac5.Models;

public class WorkOrder
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int? ProductionLineId { get; set; }

    public ProductionLine? ProductionLine { get; set; }

    public int Quantity { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EstimatedEndDate { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = WorkOrderStatus.Pending;

    public int ProgressPercent { get; set; }
}

public static class WorkOrderStatus
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}
