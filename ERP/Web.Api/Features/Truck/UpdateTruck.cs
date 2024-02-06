using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Contracts.Truck.Update;
using Web.Api.Database;
using Web.Api.Entities;
using Web.Api.Shared;
using Web.Api.Shared.TruckService;

namespace Web.Api.Features.Truck;

public class UpdateTruck
{
    public class Command : IRequest<Result>
    {
        public int TruckId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        
        public TruckStatusEnum TrackStatus { get; set; }
        public string Description { get; set; }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(Entity.DescriptionMaxLength);
            RuleFor(x => x.Code)
                .NotEmpty()
                .Matches(@"^[a-zA-Z0-9]+$")
                .Length(Entities.Truck.CodeLength);
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(Entity.NameMaxLength);
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
                    return Result.Failure(new Error("UpdateTruck.Validation", validationResult.ToString()));
                if(!await _truckService.CanUpdateStatus(request.TruckId, request.TrackStatus, cancellationToken))
                    return Result.Failure(new Error("UpdateTruck.Validation", "Unable to update the truck"));
                
                var truck = await _dbContext.Trucks.Where(x=>x.Id == request.TruckId).FirstOrDefaultAsync(cancellationToken);
                if (truck == default) 
                    return Result.Failure(new Error("UpdateTruck.NotFound", "The truck not found"));

                if (truck.Code != request.Code && !await _truckService.CanCreate(request.Code, cancellationToken))
                    return Result.Failure(new Error("UpdateTruck.Validation", "A truck with the same code already exists"));
                
                truck.Status = request.TrackStatus;
                truck.Name = request.Name;
                truck.Description = request.Description;
                
                _dbContext.Trucks.Update(truck);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to perform UpdateTruck");
                return Result.Failure(new Error("UpdateTruck.Exception", "Unable to update the truck"));
            }
        }
        

        async Task UpdateTruckStatus(int truckId, TruckStatusEnum truckStatus, CancellationToken cancellationToken)
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


public class UpdateTruckEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/trucks/{id}", async (int id, UpdateTruckRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateTruck.Command>();
            command.TruckId = id;
            var result = await sender.Send(command);
            if (result.IsFailure)
                return Results.BadRequest(result.Error);

            return Results.Ok(result);
        }).WithTags("Trucks");
    }
}