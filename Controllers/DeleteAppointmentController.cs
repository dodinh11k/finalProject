//using Microsoft.AspNetCore.Mvc;
//using test_2.Models; // namespace chứa model Service
//using Microsoft.EntityFrameworkCore;

//namespace test_2.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ServiceController : ControllerBase
//    {
//        private readonly MyGarageFinalContext _context;

//        public ServiceController(MyGarageFinalContext context)
//        {
//            _context = context;
//        }

//        // POST: api/Service
//        [HttpPost]
//        public async Task<IActionResult> CreateService([FromBody] Service service)
//        {
//            if (service == null || string.IsNullOrWhiteSpace(service.ServiceName))
//            {
//                return BadRequest("Thông tin dịch vụ không hợp lệ.");
//            }

//            _context.Services.Add(service);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetServiceById), new { id = service.ServiceId }, service);
//        }

//        // GET: api/Service/5
//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetServiceById(int id)
//        {
//            var service = await _context.Services.FindAsync(id);
//            if (service == null) return NotFound();
//            return Ok(service);
//        }
//    }
//}
