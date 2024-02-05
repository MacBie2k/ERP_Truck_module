using Web.Api.Dtos;

namespace Web.Api.Contracts.Truck.Details;

public class GetTruckDetailsResponse
{
    public TruckDetailsDto Truck { get; set; }
}