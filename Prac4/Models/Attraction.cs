using System.ComponentModel.DataAnnotations;

namespace Prac4.Models;

public class Attraction
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(360)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required]
    public string History { get; set; } = string.Empty;

    [MaxLength(360)]
    public string ImagePath { get; set; } = string.Empty;

    [MaxLength(360)]
    public string PhotoSourceUrl { get; set; } = string.Empty;

    [MaxLength(120)]
    public string OpeningHours { get; set; } = string.Empty;

    [MaxLength(120)]
    public string TicketPrice { get; set; } = string.Empty;

    [MaxLength(220)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(500)]
    public string MapUrl { get; set; } = string.Empty;

    public int CityId { get; set; }

    public City? City { get; set; }
}
