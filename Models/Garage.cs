using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class Garage
{
    public int GarageId { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? OperatingArea { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<GarageSchedule> GarageSchedules { get; set; } = new List<GarageSchedule>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
