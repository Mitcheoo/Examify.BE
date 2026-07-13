// 📁 Examify.Infrastructure/External/OpenAIClient.cs

using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Examify.Infrastructure.External;

public interface IOpenAIClient
{
    Task<OpenAIResponse?> GenerateContentAsync(string prompt, string systemPrompt = "");
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
        _chatModel = configuration["OpenAI:ChatModel"] ?? "gpt-4o-mini";

        var baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";

        _httpClient.BaseAddress = new Uri(baseUrl);

        Console.WriteLine($"✅ OpenAIClient initialized!");
        Console.WriteLine($"   BaseAddress: {_httpClient.BaseAddress}");
        Console.WriteLine($"   Model: {_chatModel}");
        Console.WriteLine($"   API Key present: {!string.IsNullOrEmpty(_apiKey)}");
    }

    public async Task<OpenAIResponse?> GenerateContentAsync(string prompt, string systemPrompt = "")
    {
        Console.WriteLine($"📤 Sending to OpenAI: {prompt.Substring(0, Math.Min(200, prompt.Length))}...");
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
            max_tokens = 4096
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ OpenAI API Error ({response.StatusCode}): {responseJson}");
                return new OpenAIResponse
                {
                    Content = "{}"
                };
            }

            Console.WriteLine($"✅ OpenAI Response received ({responseJson.Length} chars)");

            var result = JsonSerializer.Deserialize<OpenAIResponseWrapper>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Choices != null && result.Choices.Count > 0)
            {
                var contentResponse = new OpenAIResponse
                {
                    Content = result.Choices[0].Message?.Content ?? "{}"
                };
                Console.WriteLine($"📥 OpenAI Content: {contentResponse.Content.Substring(0, Math.Min(200, contentResponse.Content.Length))}...");
                return contentResponse;
            }

            Console.WriteLine("❌ No choices in OpenAI response");
            return new OpenAIResponse { Content = "{}" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ OpenAI Exception: {ex.Message}");
            return new OpenAIResponse { Content = "{}" };
        }
    }
}

// ============================================================
// RESPONSE MODELS
// ============================================================

public class OpenAIResponseWrapper
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

public class OpenAIResponse
{
    public string Content { get; set; } = string.Empty;
}