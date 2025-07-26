using System;
using System.Collections.Generic;

namespace test_2.Models
{
    public class AppointmentDetailsViewModel
    {
        public Appointment Appointment { get; set; }
        public List<AppointmentVehicleDetail> VehicleDetails { get; set; } = new();
        public decimal TotalServicePrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<(string ServiceName, decimal Price)> ServicePriceDetails { get; set; } = new();
        public List<(string DiscountType, decimal Amount)> DiscountDetails { get; set; } = new();
    }
}
