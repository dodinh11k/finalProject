using System;

namespace test_2.Models
{
    public class AppointmentProductDetail
    {
        
        public int AppointmentProductDetailId { get; set; }
        public int AppointmentId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public virtual Appointment Appointment { get; set; }
        public virtual Product Product { get; set; }
    }
} 