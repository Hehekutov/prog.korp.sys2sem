using System.ComponentModel.DataAnnotations;

namespace Prac5.Models;

public class Material
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    [Required]
    [StringLength(30)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    public decimal MinimalStock { get; set; }

    public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
}
