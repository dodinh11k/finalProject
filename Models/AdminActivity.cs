using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class AdminActivity
{
    public int ActivityId { get; set; }

    public int? AdminId { get; set; }

    public string? ActionType { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? Admin { get; set; }
}
