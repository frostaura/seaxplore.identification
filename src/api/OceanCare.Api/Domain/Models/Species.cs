namespace OceanCare.Api.Domain.Models;

public class Species
{
    public int Id { get; set; }
    public string ScientificName { get; set; } = string.Empty;
    public string CommonName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<SpeciesAttributeValue> AttributeValues { get; set; } = [];
    public SearchEmbedding? Embedding { get; set; }
}
