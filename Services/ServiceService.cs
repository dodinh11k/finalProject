using test_2.Models;
using Microsoft.EntityFrameworkCore;

namespace test_2.Services
{
    public class ServiceService : IServiceService
    {
        private readonly MyGarageFinalContext _context;

        public ServiceService(MyGarageFinalContext context)
        {
            _context = context;
        }

        public async Task<List<Service>> GetAllAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            return await _context.Services.FindAsync(id);
        }

        public async Task CreateAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Service service)
        {
            var existing = await _context.Services.FindAsync(service.ServiceId);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(service);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ServiceWithReviewViewModel>> GetAllWithReviewsAsync()
        {
            var services = await _context.Services.ToListAsync();
            var details = await _context.AppointmentVehicleDetails.ToListAsync();
            var reviews = await _context.Reviews.ToListAsync();

            var result = services.Select(s => {
                // Lấy tất cả appointmentId có serviceId này
                var appointmentIds = details.Where(d => d.ServiceId == s.ServiceId).Select(d => d.AppointmentId).Distinct().ToList();
                // Lấy tất cả review cho các appointment này
                var serviceReviews = reviews.Where(r => appointmentIds.Contains(r.AppointmentId) && r.Rating.HasValue).ToList();
                double? avg = serviceReviews.Any() ? serviceReviews.Average(r => r.Rating.Value) : (double?)null;
                int count = serviceReviews.Count;
                return new ServiceWithReviewViewModel {
                    Service = s,
                    AverageRating = avg,
                    ReviewCount = count
                };
            }).ToList();
            return result;
        }
    }
}
