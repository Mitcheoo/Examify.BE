// Examify.Infrastructure/Services/PayPalService.cs
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace Examify.Infrastructure.Services;

public class PayPalOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public class PayPalService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PayPalOptions _options;

    // Tỷ giá cố định cho test: 1 USD = 25,000 VND
    private const decimal USD_TO_VND_RATE = 25000m;

    public PayPalService(IHttpClientFactory httpClientFactory, IOptions<PayPalOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public decimal ConvertVNDToUSD(decimal vndAmount)
    {
        return Math.Round(vndAmount / USD_TO_VND_RATE, 2);
    }

    public decimal ConvertUSDToVND(decimal usdAmount)
    {
        return Math.Round(usdAmount * USD_TO_VND_RATE, 0);
    }

    public async Task<string> GetAccessTokenAsync()
    {
        using var httpClient = _httpClientFactory.CreateClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.BaseUrl}/v1/oauth2/token");

        var rawCredential = $"{_options.ClientId}:{_options.ClientSecret}";
        var base64Credential = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(rawCredential));

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", base64Credential);

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials"
        });

        using var response = await httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"PayPal auth failed: {body}");

        var json = JsonNode.Parse(body);
        var accessToken = json?["access_token"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException("PayPal access token not found.");

        return accessToken;
    }

    public async Task<(bool IsSuccess, string? PayPalOrderId, string? ApprovalUrl, string? Status, string? Error)>
        CreateOrderAsync(string orderCode, decimal amountUSD, string currency = "USD")
    {
        var accessToken = await GetAccessTokenAsync();

        var payload = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = orderCode,
                    custom_id = orderCode,
                    invoice_id = orderCode,
                    amount = new
                    {
                        currency_code = currency,
                        value = amountUSD.ToString("0.00", CultureInfo.InvariantCulture)
                    }
                }
            },
            application_context = new
            {
                return_url = $"{_options.ReturnUrl}?orderCode={Uri.EscapeDataString(orderCode)}",
                cancel_url = $"{_options.CancelUrl}?orderCode={Uri.EscapeDataString(orderCode)}",
                user_action = "PAY_NOW"
            }
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.BaseUrl}/v2/checkout/orders");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        httpRequest.Headers.Add("PayPal-Request-Id", $"create-{orderCode}");
        httpRequest.Headers.Add("Prefer", "return=representation");

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(httpRequest);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return (false, null, null, null, body);
        }

        var json = JsonNode.Parse(body);
        var paypalOrderId = json?["id"]?.GetValue<string>();
        var status = json?["status"]?.GetValue<string>();

        var approvalUrl = json?["links"]?
            .AsArray()
            .FirstOrDefault(x =>
                string.Equals(
                    x?["rel"]?.GetValue<string>(),
                    "approve",
                    StringComparison.OrdinalIgnoreCase))
            ?["href"]?.GetValue<string>();

        return (true, paypalOrderId, approvalUrl, status, null);
    }

    public async Task<(bool IsSuccess, string? Status, string? CaptureId, string? CaptureStatus, string? Error)>
        CaptureOrderAsync(string paypalOrderId)
    {
        var accessToken = await GetAccessTokenAsync();

        // Kiểm tra trạng thái order trước khi capture
        using var getOrderRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.BaseUrl}/v2/checkout/orders/{Uri.EscapeDataString(paypalOrderId)}");

        getOrderRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var httpClient = _httpClientFactory.CreateClient();
        var getOrderResponse = await httpClient.SendAsync(getOrderRequest);
        var getOrderBody = await getOrderResponse.Content.ReadAsStringAsync();

        if (!getOrderResponse.IsSuccessStatusCode)
        {
            return (false, null, null, null, getOrderBody);
        }

        var orderJson = JsonNode.Parse(getOrderBody);
        var orderStatus = orderJson?["status"]?.GetValue<string>();

        // Nếu đã hoàn thành thì trả về luôn
        if (string.Equals(orderStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            var existingCapture = orderJson?["purchase_units"]?[0]?["payments"]?["captures"]?[0];
            var existingCaptureId = existingCapture?["id"]?.GetValue<string>();
            var existingCaptureStatus = existingCapture?["status"]?.GetValue<string>();

            return (true, "COMPLETED", existingCaptureId, existingCaptureStatus, null);
        }

        // Nếu chưa approved thì không thể capture
        if (!string.Equals(orderStatus, "APPROVED", StringComparison.OrdinalIgnoreCase))
        {
            return (false, orderStatus, null, null, $"Order status is {orderStatus}, cannot capture");
        }

        // Capture order
        using var captureRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.BaseUrl}/v2/checkout/orders/{Uri.EscapeDataString(paypalOrderId)}/capture");

        captureRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        captureRequest.Headers.Add("PayPal-Request-Id", $"capture-{paypalOrderId}");
        captureRequest.Headers.Add("Prefer", "return=representation");

        captureRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        var captureResponse = await httpClient.SendAsync(captureRequest);
        var captureBody = await captureResponse.Content.ReadAsStringAsync();

        if (!captureResponse.IsSuccessStatusCode)
        {
            return (false, null, null, null, captureBody);
        }

        var captureJson = JsonNode.Parse(captureBody);
        var status = captureJson?["status"]?.GetValue<string>();
        var capture = captureJson?["purchase_units"]?[0]?["payments"]?["captures"]?[0];
        var newCaptureId = capture?["id"]?.GetValue<string>();
        var newCaptureStatus = capture?["status"]?.GetValue<string>();

        return (true, status, newCaptureId, newCaptureStatus, null);
    }
    public async Task<(bool IsSuccess, string? Status, string? Error)> GetOrderStatusAsync(string paypalOrderId)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_options.BaseUrl}/v2/checkout/orders/{Uri.EscapeDataString(paypalOrderId)}");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return (false, null, body);
            }

            var json = JsonNode.Parse(body);
            var status = json?["status"]?.GetValue<string>();

            return (true, status, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }
    public bool IsSandbox()
    {
        return _options.BaseUrl.Contains("sandbox");
    }
}