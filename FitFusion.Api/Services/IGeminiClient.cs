namespace FitFusion.Api.Services;

public interface IGeminiClient
{
    /// <summary>
    /// Llama a Gemini con structured output. <paramref name="responseSchemaJson"/>
    /// es un JsonSchema (string JSON) que define la forma exacta del objeto que
    /// el modelo debe devolver. La respuesta se deserializa a <typeparamref name="T"/>
    /// con el mismo JsonSerializerOptions del pipeline (camelCase).
    /// </summary>
    Task<T> GenerateAsync<T>(string prompt, string responseSchemaJson, CancellationToken ct);
}

public sealed class GeminiException : Exception
{
    public GeminiException(string message, Exception? inner = null) : base(message, inner) { }
}
