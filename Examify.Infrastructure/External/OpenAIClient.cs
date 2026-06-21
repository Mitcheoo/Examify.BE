// Examify.Infrastructure/External/OpenAIClient.cs
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Examify.Infrastructure.External;

public interface IOpenAIClient
{
    Task<OpenAIResponse> GenerateContentAsync(string prompt, string systemPrompt = "");
    // ❌ XÓA: Task<string> TranscribeAsync(byte[] audioData);
}

public class OpenAIClient : IOpenAIClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _chatModel;

    public OpenAIClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new Exception("OpenAI API Key missing");

        // ✅ Đọc đúng key
        _chatModel = configuration["OpenAI:ChatModel"] ?? "gpt-4o-mini";

        var baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";

        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<OpenAIResponse> GenerateContentAsync(string prompt, string systemPrompt = "")
    {
        // ✅ THÊM LOG
        Console.WriteLine($"📡 Calling OpenAI Chat: {_httpClient.BaseAddress}chat/completions");
        Console.WriteLine($"📡 Model: {_chatModel}");

        var request = new
        {
            model = _chatModel,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = 4096,
            response_format = new { type = "json_object" }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.PostAsync("chat/completions", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ OpenAI API Error ({response.StatusCode}): {responseJson}");
            throw new Exception($"OpenAI API Error: {responseJson}");
        }

        var result = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);
        return result ?? throw new Exception("Failed to parse OpenAI response");
    }
}

// ============================================================
// RESPONSE MODELS
// ============================================================

public class OpenAIResponse
{
    public List<OpenAIChoice> Choices { get; set; } = new();
}

public class OpenAIChoice
{
    public OpenAIMessage Message { get; set; } = new();
}

public class OpenAIMessage
{
    public string Content { get; set; } = string.Empty;
}