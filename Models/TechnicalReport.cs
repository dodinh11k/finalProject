using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class TechnicalReport
{
    public int ReportId { get; set; }

    public int? AppointmentId { get; set; }

    public int? TechnicianId { get; set; }

    public string? VehicleStatus { get; set; }

    public string? PerformedItems { get; set; }

    public string? Recommendations { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual User? Technician { get; set; }
}
