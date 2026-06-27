
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Infrastructure.Data;
using Examify.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace YourApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PayPalOptions _options;
    private readonly ApplicationDbContext _context;

    public PaymentController(
        IHttpClientFactory httpClientFactory,
        IOptions<PayPalOptions> options,
        ApplicationDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _context = context;
    }

    [Authorize]
    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreatePayPalOrderRequest request)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(request.OrderCode))
            return BadRequest("OrderCode is required.");

        if (request.TotalAmount <= 0)
            return BadRequest("TotalAmount must be greater than 0.");

        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? "USD"
            : request.Currency.Trim().ToUpperInvariant();

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized("UserId in the token is invalid.");
        var existingOrder = await _context.Orders.FirstOrDefaultAsync(x => x.ExerciseId == request.ExerciseId && x.UserId == userId);
        if (string.Equals(existingOrder?.Status, "Complete", StringComparison.OrdinalIgnoreCase))
            return BadRequest("An order for this exercise already exists.");

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

        var order = new Order
        {
            UserId = userId,
            ExerciseId = request.ExerciseId,
            OrderCode = request.OrderCode,
            Amount = amountValue,
        };

        var Order = await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return Ok(new CreatePayPalOrderResponse
        {
            OrderCode = request.OrderCode,
            PayPalOrderId = paypalOrderId,
            Status = status,
            ApprovalUrl = approvalUrl
        });
    }

    [Authorize]
    [HttpPost("capture-order/{orderCode}")]
    public async Task<IActionResult> CaptureOrder(
    string orderCode,
    [FromBody] CapturePayPalOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(orderCode))
            return BadRequest("OrderCode is required.");

        if (string.IsNullOrWhiteSpace(request.PayPalOrderId))
            return BadRequest("PayPal order id is required.");

        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized("UserId in the token is invalid.");

        orderCode = orderCode.Trim();
        var paypalOrderId = request.PayPalOrderId.Trim();

        var order = await _context.Orders.FirstOrDefaultAsync(x =>
            x.OrderCode == orderCode &&
            x.UserId == userId);

        if (order is null)
            return NotFound("Order not found.");

        if (string.Equals(
                order.Status,
                "Complete",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Order has already been completed.");
        }

        var accessToken = await GetAccessTokenAsync();

        using var getOrderRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.BaseUrl}/v2/checkout/orders/" +
            $"{Uri.EscapeDataString(paypalOrderId)}");

        getOrderRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var getOrderResult =
            await SendPayPalRequestAsync(getOrderRequest);

        if (!getOrderResult.IsSuccess)
        {
            return StatusCode(
                getOrderResult.StatusCode,
                getOrderResult.Body);
        }

        var paypalOrderJson = JsonNode.Parse(getOrderResult.Body);

        if (paypalOrderJson is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Invalid response from PayPal.");
        }

        var paypalStatus =
            paypalOrderJson["status"]?.GetValue<string>();

        var purchaseUnit =
            paypalOrderJson["purchase_units"]?[0];

        var referenceId =
            purchaseUnit?["reference_id"]?.GetValue<string>();

        var customId =
            purchaseUnit?["custom_id"]?.GetValue<string>();

        var invoiceId =
            purchaseUnit?["invoice_id"]?.GetValue<string>();

        var paypalAmount =
            purchaseUnit?["amount"]?["value"]?.GetValue<string>();

        var orderCodeMatched =
            string.Equals(
                referenceId,
                order.OrderCode,
                StringComparison.Ordinal) &&
            string.Equals(
                customId,
                order.OrderCode,
                StringComparison.Ordinal) &&
            string.Equals(
                invoiceId,
                order.OrderCode,
                StringComparison.Ordinal);

        if (!orderCodeMatched)
        {
            return BadRequest(
                "PayPal order does not match the system order.");
        }

        var databaseAmountValid = decimal.TryParse(
            order.Amount,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var databaseAmount);

        var paypalAmountValid = decimal.TryParse(
            paypalAmount,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var actualPayPalAmount);

        if (!databaseAmountValid ||
            !paypalAmountValid ||
            databaseAmount != actualPayPalAmount)
        {
            return BadRequest(new
            {
                Message = "PayPal amount does not match the order amount.",
                OrderAmount = order.Amount,
                PayPalAmount = paypalAmount
            });
        }

        if (string.Equals(
                paypalStatus,
                "COMPLETED",
                StringComparison.OrdinalIgnoreCase))
        {
            var existingCapture =
                purchaseUnit?["payments"]?["captures"]?[0];

            var existingCaptureId =
                existingCapture?["id"]?.GetValue<string>();

            var existingCaptureStatus =
                existingCapture?["status"]?.GetValue<string>();

            if (string.Equals(
                    existingCaptureStatus,
                    "COMPLETED",
                    StringComparison.OrdinalIgnoreCase))
            {
                order.Status = "Complete";
                await _context.SaveChangesAsync();

                return Ok(new CapturePayPalOrderResponse
                {
                    PayPalOrderId = paypalOrderId,
                    Status = paypalStatus,
                    CaptureId = existingCaptureId,
                    CaptureStatus = existingCaptureStatus,
                    RawResponse = paypalOrderJson
                });
            }
        }

        if (!string.Equals(
                paypalStatus,
                "APPROVED",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                Message = "PayPal order has not been approved.",
                PayPalStatus = paypalStatus
            });
        }

        using var captureRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.BaseUrl}/v2/checkout/orders/" +
            $"{Uri.EscapeDataString(paypalOrderId)}/capture");

        captureRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        captureRequest.Headers.Add(
            "PayPal-Request-Id",
            $"capture-{paypalOrderId}");

        captureRequest.Headers.Add(
            "Prefer",
            "return=representation");

        captureRequest.Content = new StringContent(
            "{}",
            Encoding.UTF8,
            "application/json");

        var captureResult =
            await SendPayPalRequestAsync(captureRequest);

        if (!captureResult.IsSuccess)
        {
            return StatusCode(
                captureResult.StatusCode,
                captureResult.Body);
        }

        var captureJson = JsonNode.Parse(captureResult.Body);

        if (captureJson is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Invalid capture response from PayPal.");
        }

        var status =
            captureJson["status"]?.GetValue<string>();

        var capture =
            captureJson["purchase_units"]?[0]?
                ["payments"]?["captures"]?[0];

        var captureId =
            capture?["id"]?.GetValue<string>();

        var captureStatus =
            capture?["status"]?.GetValue<string>();

        var isCompleted =
            string.Equals(
                status,
                "COMPLETED",
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                captureStatus,
                "COMPLETED",
                StringComparison.OrdinalIgnoreCase);

        if (!isCompleted)
        {
            return BadRequest(new
            {
                Message = "PayPal payment was not completed.",
                Status = status,
                CaptureStatus = captureStatus
            });
        }

        order.Status = "Complete";

        await _context.SaveChangesAsync();

        return Ok(new CapturePayPalOrderResponse
        {
            PayPalOrderId = paypalOrderId,
            Status = status,
            CaptureId = captureId,
            CaptureStatus = captureStatus,
            RawResponse = captureJson
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
    public Guid ExerciseId { get; set; }
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

public class CapturePayPalOrderRequest
{
    public string PayPalOrderId { get; set; } = string.Empty;
}