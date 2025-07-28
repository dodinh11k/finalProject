using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace test_2.Models;
public class AppointmentEditViewModel
{
    public int AppointmentId { get; set; }

    [Required]
    public int GarageId { get; set; }

    public DateTime AppointmentTime { get; set; }

    public string? Notes { get; set; }

    public string? VehicleMake { get; set; }

    public string? VehicleModel { get; set; }

    public string? LicensePlate { get; set; }

    public List<SelectListItem> GarageList { get; set; } = new();
    public List<int> ServiceIds { get; set; } = new();
    public List<SelectListItem> ServiceList { get; set; } = new();
    public string? PromoCode { get; set; }
    public int? SelectedVehicleId { get; set; }
    public List<SelectListItem> VehicleList { get; set; } = new();
}
