using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prac4.Data;

namespace Prac4.Controllers;

public class CitiesController(TourGuideDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var cities = await context.Cities
            .Include(city => city.Attractions)
            .AsNoTracking()
            .OrderBy(city => city.Name)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            cities = cities
                .Where(city => city.Name.Contains(normalizedSearch, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
        }

        ViewData["Search"] = search;
        return View(cities);
    }

    public async Task<IActionResult> Details(int id)
    {
        var city = await context.Cities
            .Include(city => city.Attractions)
            .AsNoTracking()
            .FirstOrDefaultAsync(city => city.Id == id);

        if (city is null)
        {
            return NotFound();
        }

        return View(city);
    }
}
