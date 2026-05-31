using System.ComponentModel.DataAnnotations;

namespace BlazorShared;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название товара")]
    [StringLength(120, ErrorMessage = "Название не должно быть длиннее 120 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите категорию")]
    [StringLength(80, ErrorMessage = "Категория не должна быть длиннее 80 символов")]
    public string Category { get; set; } = string.Empty;

    [Range(0.01, 1_000_000, ErrorMessage = "Цена должна быть больше 0")]
    public decimal Price { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Количество не может быть отрицательным")]
    public int Quantity { get; set; }

    [StringLength(500, ErrorMessage = "Описание не должно быть длиннее 500 символов")]
    public string Description { get; set; } = string.Empty;
}
