using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace test_2.Models
{
    public class AppointmentViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một dịch vụ.")]
        public List<int> ServiceIds { get; set; } = new();  // Tương ứng nhiều service → AppointmentVehicleDetails

        [Required(ErrorMessage = "Garage không được bỏ trống.")]
        public int GarageId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian hẹn.")]
        public DateTime AppointmentTime { get; set; }

        public string? Notes { get; set; }

        // Thông tin xe mới nếu người dùng không có xe trước đó
        public string? VehicleMake { get; set; }

        public string? VehicleModel { get; set; }

        public string? LicensePlate { get; set; }

        // Dành cho trường hợp user chọn xe có sẵn (cần nếu user có nhiều xe)
        public int? SelectedVehicleId { get; set; }

        public bool IsExistingUser { get; set; } = false;

        public string? PromoCode { get; set; }

        // Dropdowns
        public List<SelectListItem> ServiceList { get; set; } = new();
        public List<SelectListItem> GarageList { get; set; } = new();
        public List<SelectListItem> VehicleList { get; set; } = new();  // nếu dùng chọn xe cũ
    }
}
