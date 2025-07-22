using test_2.Models;
namespace test_2.Models;
public class AppointmentHistoryViewModel
{
    public List<Appointment> Appointments { get; set; }
    public List<AppointmentVehicleDetail> Details { get; set; }
    public List<Review> Reviews { get; set; }
}
