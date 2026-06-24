// Examify.Infrastructure/External/WhisperApiClient.cs
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Examify.Infrastructure.External;

public interface IWhisperApiClient
{
    Task<string> TranscribeAsync(byte[] audioData, string fileName = "audio.webm");
}

public class WhisperApiClient : IWhisperApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public WhisperApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        if (httpClient == null)
        {
            Console.WriteLine("❌ httpClient is NULL in WhisperApiClient constructor!");
            throw new ArgumentNullException(nameof(httpClient));
        }

        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new Exception("OpenAI API Key missing");

        _baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1/";
        if (!_baseUrl.EndsWith("/"))
            _baseUrl += "/";

        _httpClient.BaseAddress = new Uri(_baseUrl);

        Console.WriteLine($"✅ WhisperApiClient initialized successfully!");
        Console.WriteLine($"   BaseAddress: {_httpClient.BaseAddress}");
        Console.WriteLine($"   API Key present: {!string.IsNullOrEmpty(_apiKey)}");
    }

    public async Task<string> TranscribeAsync(byte[] audioData, string fileName = "audio.webm")
    {
        // ============================================================
        // 1. KIỂM TRA INPUT
        // ============================================================
        if (audioData == null || audioData.Length == 0)
        {
            Console.WriteLine($"⚠️ Audio data is null or empty for {fileName}");
            throw new ArgumentException("Audio data is null or empty", nameof(audioData));
        }

        Console.WriteLine($"🎤 Processing: {fileName}, Size: {audioData.Length} bytes");

        // ============================================================
        // 2. KIỂM TRA HTTPCLIENT
        // ============================================================
        if (_httpClient == null)
        {
            Console.WriteLine("❌ _httpClient is NULL!");
            throw new InvalidOperationException("HttpClient is not initialized");
        }

        if (_httpClient.BaseAddress == null)
        {
            Console.WriteLine("⚠️ BaseAddress is null, setting default...");
            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            Console.WriteLine("❌ API Key is NULL or EMPTY!");
            throw new InvalidOperationException("API Key is not configured");
        }

        // ============================================================
        // 3. GỌI API
        // ============================================================
        try
        {
            // ✅ Xác định đúng content type dựa trên file name
            var mediaType = fileName.EndsWith(".mp3") ? "audio/mpeg" :
                           fileName.EndsWith(".wav") ? "audio/wav" :
                           fileName.EndsWith(".webm") ? "audio/webm" :
                           fileName.EndsWith(".m4a") ? "audio/mp4" :
                           fileName.EndsWith(".ogg") ? "audio/ogg" :
                           "audio/webm";

            Console.WriteLine($"📡 Calling Whisper API: {_httpClient.BaseAddress}audio/transcriptions");
            Console.WriteLine($"🔑 API Key (first 10): {_apiKey.Substring(0, Math.Min(10, _apiKey.Length))}...");
            Console.WriteLine($"📁 Content-Type: {mediaType}");

            using var content = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(audioData);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            content.Add(fileContent, "file", fileName);
            content.Add(new StringContent("whisper-1"), "model");
            content.Add(new StringContent("en"), "language");
            content.Add(new StringContent("json"), "response_format");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            Console.WriteLine("📤 Sending request...");
            var startTime = DateTime.Now;

            var response = await _httpClient.PostAsync("audio/transcriptions", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            var elapsed = DateTime.Now - startTime;
            Console.WriteLine($"⏱️ Response time: {elapsed.TotalMilliseconds}ms");
            Console.WriteLine($"📡 Response Status: {(int)response.StatusCode} - {response.StatusCode}");

            // ============================================================
            // 4. XỬ LÝ RESPONSE
            // ============================================================
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Whisper API Error ({response.StatusCode}): {jsonResponse}");

                // Parse lỗi chi tiết
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<WhisperErrorResponse>(jsonResponse);
                    var errorMessage = errorResponse?.Error?.Message ?? jsonResponse;
                    throw new Exception($"Whisper API Error: {errorMessage}");
                }
                catch
                {
                    throw new Exception($"Whisper API Error: {jsonResponse}");
                }
            }

            Console.WriteLine($"📄 Raw Response: {jsonResponse}");

            // ✅ Parse đúng response structure
            var result = JsonSerializer.Deserialize<WhisperResponse>(jsonResponse);

            if (result == null)
            {
                Console.WriteLine("❌ Failed to deserialize Whisper response");
                throw new Exception("Failed to deserialize Whisper response");
            }

            Console.WriteLine($"📝 Transcript length: {result.Text?.Length ?? 0} chars");

            if (string.IsNullOrWhiteSpace(result.Text))
            {
                Console.WriteLine($"⚠️ Whisper returned empty transcript. Full response: {jsonResponse}");
                throw new Exception("Whisper returned empty transcript");
            }

            Console.WriteLine($"✅ Whisper Success! Transcript: {result.Text.Substring(0, Math.Min(result.Text.Length, 100))}...");
            return result.Text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Whisper Exception: {ex.Message}");
            Console.WriteLine($"   Type: {ex.GetType().Name}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            throw;
        }
    }
}

// ============================================================
// RESPONSE MODELS
// ============================================================

public class WhisperResponse
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class WhisperErrorResponse
{
    [JsonPropertyName("error")]
    public WhisperError Error { get; set; } = new();
}

public class WhisperError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}