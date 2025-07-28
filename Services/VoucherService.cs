using Microsoft.EntityFrameworkCore;
using test_2.Models;

namespace test_2.Services
{
    public interface IVoucherService
    {
        Task<(bool IsValid, string Message, decimal DiscountAmount)> ValidateVoucherAsync(string promoCode, decimal originalAmount);
        Task<decimal> CalculateDiscountedAmountAsync(string promoCode, decimal originalAmount);
    }

    public class VoucherService : IVoucherService
    {
        private readonly MyGarageFinalContext _context;

        public VoucherService(MyGarageFinalContext context)
        {
            _context = context;
        }

        public async Task<(bool IsValid, string Message, decimal DiscountAmount)> ValidateVoucherAsync(string promoCode, decimal originalAmount)
        {
            if (string.IsNullOrWhiteSpace(promoCode))
            {
                return (false, "Mã khuyến mãi không được để trống", 0);
            }

            var voucher = await _context.PromoCodes
                .FirstOrDefaultAsync(p => p.Code.ToUpper() == promoCode.ToUpper());

            if (voucher == null)
            {
                return (false, "Mã khuyến mãi không tồn tại", 0);
            }

            // Kiểm tra ngày hết hạn
            if (voucher.ExpiryDate.HasValue && voucher.ExpiryDate.Value < DateTime.Now)
            {
                return (false, "Mã khuyến mãi đã hết hạn", 0);
            }

            // Tính toán số tiền giảm
            decimal discountAmount = 0;
            if (voucher.DiscountPercent.HasValue)
            {
                discountAmount = originalAmount * (decimal)(voucher.DiscountPercent.Value / 100);
            }
            else if (voucher.DiscountAmount.HasValue)
            {
                discountAmount = voucher.DiscountAmount.Value;
            }

            // Đảm bảo không giảm quá số tiền gốc
            if (discountAmount > originalAmount)
            {
                discountAmount = originalAmount;
            }

            string message = voucher.DiscountPercent.HasValue 
                ? $"Áp dụng giảm {voucher.DiscountPercent}%"
                : $"Áp dụng giảm {voucher.DiscountAmount:N0} VNĐ";

            return (true, message, discountAmount);
        }

        public async Task<decimal> CalculateDiscountedAmountAsync(string promoCode, decimal originalAmount)
        {
            var (isValid, _, discountAmount) = await ValidateVoucherAsync(promoCode, originalAmount);
            
            if (!isValid)
            {
                return originalAmount; // Trả về số tiền gốc nếu voucher không hợp lệ
            }

            return originalAmount - discountAmount;
        }
    }
} 