using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace test_2.Models
{
    public class AppointmentEditViewModel
    {
        public int AppointmentId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int GarageId { get; set; }

        public DateTime AppointmentTime { get; set; }

        public string? Notes { get; set; }

        public string? VehicleMake { get; set; }

        public string? VehicleModel { get; set; }

        public string? LicensePlate { get; set; }

        public List<SelectListItem> ServiceList { get; set; } = new();
        public List<SelectListItem> GarageList { get; set; } = new();
    }

}
