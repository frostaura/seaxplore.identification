using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OceanCare.Api.Application.Services;
using OceanCare.Api.Domain.Interfaces;
using OceanCare.Api.Domain.Models;
using OceanCare.Api.Infrastructure.Data;
using OceanCare.Api.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OceanCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController(OceanCareDbContext db, IEmbeddingPlugin embeddingPlugin, IConfiguration configuration) : ControllerBase
{
    // POST api/admin/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request, CancellationToken ct)
    {
        var admin = await db.Admins.FirstOrDefaultAsync(a => a.Username == request.Username, ct);
        if (admin is null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var token = GenerateJwtToken(admin);
        return Ok(new AdminLoginResponse(token, admin.Username));
    }

    // ──────────── SPECIES ────────────

    [HttpGet("species")]
    [Authorize]
    public async Task<IActionResult> ListSpecies(CancellationToken ct)
    {
        var all = await db.Species
            .Include(s => s.Category)
            .Include(s => s.AttributeValues).ThenInclude(av => av.Attribute)
            .ToListAsync(ct);
        return Ok(all.Select(SearchService.MapToDto));
    }

    [HttpPost("species")]
    [Authorize]
    public async Task<IActionResult> CreateSpecies([FromBody] CreateSpeciesRequest req, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([req.CategoryId], ct);
        if (category is null) return BadRequest("Category not found.");

        var species = new Species
        {
            ScientificName = req.ScientificName,
            CommonName = req.CommonName,
            Description = req.Description,
            ImageUrl = req.ImageUrl,
            CategoryId = req.CategoryId
        };
        db.Species.Add(species);
        await db.SaveChangesAsync(ct);

        await SaveAttributeValuesAsync(species.Id, req.Attributes, ct);
        await RegenerateEmbeddingAsync(species, ct);

        return CreatedAtAction(nameof(SpeciesController.GetById), "Species", new { id = species.Id },
            SearchService.MapToDto(await LoadSpeciesAsync(species.Id, ct)));
    }

    [HttpPut("species/{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateSpecies(int id, [FromBody] UpdateSpeciesRequest req, CancellationToken ct)
    {
        var species = await db.Species.Include(s => s.AttributeValues).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (species is null) return NotFound();

        species.ScientificName = req.ScientificName;
        species.CommonName = req.CommonName;
        species.Description = req.Description;
        species.ImageUrl = req.ImageUrl;
        species.CategoryId = req.CategoryId;

        db.SpeciesAttributeValues.RemoveRange(species.AttributeValues);
        await db.SaveChangesAsync(ct);

        await SaveAttributeValuesAsync(species.Id, req.Attributes, ct);
        await RegenerateEmbeddingAsync(species, ct);

        return Ok(SearchService.MapToDto(await LoadSpeciesAsync(id, ct)));
    }

    [HttpDelete("species/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteSpecies(int id, CancellationToken ct)
    {
        var species = await db.Species
            .Include(s => s.Embedding)
            .Include(s => s.AttributeValues)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
        if (species is null) return NotFound();

        if (species.Embedding is not null) db.SearchEmbeddings.Remove(species.Embedding);
        db.SpeciesAttributeValues.RemoveRange(species.AttributeValues);
        db.Species.Remove(species);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ──────────── ATTRIBUTES ────────────

    [HttpGet("attributes")]
    [Authorize]
    public async Task<IActionResult> ListAttributes(CancellationToken ct)
    {
        var attrs = await db.Attributes.ToListAsync(ct);
        return Ok(attrs.Select(a => new MarineAttributeDto(a.Id, a.Name, a.Description, a.DataType)));
    }

    [HttpPost("attributes")]
    [Authorize]
    public async Task<IActionResult> CreateAttribute([FromBody] CreateAttributeRequest req, CancellationToken ct)
    {
        var attr = new MarineAttribute { Name = req.Name, Description = req.Description, DataType = req.DataType };
        db.Attributes.Add(attr);
        await db.SaveChangesAsync(ct);
        return Ok(new MarineAttributeDto(attr.Id, attr.Name, attr.Description, attr.DataType));
    }

    [HttpPut("attributes/{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateAttribute(int id, [FromBody] UpdateAttributeRequest req, CancellationToken ct)
    {
        var attr = await db.Attributes.FindAsync([id], ct);
        if (attr is null) return NotFound();
        attr.Name = req.Name;
        attr.Description = req.Description;
        attr.DataType = req.DataType;
        await db.SaveChangesAsync(ct);
        return Ok(new MarineAttributeDto(attr.Id, attr.Name, attr.Description, attr.DataType));
    }

    [HttpDelete("attributes/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteAttribute(int id, CancellationToken ct)
    {
        var attr = await db.Attributes.FindAsync([id], ct);
        if (attr is null) return NotFound();
        db.Attributes.Remove(attr);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ──────────── CATEGORIES ────────────

    [HttpGet("categories")]
    [Authorize]
    public async Task<IActionResult> ListCategories(CancellationToken ct)
    {
        var cats = await db.Categories.ToListAsync(ct);
        return Ok(cats.Select(c => new CategoryDto(c.Id, c.Name, c.Description)));
    }

    [HttpPost("categories")]
    [Authorize]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest req, CancellationToken ct)
    {
        var cat = new Category { Name = req.Name, Description = req.Description };
        db.Categories.Add(cat);
        await db.SaveChangesAsync(ct);
        return Ok(new CategoryDto(cat.Id, cat.Name, cat.Description));
    }

    [HttpPut("categories/{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest req, CancellationToken ct)
    {
        var cat = await db.Categories.FindAsync([id], ct);
        if (cat is null) return NotFound();
        cat.Name = req.Name;
        cat.Description = req.Description;
        await db.SaveChangesAsync(ct);
        return Ok(new CategoryDto(cat.Id, cat.Name, cat.Description));
    }

    [HttpDelete("categories/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct)
    {
        var cat = await db.Categories.FindAsync([id], ct);
        if (cat is null) return NotFound();
        db.Categories.Remove(cat);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ──────────── HELPERS ────────────

    private string GenerateJwtToken(Admin admin)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task SaveAttributeValuesAsync(int speciesId, IEnumerable<AttributeValueRequest> attrs, CancellationToken ct)
    {
        foreach (var av in attrs)
        {
            db.SpeciesAttributeValues.Add(new SpeciesAttributeValue
            {
                SpeciesId = speciesId,
                AttributeId = av.AttributeId,
                Value = av.Value
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task RegenerateEmbeddingAsync(Species species, CancellationToken ct)
    {
        var full = await LoadSpeciesAsync(species.Id, ct);
        var searchText = $"{full.CommonName} {full.ScientificName} {full.Description} {string.Join(" ", full.AttributeValues.Select(a => $"{a.Attribute?.Name}: {a.Value}"))}";
        var vector = await embeddingPlugin.GenerateEmbeddingAsync(searchText, ct);

        var existing = await db.SearchEmbeddings.FirstOrDefaultAsync(e => e.SpeciesId == species.Id, ct);
        if (existing is not null)
        {
            existing.SetVector(vector);
        }
        else
        {
            var emb = new SearchEmbedding { SpeciesId = species.Id };
            emb.SetVector(vector);
            db.SearchEmbeddings.Add(emb);
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task<Domain.Models.Species> LoadSpeciesAsync(int id, CancellationToken ct) =>
        await db.Species
            .Include(s => s.Category)
            .Include(s => s.AttributeValues).ThenInclude(av => av.Attribute)
            .FirstAsync(s => s.Id == id, ct);
}
