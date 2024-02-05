using Web.Api.Entities;

namespace Web.Api.Contracts.Truck.Update;

public class UpdateTruckRequest
{
    public int TruckId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public TruckStatusEnum TrackStatus { get; set; }
    public string Description { get; set; }
}