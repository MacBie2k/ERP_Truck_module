using Web.Api.Dtos;

namespace Web.Api.Contracts.Truck.List;

public class GetTruckListResponse
{
    public List<TruckListItemDto> Trucks { get; set; }
}