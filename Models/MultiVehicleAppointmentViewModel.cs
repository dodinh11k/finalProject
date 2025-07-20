using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace test_2.Models
{
    public class MultiVehicleAppointmentViewModel
    {
        [Required(ErrorMessage = "Garage không được bỏ trống.")]
        public int GarageId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian hẹn.")]
        public DateTime AppointmentTime { get; set; }

        public string? Notes { get; set; }

        // Danh sách các xe cần đặt lịch
        [Required(ErrorMessage = "Vui lòng thêm ít nhất một xe.")]
        [MinLength(1, ErrorMessage = "Vui lòng thêm ít nhất một xe.")]
        public List<VehicleAppointmentItem> Vehicles { get; set; } = new List<VehicleAppointmentItem>();

        // Dropdowns
        public List<SelectListItem> GarageList { get; set; } = new();
        public List<SelectListItem> ServiceList { get; set; } = new();
        public List<SelectListItem> VehicleList { get; set; } = new();
        public List<SelectListItem> VehicleMakeList { get; set; } = new();
        public Dictionary<string, List<SelectListItem>> VehicleModelList { get; set; } = new();
    }

    public class VehicleAppointmentItem
    {
        // Thông tin xe mới
        public string? VehicleMake { get; set; }
        public string? VehicleModel { get; set; }
        public string? LicensePlate { get; set; }

        // Hoặc chọn xe có sẵn
        public int? SelectedVehicleId { get; set; }

        // Dịch vụ cho xe này
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một dịch vụ cho xe này.")]
        public List<int> ServiceIds { get; set; } = new();

        // Ghi chú riêng cho xe này
        public string? VehicleNotes { get; set; }
    }

    public static class VehicleData
    {
        public static readonly Dictionary<string, List<string>> VehicleMakesAndModels = new()
        {
            ["Toyota"] = new List<string>
            {
                "Camry", "Corolla", "Prius", "RAV4", "Highlander", "Tacoma", "Tundra", "Sienna", "Avalon", "C-HR", "Venza", "Sequoia"
            },
            ["Honda"] = new List<string>
            {
                "Civic", "Accord", "CR-V", "Pilot", "Odyssey", "HR-V", "Passport", "Ridgeline", "Insight", "Clarity", "Fit", "Element"
            },
            ["Ford"] = new List<string>
            {
                "F-150", "Mustang", "Explorer", "Escape", "Edge", "Ranger", "Bronco", "Expedition", "Fusion", "Focus", "Transit", "EcoSport"
            },
            ["Chevrolet"] = new List<string>
            {
                "Silverado", "Camaro", "Equinox", "Tahoe", "Suburban", "Colorado", "Traverse", "Malibu", "Cruze", "Spark", "Bolt", "Corvette"
            },
            ["Nissan"] = new List<string>
            {
                "Altima", "Sentra", "Rogue", "Murano", "Pathfinder", "Frontier", "Titan", "Maxima", "Versa", "Kicks", "Armada", "Leaf"
            },
            ["BMW"] = new List<string>
            {
                "3 Series", "5 Series", "7 Series", "X1", "X3", "X5", "X7", "1 Series", "2 Series", "4 Series", "6 Series", "8 Series", "Z4", "i3", "i4", "iX"
            },
            ["Mercedes-Benz"] = new List<string>
            {
                "A-Class", "C-Class", "E-Class", "S-Class", "GLA", "GLC", "GLE", "GLS", "CLA", "CLS", "AMG GT", "EQS", "EQE", "Sprinter"
            },
            ["Audi"] = new List<string>
            {
                "A3", "A4", "A6", "A8", "Q3", "Q5", "Q7", "Q8", "TT", "RS", "e-tron", "e-tron GT", "S3", "S4", "S6", "S8"
            },
            ["Volkswagen"] = new List<string>
            {
                "Golf", "Passat", "Jetta", "Tiguan", "Atlas", "ID.4", "ID.3", "Arteon", "Taos", "Touareg", "Polo", "T-Cross"
            },
            ["Hyundai"] = new List<string>
            {
                "Elantra", "Sonata", "Tucson", "Santa Fe", "Palisade", "Kona", "Venue", "Accent", "Veloster", "IONIQ", "Nexo", "Staria"
            },
            ["Kia"] = new List<string>
            {
                "Forte", "K5", "Sportage", "Sorento", "Telluride", "Soul", "Rio", "Stinger", "EV6", "Niro", "Carnival", "Seltos"
            },
            ["Mazda"] = new List<string>
            {
                "Mazda3", "Mazda6", "CX-30", "CX-5", "CX-9", "MX-5", "CX-3", "MX-30", "CX-50", "CX-60", "CX-90"
            },
            ["Subaru"] = new List<string>
            {
                "Impreza", "Legacy", "Outback", "Forester", "Crosstrek", "Ascent", "BRZ", "WRX", "XV"
            },
            ["Lexus"] = new List<string>
            {
                "ES", "IS", "LS", "GS", "LC", "RC", "UX", "NX", "RX", "GX", "LX", "LFA"
            },
            ["Acura"] = new List<string>
            {
                "ILX", "TLX", "RLX", "RDX", "MDX", "NSX", "Integra", "ZDX"
            },
            ["Infiniti"] = new List<string>
            {
                "Q50", "Q60", "Q70", "QX30", "QX50", "QX60", "QX80", "Q30", "Q40"
            },
            ["Volvo"] = new List<string>
            {
                "S60", "S90", "V60", "V90", "XC40", "XC60", "XC90", "C40", "Polestar"
            },
            ["Jaguar"] = new List<string>
            {
                "XE", "XF", "XJ", "F-Type", "E-Pace", "F-Pace", "I-Pace", "F-Pace SVR"
            },
            ["Land Rover"] = new List<string>
            {
                "Range Rover", "Range Rover Sport", "Range Rover Velar", "Range Rover Evoque", "Discovery", "Discovery Sport", "Defender"
            },
            ["Porsche"] = new List<string>
            {
                "911", "Cayman", "Boxster", "Cayenne", "Macan", "Panamera", "Taycan", "Carrera", "Turbo"
            },
            ["Tesla"] = new List<string>
            {
                "Model S", "Model 3", "Model X", "Model Y", "Cybertruck", "Roadster"
            },
            ["Ferrari"] = new List<string>
            {
                "F8", "SF90", "296", "812", "Roma", "Portofino", "F12", "488", "LaFerrari"
            },
            ["Lamborghini"] = new List<string>
            {
                "Huracán", "Aventador", "Urus", "Revuelto", "Gallardo", "Murciélago", "Countach"
            },
            ["McLaren"] = new List<string>
            {
                "720S", "765LT", "Artura", "GT", "570S", "650S", "P1", "Senna"
            },
            ["Bentley"] = new List<string>
            {
                "Continental GT", "Flying Spur", "Bentayga", "Mulliner", "Arnage", "Azure"
            },
            ["Rolls-Royce"] = new List<string>
            {
                "Phantom", "Ghost", "Wraith", "Dawn", "Cullinan", "Spectre"
            },
            ["Aston Martin"] = new List<string>
            {
                "DB11", "Vantage", "DBS", "DBX", "Valkyrie", "One-77", "Rapide"
            },
            ["Maserati"] = new List<string>
            {
                "Ghibli", "Quattroporte", "Levante", "Grecale", "MC20", "GranTurismo"
            },
            ["Alfa Romeo"] = new List<string>
            {
                "Giulia", "Stelvio", "Tonale", "Giulietta", "4C", "Brera", "159"
            },
            ["Fiat"] = new List<string>
            {
                "500", "Panda", "Tipo", "Doblo", "Punto", "Bravo", "Linea"
            },
            ["Peugeot"] = new List<string>
            {
                "208", "308", "508", "2008", "3008", "5008", "Rifter", "Traveller"
            },
            ["Renault"] = new List<string>
            {
                "Clio", "Megane", "Captur", "Kadjar", "Koleos", "Talisman", "Zoe", "Twingo"
            },
            ["Citroën"] = new List<string>
            {
                "C3", "C4", "C5", "Berlingo", "C-Elysee", "C4 Cactus", "C3 Aircross"
            },
            ["Opel"] = new List<string>
            {
                "Corsa", "Astra", "Insignia", "Mokka", "Crossland", "Grandland", "Combo"
            },
            ["Skoda"] = new List<string>
            {
                "Fabia", "Octavia", "Superb", "Kamiq", "Karoq", "Kodiaq", "Scala", "Enyaq"
            },
            ["Seat"] = new List<string>
            {
                "Ibiza", "Leon", "Arona", "Ateca", "Tarraco", "Mii", "Alhambra"
            },
            ["Dacia"] = new List<string>
            {
                "Sandero", "Logan", "Duster", "Spring", "Jogger", "Dokker", "Lodgy"
            },
            ["Lada"] = new List<string>
            {
                "Granta", "Vesta", "XRAY", "Largus", "4x4", "Niva", "Kalina"
            },
            ["UAZ"] = new List<string>
            {
                "Patriot", "Hunter", "Pickup", "Cargo", "Profi", "Buhanka"
            },
            ["GAZ"] = new List<string>
            {
                "Volga", "Chaika", "Gazelle", "Sobol", "Valdai", "Next"
            },
            ["ZAZ"] = new List<string>
            {
                "Tavria", "Sens", "Chance", "Vida", "Forza", "Lanos"
            },
            ["Bogdan"] = new List<string>
            {
                "2110", "2111", "2112", "Tata", "Isuzu", "Chery"
            },
            ["Chery"] = new List<string>
            {
                "QQ", "Tiggo", "Arrizo", "E5", "A3", "A5", "A6", "A7", "A8", "Omoda", "Jaecoo"
            },
            ["Geely"] = new List<string>
            {
                "Emgrand", "Vision", "Coolray", "Okavango", "Monjaro", "Geometry", "Zeekr"
            },
            ["BYD"] = new List<string>
            {
                "Han", "Tang", "Song", "Yuan", "Qin", "Seal", "Dolphin", "Atto", "Seagull"
            },
            ["Haval"] = new List<string>
            {
                "H6", "H9", "Jolion", "Dargo", "Chitu", "Shenshou", "Cool Dog"
            },
            ["MG"] = new List<string>
            {
                "3", "5", "6", "HS", "ZS", "Marvel R", "Cyberster", "4", "7"
            },
            ["VinFast"] = new List<string>
            {
                "VF 5", "VF 6", "VF 7", "VF 8", "VF 9", "VF e34", "VF 3"
            },
            ["Thaco"] = new List<string>
            {
                "Ollin", "Forland", "Auman", "TMT", "TMT Pro", "TMT Max"
            },
            ["Truong Hai"] = new List<string>
            {
                "Ollin", "Forland", "Auman", "TMT", "TMT Pro", "TMT Max"
            },
            ["Hyundai Thành Công"] = new List<string>
            {
                "Accent", "Elantra", "Sonata", "Tucson", "Santa Fe", "Palisade", "Kona", "Venue"
            },
            ["Toyota Việt Nam"] = new List<string>
            {
                "Vios", "Corolla Cross", "Camry", "Fortuner", "Innova", "Hilux", "Land Cruiser"
            },
            ["Honda Việt Nam"] = new List<string>
            {
                "City", "Civic", "Accord", "CR-V", "BR-V", "HR-V", "City RS"
            },
            ["Ford Việt Nam"] = new List<string>
            {
                "Ranger", "Everest", "EcoSport", "Explorer", "Transit", "F-150"
            },
            ["Mazda Việt Nam"] = new List<string>
            {
                "Mazda2", "Mazda3", "Mazda6", "CX-3", "CX-30", "CX-5", "CX-8", "CX-9"
            },
            ["Suzuki Việt Nam"] = new List<string>
            {
                "Swift", "Ertiga", "XL7", "Carry", "Super Carry", "Vitara"
            },
            ["Mitsubishi Việt Nam"] = new List<string>
            {
                "Attrage", "Mirage", "Xpander", "Pajero Sport", "Triton", "Outlander"
            },
            ["Nissan Việt Nam"] = new List<string>
            {
                "Sunny", "Almera", "Terra", "Navara", "X-Trail", "Murano"
            },
            ["Isuzu Việt Nam"] = new List<string>
            {
                "D-Max", "MU-X", "QKR", "NQR", "FRR", "FVR"
            },
            ["Hino Việt Nam"] = new List<string>
            {
                "300 Series", "500 Series", "700 Series", "Dutro", "Ranger", "Profia"
            }
        };

        public static List<SelectListItem> GetVehicleMakeList()
        {
            return VehicleMakesAndModels.Keys
                .OrderBy(k => k)
                .Select(make => new SelectListItem
                {
                    Value = make,
                    Text = make
                }).ToList();
        }

        public static List<SelectListItem> GetVehicleModelList(string make)
        {
            if (VehicleMakesAndModels.ContainsKey(make))
            {
                return VehicleMakesAndModels[make]
                    .OrderBy(m => m)
                    .Select(model => new SelectListItem
                    {
                        Value = model,
                        Text = model
                    }).ToList();
            }
            return new List<SelectListItem>();
        }
    }
} 