using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prac4.Data;

namespace Prac4.Controllers;

public class AttractionsController(TourGuideDbContext context) : Controller
{
    public async Task<IActionResult> Details(int id)
    {
        var attraction = await context.Attractions
            .Include(item => item.City)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (attraction is null)
        {
            return NotFound();
        }

        return View(attraction);
    }
}
