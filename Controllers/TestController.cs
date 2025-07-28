using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test_2.Models;

namespace test_2.Controllers
{
    [Route("test")]
    public class TestController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public TestController(MyGarageFinalContext context)
        {
            _context = context;
        }

        [HttpGet("add-vouchers")]
        public async Task<IActionResult> AddTestVouchers()
        {
            try
            {
                // Check if vouchers already exist
                var existingVouchers = await _context.PromoCodes.ToListAsync();
                if (existingVouchers.Any())
                {
                    return Content("Vouchers already exist in database.");
                }

                // Add test vouchers
                var vouchers = new List<PromoCode>
                {
                    new PromoCode
                    {
                        Code = "SAVE10",
                        DiscountPercent = 10,
                        ExpiryDate = DateTime.Now.AddMonths(3),
                        Description = "Giảm 10% cho tất cả dịch vụ"
                    },
                    new PromoCode
                    {
                        Code = "SAVE50K",
                        DiscountAmount = 50000,
                        ExpiryDate = DateTime.Now.AddMonths(2),
                        Description = "Giảm 50,000 VNĐ cho đơn hàng từ 200,000 VNĐ"
                    },
                    new PromoCode
                    {
                        Code = "WELCOME20",
                        DiscountPercent = 20,
                        ExpiryDate = DateTime.Now.AddMonths(1),
                        Description = "Giảm 20% cho khách hàng mới"
                    },
                    new PromoCode
                    {
                        Code = "FIXED30K",
                        DiscountAmount = 30000,
                        ExpiryDate = DateTime.Now.AddDays(30),
                        Description = "Giảm cố định 30,000 VNĐ"
                    }
                };

                _context.PromoCodes.AddRange(vouchers);
                await _context.SaveChangesAsync();

                return Content("Test vouchers added successfully!<br><br>" +
                    "Available vouchers:<br>" +
                    "- SAVE10: 10% discount<br>" +
                    "- SAVE50K: 50,000 VNĐ discount<br>" +
                    "- WELCOME20: 20% discount<br>" +
                    "- FIXED30K: 30,000 VNĐ discount<br><br>" +
                    "You can now test the voucher system by creating appointments.");
            }
            catch (Exception ex)
            {
                return Content($"Error adding vouchers: {ex.Message}");
            }
        }

        [HttpGet("list-vouchers")]
        public async Task<IActionResult> ListVouchers()
        {
            var vouchers = await _context.PromoCodes.ToListAsync();
            
            var html = "<h3>Available Vouchers:</h3><ul>";
            foreach (var voucher in vouchers)
            {
                var discountInfo = voucher.DiscountPercent.HasValue 
                    ? $"{voucher.DiscountPercent}%" 
                    : $"{voucher.DiscountAmount:N0} VNĐ";
                
                var expiryInfo = voucher.ExpiryDate.HasValue 
                    ? voucher.ExpiryDate.Value.ToString("dd/MM/yyyy") 
                    : "No expiry";
                
                html += $"<li><strong>{voucher.Code}</strong>: {discountInfo} - {voucher.Description} (Expires: {expiryInfo})</li>";
            }
            html += "</ul>";
            
            return Content(html, "text/html");
        }
    }
} 