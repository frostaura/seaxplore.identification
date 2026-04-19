using Microsoft.AspNetCore.Mvc;
using OceanCare.Api.Domain.Interfaces;

namespace OceanCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int topK = 10,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Query parameter 'q' is required.");

        var results = await searchService.SearchAsync(q, Math.Clamp(topK, 1, 50), ct);
        return Ok(results);
    }
}
