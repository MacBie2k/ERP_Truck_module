using Web.Api.Entities;

namespace Web.Api.Shared.TruckService;

public interface ITruckService
{
    Task<bool> CanUpdateStatus(int truckId, TruckStatusEnum truckStatus, CancellationToken cancellationToken);
    Task<bool> CanCreate(string code, CancellationToken cancellationToken);

}