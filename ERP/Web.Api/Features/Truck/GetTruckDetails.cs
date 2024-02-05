using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Contracts.Truck.Details;
using Web.Api.Database;
using Web.Api.Dtos;
using Web.Api.Shared;

namespace Web.Api.Features.Truck;

public class GetTruckDetails
{
    public class Query : IRequest<Result<GetTruckDetailsResponse>>
    {
        public int TruckId { get; set; }
    }
    
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.TruckId).NotEmpty();
        }
    }
    
    internal sealed class Handler : IRequestHandler<Query, Result<GetTruckDetailsResponse>>
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly IValidator<Query> _validator;
        private readonly ILogger<Handler> _logger;

        public Handler(ApplicationDBContext dbContext, IValidator<Query> validator, ILogger<Handler> logger)
        {
            _dbContext = dbContext;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<GetTruckDetailsResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<GetTruckDetailsResponse>(new Error("GetTruckDetailsResponse.Validation", validationResult.ToString()));

                var truck = await _dbContext.Trucks.Where(x => x.Id == request.TruckId)
                    .Select(x => new TruckDetailsDto()
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Name = x.Name,
                        Description = x.Description,
                        Status = x.Status
                    }).FirstOrDefaultAsync(cancellationToken);

                return truck != null
                    ? new GetTruckDetailsResponse() {  Truck = truck }
                    : Result.Failure<GetTruckDetailsResponse>(new Error("GetTruckDetailsResponse.NotFounded",
                        "The truck not founded"));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to perform GetTruckDetails");
                return Result.Failure<GetTruckDetailsResponse>(new Error("GetTruckDetailsResponse.Exception", "Unable to get truck details"));
            }
        }
    }
}

public class GetTruckDetailsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/trucks/{id}", async (int truckId,  ISender sender) =>
        {
            var query = new GetTruckDetails.Query() { TruckId = truckId};

            var result = await sender.Send(query);
            if (result.IsFailure)
                return Results.BadRequest(result.Error);

            return Results.Ok(result.Value);
        }).WithTags("Trucks");
    }
}