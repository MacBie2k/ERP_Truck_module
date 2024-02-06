using Carter;
using FluentValidation;
using MediatR;
using Web.Api.Database;
using Web.Api.Shared;

namespace Web.Api.Features.Truck;

public class DeleteTruck
{
    public class Command : IRequest<Result>
    {
        public int TruckId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.TruckId).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IValidator<Command> _validator;
        private readonly ApplicationDBContext _dbContext;
        private readonly ILogger<Handler> _logger;
        public Handler(IValidator<Command> validator, ApplicationDBContext dbContext, ILogger<Handler> logger)
        {
            _validator = validator;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure(new Error("DeleteTruck.Validation", validationResult.ToString()));
                await DeleteTruck(request.TruckId, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Result.Failure(new Error("DeleteTruck", "Unable to delete truck"));
            }
        }
        
        public async Task DeleteTruck(int truckId, CancellationToken cancellationToken = default)
        {
            var truck = _dbContext.Trucks.FirstOrDefault(x => x.Id == truckId);
            if (truck != null) 
                _dbContext.Trucks.Remove(truck);
        }
    }
}

public class DeleteTruckEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/trucks/{id}", async (int id, ISender sender) =>
        {
            var command = new DeleteTruck.Command() { TruckId = id };

            var result = await sender.Send(command);
            if (result.IsFailure)
                return Results.BadRequest(result.Error);

            return Results.Ok(result);
        }).WithTags("Trucks");
    }
}