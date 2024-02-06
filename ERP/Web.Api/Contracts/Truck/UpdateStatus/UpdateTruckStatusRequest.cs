using Web.Api.Entities;

namespace Web.Api.Contracts.Truck.UpdateStatus;

public class UpdateTruckStatusRequest
{
    public TruckStatusEnum TruckStatus { get; set; }
}