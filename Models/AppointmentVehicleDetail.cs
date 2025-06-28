using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class AppointmentVehicleDetail
{
    public int AppointmentVehicleDetailId { get; set; }

    public int AppointmentId { get; set; }

    public int? VehicleId { get; set; }

    public int? ServiceId { get; set; }

    public int? Quantity { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual Service? Service { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
