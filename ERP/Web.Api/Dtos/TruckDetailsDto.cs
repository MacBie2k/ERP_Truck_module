using Web.Api.Entities;

namespace Web.Api.Dtos;

public class TruckDetailsDto
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public TruckStatusEnum Status { get; set; }
    public string Description { get; set; }
}