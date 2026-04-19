namespace OceanCare.Api.Domain.Interfaces;

/// <summary>
/// Plugin interface for embedding generation.
/// Implement this interface to use a different embedding model (e.g., OpenAI, Ollama, HuggingFace).
/// </summary>
public interface IEmbeddingPlugin : IPlugin
{
    /// <summary>
    /// Generates a float vector embedding for the given text.
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);

    /// <summary>
    /// Returns the dimension of the embedding vector produced by this plugin.
    /// </summary>
    int EmbeddingDimension { get; }
}
