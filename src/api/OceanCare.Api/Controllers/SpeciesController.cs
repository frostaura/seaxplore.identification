using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OceanCare.Api.Application.Services;
using OceanCare.Api.Infrastructure.Data;
using OceanCare.Api.Models.DTOs;

namespace OceanCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeciesController(OceanCareDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var species = await db.Species
            .Include(s => s.Category)
            .Include(s => s.AttributeValues).ThenInclude(av => av.Attribute)
            .ToListAsync(ct);

        return Ok(species.Select(SearchService.MapToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var species = await db.Species
            .Include(s => s.Category)
            .Include(s => s.AttributeValues).ThenInclude(av => av.Attribute)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return species is null ? NotFound() : Ok(SearchService.MapToDto(species));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var cats = await db.Categories.ToListAsync(ct);
        return Ok(cats.Select(c => new CategoryDto(c.Id, c.Name, c.Description)));
    }
}
