
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace YourApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PayPalOptions _options;

    public PaymentController(
        IHttpClientFactory httpClientFactory,
        IOptions<PayPalOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreatePayPalOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OrderCode))
            return BadRequest("OrderCode is required.");

        if (request.TotalAmount <= 0)
            return BadRequest("TotalAmount must be greater than 0.");

        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? "USD"
            : request.Currency.Trim().ToUpperInvariant();

        var accessToken = await GetAccessTokenAsync();

        var amountValue = request.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture);

        var payload = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = request.OrderCode,
                    custom_id = request.OrderCode,
                    invoice_id = request.OrderCode,
                    amount = new
                    {
                        currency_code = currency,
                        value = amountValue
                    }
                }
            },
            application_context = new
            {
                return_url = $"{_options.ReturnUrl}?orderCode={Uri.EscapeDataString(request.OrderCode)}",
                cancel_url = $"{_options.CancelUrl}?orderCode={Uri.EscapeDataString(request.OrderCode)}",
                user_action = "PAY_NOW"
            }
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.BaseUrl}/v2/checkout/orders");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        httpRequest.Headers.Add("PayPal-Request-Id", $"create-{request.OrderCode}");
        httpRequest.Headers.Add("Prefer", "return=representation");

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var result = await SendPayPalRequestAsync(httpRequest);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Body);

        var json = JsonNode.Parse(result.Body);

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

        return Ok(new CreatePayPalOrderResponse
        {
            OrderCode = request.OrderCode,
            PayPalOrderId = paypalOrderId,
            Status = status,
            ApprovalUrl = approvalUrl
        });
    }

    [HttpPost("capture-order/{paypalOrderId}")]
    public async Task<IActionResult> CaptureOrder(string paypalOrderId)
    {
        if (string.IsNullOrWhiteSpace(paypalOrderId))
            return BadRequest("PayPal order id is required.");

        var accessToken = await GetAccessTokenAsync();

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.BaseUrl}/v2/checkout/orders/{paypalOrderId}/capture");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        httpRequest.Headers.Add("PayPal-Request-Id", $"capture-{paypalOrderId}");
        httpRequest.Headers.Add("Prefer", "return=representation");

        httpRequest.Content = new StringContent(
            "{}",
            Encoding.UTF8,
            "application/json");

        var result = await SendPayPalRequestAsync(httpRequest);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Body);

        var json = JsonNode.Parse(result.Body);

        var status = json?["status"]?.GetValue<string>();

        var capture = json?["purchase_units"]?[0]?["payments"]?["captures"]?[0];
        var captureId = capture?["id"]?.GetValue<string>();
        var captureStatus = capture?["status"]?.GetValue<string>();

        return Ok(new CapturePayPalOrderResponse
        {
            PayPalOrderId = paypalOrderId,
            Status = status,
            CaptureId = captureId,
            CaptureStatus = captureStatus,
            RawResponse = json
        });
    }

    private async Task<string> GetAccessTokenAsync()
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

    private async Task<PayPalHttpResult> SendPayPalRequestAsync(HttpRequestMessage request)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request);

        var body = await response.Content.ReadAsStringAsync();

        return new PayPalHttpResult
        {
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Body = body
        };
    }
}

public class PayPalOptions
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";
    public string ReturnUrl { get; set; } = "";
    public string CancelUrl { get; set; } = "";
}

public class CreatePayPalOrderRequest
{
    public string OrderCode { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
}

public class CreatePayPalOrderResponse
{
    public string OrderCode { get; set; } = "";
    public string? PayPalOrderId { get; set; }
    public string? Status { get; set; }
    public string? ApprovalUrl { get; set; }
}

public class CapturePayPalOrderResponse
{
    public string PayPalOrderId { get; set; } = "";
    public string? Status { get; set; }
    public string? CaptureId { get; set; }
    public string? CaptureStatus { get; set; }
    public JsonNode? RawResponse { get; set; }
}

public class PayPalHttpResult
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Body { get; set; } = "";
}