namespace test_2.Models
{
    public class AppointmentViewModel
    {
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public int ServiceId { get; set; }
        public int GarageId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string Notes { get; set; }
    }
}
