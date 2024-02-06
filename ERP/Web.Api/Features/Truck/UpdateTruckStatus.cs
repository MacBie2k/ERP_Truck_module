using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Contracts.Truck.UpdateStatus;
using Web.Api.Database;
using Web.Api.Entities;
using Web.Api.Shared;
using Web.Api.Shared.TruckService;

namespace Web.Api.Features.Truck;

public class UpdateTruckStatus
{
    public class Command : IRequest<Result>
    {
        public int TruckId { get; set; }
        public TruckStatusEnum TruckStatus { get; set; }
    }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.TruckId).NotEmpty();
            RuleFor(x => x.TruckStatus).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly ApplicationDBContext _dbContext;
        private readonly IValidator<Command> _validator;
        private readonly ITruckService _truckService;

        public Handler(ILogger<Handler> logger, ApplicationDBContext dbContext, IValidator<Command> validator, ITruckService truckService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _validator = validator;
            _truckService = truckService;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure(new Error("UpdateTruckStatus.Validation", validationResult.ToString()));
                if(!await _truckService.CanUpdateStatus(request.TruckId, request.TruckStatus, cancellationToken))
                    return Result.Failure(new Error("UpdateTruckStatus.Validation", "Unable to update truck's status"));
                
                await UpdateStatus(request.TruckId, request.TruckStatus, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to perform UpdateTruckStatus");
                return Result.Failure(new Error("UpdateTruckStatus.Exception", "Unable to update truck's status"));
            }
        }
        

        async Task UpdateStatus(int truckId, TruckStatusEnum truckStatus, CancellationToken cancellationToken)
        {
            var truck = await _dbContext.Trucks.Where(x=>x.Id == truckId).FirstOrDefaultAsync(cancellationToken);
            if (truck != default)
            {
                truck.Status = truckStatus;
                _dbContext.Trucks.Update(truck);
            }
        }
    }
}

public class UpdateTruckStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/trucks/status/{id}", async (int id, UpdateTruckStatusRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateTruckStatus.Command>();
            command.TruckId = id;
            var result = await sender.Send(command);
            if (result.IsFailure)
                return Results.BadRequest(result.Error);

            return Results.Ok(result);
        }).WithTags("Trucks");
    }
}