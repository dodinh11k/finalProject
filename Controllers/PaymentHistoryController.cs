using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test_2.Services;

namespace test_2.Controllers
{
    [Authorize]
    [Route("payment-history")]
    public class PaymentHistoryController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentHistoryController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var payments = await _paymentService.GetUserPaymentHistoryAsync(userId.Value);
            return View(payments);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserID");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
} 