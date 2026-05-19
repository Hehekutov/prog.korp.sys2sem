using System.ComponentModel.DataAnnotations;

namespace Prac5.Models;

public class ProductionLine
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = ProductionLineStatus.Active;

    public float EfficiencyFactor { get; set; } = 1.0f;

    public int? CurrentWorkOrderId { get; set; }

    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}

public static class ProductionLineStatus
{
    public const string Active = "Active";
    public const string Stopped = "Stopped";
}
