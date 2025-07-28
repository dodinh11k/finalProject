using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace test_2.Services
{
    public class PayOSService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _apiKey;
        private readonly string _checksumKey;

        public PayOSService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _clientId = configuration["PAYOS_CLIENT_ID"] ?? throw new InvalidOperationException("PayOS ClientId not configured");
            _apiKey = configuration["PAYOS_API_KEY"] ?? throw new InvalidOperationException("PayOS ApiKey not configured");
            _checksumKey = configuration["PAYOS_CHECKSUM_KEY"] ?? throw new InvalidOperationException("PayOS ChecksumKey not configured");
            
            // Sử dụng PayOS Production environment
            _httpClient.BaseAddress = new Uri("https://api-merchant.payos.vn/");
            _httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyGarageApp/1.0");
        }

        public async Task<PayOSPaymentResponse?> CreatePaymentRequest(PayOSPaymentRequest request)
        {
            var signature = GenerateSignature(request);

            var payOSRequest = new
            {
                orderCode = request.OrderCode,
                amount = request.Amount,
                description = request.Description,
                cancelUrl = request.CancelUrl,
                returnUrl = request.ReturnUrl,
                signature = signature,
                items = request.Items,
                buyerName = request.BuyerName,
                buyerEmail = request.BuyerEmail,
                buyerPhone = request.BuyerPhone,
                buyerAddress = request.BuyerAddress,
                expiredAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
            };

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var jsonString = JsonSerializer.Serialize(payOSRequest, options);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var domain = "https://api-merchant.payos.vn/";
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-client-id", _clientId);
                client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                client.DefaultRequestHeaders.Add("User-Agent", "MyGarageApp/1.0");
                client.Timeout = TimeSpan.FromSeconds(30);

                var response = await client.PostAsync($"{domain}v2/payment-requests", content);
                var responseString = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📥 PayOS Response from {domain}: {response.StatusCode} - {responseString}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<PayOSApiResponse>(responseString);
                    Console.WriteLine($"✅ Success with domain: {domain}");
                    return result?.Data;
                }
                else
                {
                    throw new Exception($"PayOS API Error: {response.StatusCode} - {responseString}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ PayOS Service Error: {ex.Message}");
                throw;
            }
        }

        public bool VerifyWebhookData(string body, string signature)
        {
            try
            {
                // Verify webhook signature
                var expectedSignature = GenerateWebhookSignature(body);
                return signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Webhook verification error: {ex.Message}");
                return false;
            }
        }

        private string GenerateSignature(PayOSPaymentRequest request)
        {
            // Build dictionary of fields
            var dict = new Dictionary<string, object>
            {
                {"amount", request.Amount},
                {"cancelUrl", request.CancelUrl ?? string.Empty},
                {"description", request.Description ?? string.Empty},
                {"orderCode", request.OrderCode},
                {"returnUrl", request.ReturnUrl ?? string.Empty}
            };

            // Sort keys alphabetically
            var sorted = dict.OrderBy(kv => kv.Key, StringComparer.Ordinal);
            var raw = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_checksumKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(raw));
            // Output as lowercase hex string (PayOS expects hex, not base64)
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private string GenerateWebhookSignature(string body)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_checksumKey);
            var dataBytes = Encoding.UTF8.GetBytes(body);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }

    public class PayOSPaymentResponse
    {
        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }
        [JsonPropertyName("checkoutUrl")]
        public string CheckoutUrl { get; set; } = "";
        [JsonPropertyName("qrCode")]
        public string QrCode { get; set; } = "";
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
        // Thêm các trường khác nếu cần (bin, accountNumber, ...), nhưng 4 trường trên là đủ cho redirect.
    }

    public class PayOSApiResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";
        [JsonPropertyName("desc")]
        public string Message { get; set; } = "";
        [JsonPropertyName("data")]
        public PayOSPaymentResponse? Data { get; set; }
    }
} 