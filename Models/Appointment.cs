using System;
using System.Collections.Generic;

namespace test_2.Models
{
    public partial class Appointment
    {
        public int AppointmentId { get; set; }

        public int? UserId { get; set; }

        public int? GarageId { get; set; }

        public DateTime? AppointmentTime { get; set; }

        public string? Status { get; set; }

        public string? Notes { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? PromoCode { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TotalAmount { get; set; }

        // 👇 Quan hệ: Mỗi lịch hẹn có thể gồm nhiều chi tiết (xe + dịch vụ + thợ)
        public virtual ICollection<AppointmentVehicleDetail> AppointmentVehicleDetails { get; set; } = new List<AppointmentVehicleDetail>();

        public virtual Garage? Garage { get; set; }

        public virtual User? User { get; set; }

        // 👇 Các bảng liên kết không cần thay đổi
        public virtual ICollection<RepairStatus> RepairStatuses { get; set; } = new List<RepairStatus>();

        public virtual ICollection<TechnicalReport> TechnicalReports { get; set; } = new List<TechnicalReport>();
    }
}
