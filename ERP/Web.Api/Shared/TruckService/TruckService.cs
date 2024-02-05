using Microsoft.EntityFrameworkCore;
using Web.Api.Database;
using Web.Api.Entities;

namespace Web.Api.Shared.TruckService;

public class TruckService(ApplicationDBContext dbContext) : ITruckService
{
    public async Task<bool> CanUpdateStatus(int truckId, TruckStatusEnum truckStatus, CancellationToken cancellationToken)
    {
        var currentStatus = await dbContext.Trucks.Where(x => x.Id == truckId).Select(x => x.Status)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (truckStatus == TruckStatusEnum.OutOfService)
        {
            return true;
        }
    
        if (currentStatus == TruckStatusEnum.OutOfService)
        {
            return true;
        }

        switch (currentStatus)
        {
            case TruckStatusEnum.Loading when truckStatus == TruckStatusEnum.ToJob:
            case TruckStatusEnum.ToJob when truckStatus == TruckStatusEnum.AtJob:
            case TruckStatusEnum.AtJob when truckStatus == TruckStatusEnum.Returning:
            case TruckStatusEnum.Returning when truckStatus == TruckStatusEnum.Loading:
                return true; 
            default:
                return false; 
        } 
    }
        
    public async Task<bool> CanCreate(string code, CancellationToken cancellationToken)
    {
        return await dbContext.Trucks.AllAsync(x => x.Code != code, cancellationToken);
    }
}