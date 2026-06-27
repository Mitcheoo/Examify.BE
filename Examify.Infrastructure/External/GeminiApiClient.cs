// Examify.Infrastructure/External/GeminiApiClient.cs
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Examify.Infrastructure.External;

public interface IGeminiApiClient
{
    Task<GeminiResponse> GenerateContentAsync(string prompt, string systemInstruction = "");
}

public class GeminiApiClient : IGeminiApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;

    public GeminiApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini API Key missing");
        _model = configuration["Gemini:Model"] ?? "gemini-1.5-flash";
        _baseUrl = "https://generativelanguage.googleapis.com/v1beta";
    }

    public async Task<GeminiResponse> GenerateContentAsync(string prompt, string systemInstruction = "")
    {
        // Xây dựng request body theo format Gemini
        var requestBody = new
        {
            system_instruction = string.IsNullOrEmpty(systemInstruction) ? null : new
            {
                parts = new[] { new { text = systemInstruction } }
            },
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            },
            generation_config = new
            {
                temperature = 0.3,
                max_output_tokens = 4096,
                response_mime_type = "application/json"
            }
        };

        // Loại bỏ system_instruction nếu null
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        var jsonRequest = JsonSerializer.Serialize(requestBody, options);

        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Gemini dùng API key qua query string
        var url = $"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}";

        _httpClient.DefaultRequestHeaders.Clear();

        var response = await _httpClient.PostAsync(url, content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini API Error: {jsonResponse}");

        var result = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse);
        return result ?? throw new Exception("Failed to parse Gemini response");
    }
}

// Response models cho Gemini
public class GeminiResponse
{
    public List<GeminiCandidate> candidates { get; set; } = new();
    public GeminiUsageMetadata? usageMetadata { get; set; }
}

public class GeminiCandidate
{
    public GeminiContent? content { get; set; }
    public string? finishReason { get; set; }
    public int? index { get; set; }
}

public class GeminiContent
{
    public List<GeminiPart> parts { get; set; } = new();
    public string? role { get; set; }
}

public class GeminiPart
{
    public string? text { get; set; }
}

public class GeminiUsageMetadata
{
    public int promptTokenCount { get; set; }
    public int candidatesTokenCount { get; set; }
    public int totalTokenCount { get; set; }
}