using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int? UserId { get; set; }

    public int? GarageId { get; set; }

    public int? TechnicianId { get; set; }

    public DateTime? AppointmentTime { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AppointmentVehicleDetail> AppointmentVehicleDetails { get; set; } = new List<AppointmentVehicleDetail>();

    public virtual Garage? Garage { get; set; }

    public virtual ICollection<RepairStatus> RepairStatuses { get; set; } = new List<RepairStatus>();

    public virtual ICollection<TechnicalReport> TechnicalReports { get; set; } = new List<TechnicalReport>();

    public virtual User? Technician { get; set; }

    public virtual User? User { get; set; }
}
