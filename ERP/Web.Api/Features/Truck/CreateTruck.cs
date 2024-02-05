using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Contracts.Truck.Create;
using Web.Api.Database;
using Web.Api.Entities;
using Web.Api.Shared;
using Web.Api.Shared.TruckService;

namespace Web.Api.Features.Truck;

public class CreateTruck
{
    public class Command : IRequest<Result<int>>
    {
        public string Code { get; set; }
        public string Name { get; set; }
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
    
    internal sealed class Handler : IRequestHandler<CreateTruck.Command, Result<int>>
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IValidator<Command> _validator;
        private readonly ILogger<Handler> _logger;
        private readonly ITruckService _truckService;

        public Handler(ApplicationDBContext dbContext, IValidator<Command> validator, ILogger<Handler> logger, ITruckService truckService)
        {
            _dbContext = dbContext;
            _validator = validator;
            _logger = logger;
            _truckService = truckService;
        }

        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<int>(new Error("CreateTruck.Validation", validationResult.ToString()));
                if(!await _truckService.CanCreate(request.Code, cancellationToken))
                    return Result.Failure<int>(new Error("CreateTruck.Validation", "A truck with the same code already exists"));
                
                var truck = request.Adapt<Entities.Truck>();
                await _dbContext.Trucks.AddAsync(truck, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return truck.Id;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to perform CreateTruck");
                return Result.Failure<int>(new Error("CreateTruck.Exception", "Unable to create new Truck"));
            }
        }
        
    }
}

public class CreateProjectEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/trucks", async (CreateTruckRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateTruck.Command>();

            var result = await sender.Send(command);
            if (result.IsFailure)
                return Results.BadRequest(result.Error);

            return Results.Ok(result.Value);
        }).WithTags("Trucks");
    }
}