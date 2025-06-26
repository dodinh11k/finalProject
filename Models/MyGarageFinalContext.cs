using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace test_2.Models;

public partial class MyGarageFinalContext : DbContext
{
    public MyGarageFinalContext()
    {
    }

    public MyGarageFinalContext(DbContextOptions<MyGarageFinalContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminActivity> AdminActivities { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<FavoriteProduct> FavoriteProducts { get; set; }

    public virtual DbSet<Garage> Garages { get; set; }

    public virtual DbSet<GarageSchedule> GarageSchedules { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<RepairStatus> RepairStatuses { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<TechnicalReport> TechnicalReports { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-QUAIVAT;Initial Catalog=MyGaragefinal;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminActivity>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK__AdminAct__45F4A7F1C03F2B94");

            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");
            entity.Property(e => e.ActionType).HasMaxLength(100);
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);

            entity.HasOne(d => d.Admin).WithMany(p => p.AdminActivities)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK__AdminActi__Admin__4E88ABD4");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCA24E07B048");

            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.AppointmentTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GarageId).HasColumnName("GarageID");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TechnicianId).HasColumnName("TechnicianID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

            entity.HasOne(d => d.Garage).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.GarageId)
                .HasConstraintName("FK__Appointme__Garag__35BCFE0A");

            entity.HasOne(d => d.Service).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__Appointme__Servi__36B12243");

            entity.HasOne(d => d.Technician).WithMany(p => p.AppointmentTechnicians)
                .HasForeignKey(d => d.TechnicianId)
                .HasConstraintName("FK__Appointme__Techn__37A5467C");

            entity.HasOne(d => d.User).WithMany(p => p.AppointmentUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Appointme__UserI__33D4B598");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__Appointme__Vehic__34C8D9D1");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B2AC6126547");

            entity.Property(e => e.CartItemId).HasColumnName("CartItemID");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__CartItems__Produ__619B8048");

            entity.HasOne(d => d.User).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__CartItems__UserI__60A75C0F");
        });

        modelBuilder.Entity<FavoriteProduct>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__Favorite__CE74FAF50F8B0462");

            entity.Property(e => e.FavoriteId).HasColumnName("FavoriteID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Product).WithMany(p => p.FavoriteProducts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__FavoriteP__Produ__66603565");

            entity.HasOne(d => d.User).WithMany(p => p.FavoriteProducts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__FavoriteP__UserI__656C112C");
        });

        modelBuilder.Entity<Garage>(entity =>
        {
            entity.HasKey(e => e.GarageId).HasName("PK__Garages__5D8BEEB11FAEADCE");

            entity.Property(e => e.GarageId).HasColumnName("GarageID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.OperatingArea).HasMaxLength(100);

            entity.HasMany(d => d.Services).WithMany(p => p.Garages)
                .UsingEntity<Dictionary<string, object>>(
                    "GarageService",
                    r => r.HasOne<Service>().WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__GarageSer__Servi__30F848ED"),
                    l => l.HasOne<Garage>().WithMany()
                        .HasForeignKey("GarageId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__GarageSer__Garag__300424B4"),
                    j =>
                    {
                        j.HasKey("GarageId", "ServiceId").HasName("PK__GarageSe__E1DA55BF5BEDA4B7");
                        j.ToTable("GarageServices");
                        j.IndexerProperty<int>("GarageId").HasColumnName("GarageID");
                        j.IndexerProperty<int>("ServiceId").HasColumnName("ServiceID");
                    });
        });

        modelBuilder.Entity<GarageSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__GarageSc__9C8A5B696CA53E76");

            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");
            entity.Property(e => e.DayOfWeek).HasMaxLength(20);
            entity.Property(e => e.GarageId).HasColumnName("GarageID");

            entity.HasOne(d => d.Garage).WithMany(p => p.GarageSchedules)
                .HasForeignKey(d => d.GarageId)
                .HasConstraintName("FK__GarageSch__Garag__52593CB8");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E32240E8BC2");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__44FF419A");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF64AD0D4A");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Orders__UserID__5812160E");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED06A1C0F3E024");

            entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderItem__Order__5CD6CB2B");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderItem__Produ__5DCAEF64");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED9922DDB2");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(100);
        });

        modelBuilder.Entity<RepairStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__RepairSt__C8EE20434823EBED");

            entity.ToTable("RepairStatus");

            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.StatusStep).HasMaxLength(100);

            entity.HasOne(d => d.Appointment).WithMany(p => p.RepairStatuses)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__RepairSta__Appoi__3B75D760");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79AE8DD62B19");

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GarageId).HasColumnName("GarageID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Garage).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.GarageId)
                .HasConstraintName("FK__Reviews__GarageI__403A8C7D");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Reviews__UserID__3F466844");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB0EA13389A71");

            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.image_url)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("image_url");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceName).HasMaxLength(100);
        });

        modelBuilder.Entity<TechnicalReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Technica__D5BD48E5D34A6A3C");

            entity.Property(e => e.ReportId).HasColumnName("ReportID");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PerformedItems).HasMaxLength(500);
            entity.Property(e => e.Recommendations).HasMaxLength(500);
            entity.Property(e => e.TechnicianId).HasColumnName("TechnicianID");
            entity.Property(e => e.VehicleStatus).HasMaxLength(255);

            entity.HasOne(d => d.Appointment).WithMany(p => p.TechnicalReports)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__Technical__Appoi__49C3F6B7");

            entity.HasOne(d => d.Technician).WithMany(p => p.TechnicalReports)
                .HasForeignKey(d => d.TechnicianId)
                .HasConstraintName("FK__Technical__Techn__4AB81AF0");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACF8972260");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E48245C43B").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Amount)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__476B54B25A29C679");

            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.Make).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Vehicles__UserID__29572725");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
