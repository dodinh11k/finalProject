using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class RepairStatus
{
    public int StatusId { get; set; }

    public int? AppointmentId { get; set; }

    public string? StatusStep { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }
}
