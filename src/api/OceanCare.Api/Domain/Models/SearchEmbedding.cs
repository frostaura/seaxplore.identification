namespace OceanCare.Api.Domain.Models;

public class SearchEmbedding
{
    public int Id { get; set; }
    public int SpeciesId { get; set; }
    public Species Species { get; set; } = null!;
    public string EmbeddingJson { get; set; } = "[]";

    public float[] GetVector()
    {
        return System.Text.Json.JsonSerializer.Deserialize<float[]>(EmbeddingJson) ?? [];
    }

    public void SetVector(float[] vector)
    {
        EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(vector);
    }
}
