using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class GarageSchedule
{
    public int ScheduleId { get; set; }

    public int? GarageId { get; set; }

    public string? DayOfWeek { get; set; }

    public TimeOnly? OpenTime { get; set; }

    public TimeOnly? CloseTime { get; set; }

    public virtual Garage? Garage { get; set; }
}
