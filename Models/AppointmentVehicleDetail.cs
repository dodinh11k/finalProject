using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace test_2.Models
{
    public partial class AppointmentVehicleDetail
    {
        public int AppointmentVehicleDetailId { get; set; }

        public int AppointmentId { get; set; }


        public int? VehicleId { get; set; }

        public int? ServiceId { get; set; }

        public int? TechnicianId { get; set; }  // 🔧 Bổ sung để ánh xạ đúng với DB

        public int? Quantity { get; set; }

        public string? Note { get; set; }

        public DateTime? CreatedAt { get; set; }

        public virtual Appointment? Appointment { get; set; }

        public virtual Service? Service { get; set; }

        public virtual Vehicle? Vehicle { get; set; }
        [ForeignKey("TechnicianId")]  // ✅ RẤT QUAN TRỌNG nếu bạn KHÔNG dùng Fluent API
      
        public virtual User? Technician { get; set; }  // 🔧 Thêm navigation để truy xuất thông tin thợ máy
    }
}
