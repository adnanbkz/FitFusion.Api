using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FitFusion.Api.Services;

public sealed class GeminiClient : IGeminiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<GeminiClient> _log;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _json;

    public GeminiClient(HttpClient http, IConfiguration config, ILogger<GeminiClient> log)
    {
        _http   = http;
        _log    = log;
        _apiKey = config["Gemini:ApiKey"]
            ?? throw new InvalidOperationException(
                "Gemini:ApiKey no está configurada. Ejecuta: dotnet user-secrets set \"Gemini:ApiKey\" \"<key>\"");
        _model   = config["Gemini:Model"]   ?? "gemini-2.0-flash";
        _baseUrl = config["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";

        _json = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        var timeoutSeconds = int.TryParse(config["Gemini:TimeoutSeconds"], out var t) ? t : 45;
        _http.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public async Task<T> GenerateAsync<T>(string prompt, string responseSchemaJson, CancellationToken ct)
    {
        var url = $"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}";

        var body = new JsonObject
        {
            ["contents"] = new JsonArray
            {
                new JsonObject
                {
                    ["parts"] = new JsonArray
                    {
                        new JsonObject { ["text"] = prompt }
                    }
                }
            },
            ["generationConfig"] = new JsonObject
            {
                ["responseMimeType"] = "application/json",
                ["responseSchema"]   = JsonNode.Parse(responseSchemaJson),
                ["temperature"]      = 0.2,
            },
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"),
        };

        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(req, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new GeminiException("Gemini timeout");
        }
        catch (Exception e)
        {
            throw new GeminiException($"Error de red llamando a Gemini: {e.Message}", e);
        }

        var raw = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _log.LogWarning("Gemini {Status}: {Body}", (int)resp.StatusCode, raw);
            var detail = ExtractGeminiErrorMessage(raw) ?? Truncate(raw, 300);
            throw new GeminiException($"Gemini {(int)resp.StatusCode}: {detail}");
        }

        // Respuesta: { candidates: [{ content: { parts: [{ text: "<json>" }] } }] }
        string text;
        try
        {
            var root = JsonNode.Parse(raw)
                ?? throw new GeminiException("Respuesta vacía de Gemini");
            text = root["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>()
                ?? throw new GeminiException("Gemini no devolvió texto en la respuesta");
        }
        catch (Exception e) when (e is not GeminiException)
        {
            throw new GeminiException("Respuesta de Gemini con formato inesperado", e);
        }

        try
        {
            var result = JsonSerializer.Deserialize<T>(text, _json)
                ?? throw new GeminiException("No se pudo deserializar la respuesta de Gemini");
            return result;
        }
        catch (JsonException e)
        {
            _log.LogWarning("JSON inválido de Gemini: {Text}", text);
            throw new GeminiException("JSON inválido devuelto por Gemini", e);
        }
    }

    private static string? ExtractGeminiErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            var root = JsonNode.Parse(body);
            return root?["error"]?["message"]?.GetValue<string>();
        }
        catch { return null; }
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s.Substring(0, max) + "...";
}
