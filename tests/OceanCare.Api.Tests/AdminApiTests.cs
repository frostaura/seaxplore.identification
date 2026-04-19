using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OceanCare.Api.Models.DTOs;
using OceanCare.Api.Tests.Infrastructure;

namespace OceanCare.Api.Tests;

public class AdminApiTests
{
    [Fact]
    public async Task Login_rejects_invalid_credentials()
    {
        using var factory = new OceanCareApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/admin/login", new AdminLoginRequest("admin", "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Authorized_admin_can_complete_category_attribute_and_species_crud_flow()
    {
        using var factory = new OceanCareApiFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var unauthorizedClient = factory.CreateClient();
        var unauthorizedResponse = await unauthorizedClient.GetAsync("/api/admin/species");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

        var categoryResponse = await client.PostAsJsonAsync("/api/admin/categories", new CreateCategoryRequest("Cephalopod", "Tentacled marine animals"));
        categoryResponse.EnsureSuccessStatusCode();
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryDto>();

        var attributeResponse = await client.PostAsJsonAsync("/api/admin/attributes", new CreateAttributeRequest("Tentacle Count", "Typical number of tentacles", "number"));
        attributeResponse.EnsureSuccessStatusCode();
        var attribute = await attributeResponse.Content.ReadFromJsonAsync<MarineAttributeDto>();

        Assert.NotNull(category);
        Assert.NotNull(attribute);

        var createSpeciesResponse = await client.PostAsJsonAsync("/api/admin/species", new CreateSpeciesRequest(
            "Sepia officinalis",
            "Common Cuttlefish",
            "A color-shifting cephalopod with a broad mantle and eight arms plus two feeding tentacles.",
            "https://example.test/cuttlefish.jpg",
            category!.Id,
            [new AttributeValueRequest(attribute!.Id, "10")]));
        Assert.Equal(HttpStatusCode.Created, createSpeciesResponse.StatusCode);

        var createdSpecies = await createSpeciesResponse.Content.ReadFromJsonAsync<SpeciesDto>();
        Assert.NotNull(createdSpecies);
        Assert.Equal("Common Cuttlefish", createdSpecies!.CommonName);
        Assert.Equal(category.Name, createdSpecies.CategoryName);

        var updateSpeciesResponse = await client.PutAsJsonAsync($"/api/admin/species/{createdSpecies.Id}", new UpdateSpeciesRequest(
            "Sepia officinalis",
            "Common Cuttlefish",
            "An adaptive cephalopod that rapidly changes color and texture for camouflage.",
            "https://example.test/cuttlefish-updated.jpg",
            category.Id,
            [new AttributeValueRequest(attribute.Id, "10")]));
        updateSpeciesResponse.EnsureSuccessStatusCode();

        var updatedSpecies = await updateSpeciesResponse.Content.ReadFromJsonAsync<SpeciesDto>();
        Assert.NotNull(updatedSpecies);
        Assert.Contains("camouflage", updatedSpecies!.Description, StringComparison.OrdinalIgnoreCase);

        var deleteSpeciesResponse = await client.DeleteAsync($"/api/admin/species/{createdSpecies.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteSpeciesResponse.StatusCode);

        var speciesAfterDelete = await client.GetAsync($"/api/species/{createdSpecies.Id}");
        Assert.Equal(HttpStatusCode.NotFound, speciesAfterDelete.StatusCode);

        var deleteAttributeResponse = await client.DeleteAsync($"/api/admin/attributes/{attribute.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteAttributeResponse.StatusCode);

        var deleteCategoryResponse = await client.DeleteAsync($"/api/admin/categories/{category.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteCategoryResponse.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/admin/login", new AdminLoginRequest("admin", "OceanCare2024!"));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AdminLoginResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.Token));

        return payload.Token;
    }
}
