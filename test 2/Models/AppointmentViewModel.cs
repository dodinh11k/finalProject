using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace test_2.Models
{
    public class AppointmentViewModel
    {
        [Required]
        public string CustomerName { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int GarageId { get; set; }

        [Required]
        public DateTime AppointmentTime { get; set; }

        public string? Notes { get; set; }

        public List<SelectListItem> ServiceList { get; set; } = new();
        public List<SelectListItem> GarageList { get; set; } = new();
    }
}
