using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? UserId { get; set; }

    public int? GarageId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }
    public int AppointmentId { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual Garage? Garage { get; set; }

    public virtual User? User { get; set; }
}
