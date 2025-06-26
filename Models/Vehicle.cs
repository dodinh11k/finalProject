using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class Vehicle
{
    public int VehicleId { get; set; }

    public int? UserId { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }

    public string? LicensePlate { get; set; }

    public int? Year { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User? User { get; set; }
}
