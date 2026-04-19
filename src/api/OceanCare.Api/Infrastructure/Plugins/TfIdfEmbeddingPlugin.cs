using OceanCare.Api.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace OceanCare.Api.Infrastructure.Plugins;

/// <summary>
/// A built-in embedding plugin using TF-IDF-inspired bag-of-words vectors.
/// This works out of the box without any external API keys.
/// Replace this plugin with OpenAI or Ollama for production semantic accuracy.
/// </summary>
public class TfIdfEmbeddingPlugin : IEmbeddingPlugin
{
    public string Name => "TF-IDF Embedding Plugin";
    public string Version => "1.0.0";
    public string Description => "Generates embeddings using a TF-IDF bag-of-words approach. No external API required.";
    public int EmbeddingDimension => 512;

    private static readonly string[] Stopwords =
        ["a", "an", "the", "is", "it", "in", "on", "at", "to", "and", "or", "for", "of", "with", "by", "as", "be", "are", "was", "its", "can", "has"];

    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var tokens = Tokenize(text);
        var vector = new float[EmbeddingDimension];

        foreach (var token in tokens)
        {
            var hash = Math.Abs(token.GetHashCode()) % EmbeddingDimension;
            // Simple TF-IDF-like weight: 1/sqrt(length) to normalize
            vector[hash] += 1.0f / (float)Math.Sqrt(token.Length + 1);
        }

        // Add character n-gram features for better partial matching
        var bigrams = GetNgrams(text.ToLower(), 3);
        foreach (var gram in bigrams)
        {
            var hash = (Math.Abs(gram.GetHashCode()) + 31337) % EmbeddingDimension;
            vector[hash] += 0.5f;
        }

        return Task.FromResult(Normalize(vector));
    }

    private static string[] Tokenize(string text)
    {
        return Regex.Split(text.ToLower(), @"\W+")
            .Where(t => t.Length > 2 && !Stopwords.Contains(t))
            .ToArray();
    }

    private static IEnumerable<string> GetNgrams(string text, int n)
    {
        var clean = Regex.Replace(text, @"\s+", " ").Trim();
        for (int i = 0; i <= clean.Length - n; i++)
            yield return clean.Substring(i, n);
    }

    private static float[] Normalize(float[] vector)
    {
        var magnitude = (float)Math.Sqrt(vector.Sum(x => x * x));
        if (magnitude < float.Epsilon) return vector;
        return vector.Select(x => x / magnitude).ToArray();
    }
}
