using System;
using System.Collections.Generic;

namespace test_2.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? PasswordHash { get; set; }


    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public decimal? Amount { get; set; }

    public virtual ICollection<AdminActivity> AdminActivities { get; set; } = new List<AdminActivity>();

    public virtual ICollection<Appointment> AppointmentTechnicians { get; set; } = new List<Appointment>();

    public virtual ICollection<Appointment> AppointmentUsers { get; set; } = new List<Appointment>();

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<FavoriteProduct> FavoriteProducts { get; set; } = new List<FavoriteProduct>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<TechnicalReport> TechnicalReports { get; set; } = new List<TechnicalReport>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
