using System.ComponentModel.DataAnnotations;

namespace Prac4.Models;

public class City
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Region { get; set; } = string.Empty;

    public int Population { get; set; }

    [Required]
    public string History { get; set; } = string.Empty;

    [MaxLength(360)]
    public string ImagePath { get; set; } = string.Empty;

    [MaxLength(360)]
    public string PhotoSourceUrl { get; set; } = string.Empty;

    [MaxLength(240)]
    public string CoatOfArmsPath { get; set; } = string.Empty;

    [MaxLength(360)]
    public string CoatOfArmsSourceUrl { get; set; } = string.Empty;

    [MaxLength(160)]
    public string HeroCaption { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Established { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Climate { get; set; } = string.Empty;

    [MaxLength(16)]
    public string AccentColor { get; set; } = "#2f6f73";

    public ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();
}
