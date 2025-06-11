using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public int? GarageId { get; set; }

    public string? ServiceName { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    // Thêm thuộc tính lưu URL hoặc đường dẫn ảnh
    public string? image_url { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Garage? Garage { get; set; }
}
