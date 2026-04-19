namespace OceanCare.Api.Domain.Models;

public class MarineAttribute
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public ICollection<SpeciesAttributeValue> Values { get; set; } = [];
}
