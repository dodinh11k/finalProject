using System;
using System.Collections.Generic;

namespace test_2.Models
{
    public class AppointmentDetailsViewModel
    {
        public Appointment Appointment { get; set; }
        public List<AppointmentVehicleDetail> VehicleDetails { get; set; } = new();
    }
}
