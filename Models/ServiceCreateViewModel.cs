using Microsoft.AspNetCore.Mvc.Rendering;

public class ServiceCreateViewModel
{
    public string ServiceName { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? image_url { get; set; }

    public List<int>? SelectedGarageIds { get; set; }

    public List<SelectListItem>? GarageList { get; set; }
}
