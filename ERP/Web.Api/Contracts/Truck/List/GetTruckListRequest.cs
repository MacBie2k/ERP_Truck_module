using Web.Api.Entities;

namespace Web.Api.Contracts.Truck.List;

public class GetTruckListRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public TruckStatusEnum? TruckStatus { get; set; }
}