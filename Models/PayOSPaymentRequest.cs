using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace test_2.Services
{
    public class PayOSPaymentRequest
    {
        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }
        [JsonPropertyName("amount")]
        public long Amount { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
        [JsonPropertyName("cancelUrl")]
        public string CancelUrl { get; set; } = "";
        [JsonPropertyName("returnUrl")]
        public string ReturnUrl { get; set; } = "";
        [JsonPropertyName("items")]
        public List<PayOSPaymentItem> Items { get; set; } = new();
        [JsonPropertyName("buyerName")]
        public string BuyerName { get; set; } = "";
        [JsonPropertyName("buyerEmail")]
        public string BuyerEmail { get; set; } = "";
        [JsonPropertyName("buyerPhone")]
        public string BuyerPhone { get; set; } = "";
        [JsonPropertyName("buyerAddress")]
        public string BuyerAddress { get; set; } = "";
    }

    public class PayOSPaymentItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("price")]
        public long Price { get; set; }
    }
} 