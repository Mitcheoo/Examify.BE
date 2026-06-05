// Examify.Infrastructure/External/DeepSeekApiClient.cs
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Examify.Infrastructure.External;

public interface IDeepSeekApiClient
{
    Task<DeepSeekResponse> ChatCompletionAsync(List<DeepSeekMessage> messages);
}

public class DeepSeekApiClient : IDeepSeekApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public DeepSeekApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["DeepSeek:ApiKey"] ?? throw new Exception("DeepSeek API Key missing");
        _baseUrl = configuration["DeepSeek:BaseUrl"] ?? "https://api.deepseek.com/v1";
    }

    public async Task<DeepSeekResponse> ChatCompletionAsync(List<DeepSeekMessage> messages)
    {
        var request = new
        {
            model = "deepseek-chat",
            messages = messages,
            temperature = 0.3,
            response_format = new { type = "json_object" }
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"DeepSeek API Error: {jsonResponse}");

        return JsonSerializer.Deserialize<DeepSeekResponse>(jsonResponse) ?? throw new Exception("Failed to parse response");
    }
}

public class DeepSeekMessage
{
    public string role { get; set; } = "";
    public string content { get; set; } = "";
}

public class DeepSeekResponse
{
    public List<DeepSeekChoice> choices { get; set; } = new();
}

public class DeepSeekChoice
{
    public DeepSeekMessageContent message { get; set; } = new();
}

public class DeepSeekMessageContent
{
    public string content { get; set; } = "";
}