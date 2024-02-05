using Web.Api.Entities;

namespace Web.Api.Contracts.Truck.UpdateStatus;

public class UpdateTruckStatusRequest
{
    public int TruckId { get; set; }
    public TruckStatusEnum TruckStatus { get; set; }
}