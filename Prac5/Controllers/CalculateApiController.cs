using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Prac5.Services;

namespace Prac5.Controllers;

[ApiController]
[Route("api/calculate")]
public class CalculateApiController : ControllerBase
{
    private readonly ProductionWorkflowService _workflow;

    public CalculateApiController(ProductionWorkflowService workflow)
    {
        _workflow = workflow;
    }

    [HttpPost("production")]
    public async Task<IActionResult> CalculateProduction(ProductionCalculationRequest request)
    {
        var calculation = await _workflow.CalculateAsync(request.ProductId, request.Quantity, request.LineId);
        if (calculation is null)
        {
            return BadRequest("Продукт не найден или указано некорректное количество.");
        }

        return Ok(new
        {
            calculation.ProductId,
            calculation.ProductName,
            calculation.Quantity,
            calculation.EfficiencyFactor,
            calculation.TotalMinutes,
            EstimatedEndDate = DateTime.Now.AddMinutes(calculation.TotalMinutes)
        });
    }
}

public sealed class ProductionCalculationRequest
{
    [JsonPropertyName("product_id")]
    public int ProductId { get; init; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; init; }

    [JsonPropertyName("line_id")]
    public int? LineId { get; init; }
}
