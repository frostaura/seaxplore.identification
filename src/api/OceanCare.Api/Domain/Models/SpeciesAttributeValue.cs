namespace OceanCare.Api.Domain.Models;

public class SpeciesAttributeValue
{
    public int Id { get; set; }
    public int SpeciesId { get; set; }
    public Species Species { get; set; } = null!;
    public int AttributeId { get; set; }
    public MarineAttribute Attribute { get; set; } = null!;
    public string Value { get; set; } = string.Empty;
}
