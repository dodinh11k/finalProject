using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using test_2.Models;

public static class DropdownHelper
{
    public static async Task LoadDropdownsForMulti(MyGarageFinalContext context, MultiVehicleAppointmentViewModel model, int? userId)
    {
        model.ServiceList = await context.Services
            .Select(s => new SelectListItem
            {
                Value = s.ServiceId.ToString(),
                Text = s.ServiceName + " - " + (s.Price ?? 0).ToString("N0") + " VNĐ"
            }).ToListAsync();

        model.GarageList = await context.Garages
            .Select(g => new SelectListItem
            {
                Value = g.GarageId.ToString(),
                Text = g.Address
            }).ToListAsync();

        if (userId.HasValue)
        {
            model.VehicleList = await context.Vehicles
                .Where(v => v.UserId == userId.Value)
                .Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = $"{v.Make} {v.Model} - {v.LicensePlate}"
                }).ToListAsync();
        }

        model.VehicleMakeList = test_2.Models.VehicleData.GetVehicleMakeList();
    }
} 