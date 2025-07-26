using System;

namespace test_2.Models
{
    public class PromoCode
    {
        public int PromoCodeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal? DiscountAmount { get; set; } // Giảm cố định
        public double? DiscountPercent { get; set; } // Giảm theo %
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
    }
} 