using OceanCare.Api.Models.DTOs;

namespace OceanCare.Api.Domain.Interfaces;

/// <summary>
/// Service responsible for performing semantic searches across the marine life catalogue.
/// </summary>
public interface ISearchService
{
    Task<IEnumerable<SpeciesSearchResultDto>> SearchAsync(string query, int topK = 10, CancellationToken ct = default);
}
