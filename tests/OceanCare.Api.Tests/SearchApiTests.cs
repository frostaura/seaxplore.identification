using System.Net;
using System.Net.Http.Json;
using OceanCare.Api.Models.DTOs;
using OceanCare.Api.Tests.Infrastructure;

namespace OceanCare.Api.Tests;

public class SearchApiTests
{
    [Fact]
    public async Task Search_returns_seeded_species_ranked_for_matching_query()
    {
        using var factory = new OceanCareApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/search?q=orange%20white%20striped%20fish&topK=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var results = await response.Content.ReadFromJsonAsync<List<SpeciesSearchResultDto>>();

        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Equal("Clownfish", results[0].Species.CommonName);
        Assert.True(results[0].SimilarityScore > 0);
    }
}
