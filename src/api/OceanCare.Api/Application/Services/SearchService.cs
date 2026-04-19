using Microsoft.EntityFrameworkCore;
using OceanCare.Api.Domain.Interfaces;
using OceanCare.Api.Infrastructure.Data;
using OceanCare.Api.Models.DTOs;

namespace OceanCare.Api.Application.Services;

public class SearchService(OceanCareDbContext db, IEmbeddingPlugin embeddingPlugin) : ISearchService
{
    public async Task<IEnumerable<SpeciesSearchResultDto>> SearchAsync(string query, int topK = 10, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var queryVector = await embeddingPlugin.GenerateEmbeddingAsync(query, ct);

        var embeddings = await db.SearchEmbeddings
            .Include(e => e.Species)
                .ThenInclude(s => s.Category)
            .Include(e => e.Species)
                .ThenInclude(s => s.AttributeValues)
                    .ThenInclude(av => av.Attribute)
            .ToListAsync(ct);

        return embeddings
            .Select(e =>
            {
                var vector = e.GetVector();
                var score = CosineSimilarity(queryVector, vector);
                return new SpeciesSearchResultDto(MapToDto(e.Species), score);
            })
            .Where(r => r.SimilarityScore > 0.01f)
            .OrderByDescending(r => r.SimilarityScore)
            .Take(topK);
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0;
        var dot = a.Zip(b, (x, y) => x * y).Sum();
        return dot; // already normalized
    }

    internal static SpeciesDto MapToDto(Domain.Models.Species s) => new(
        s.Id,
        s.ScientificName,
        s.CommonName,
        s.Description,
        s.ImageUrl,
        s.CategoryId,
        s.Category?.Name ?? string.Empty,
        s.AttributeValues.Select(av => new AttributeValueDto(av.AttributeId, av.Attribute?.Name ?? string.Empty, av.Value))
    );
}
