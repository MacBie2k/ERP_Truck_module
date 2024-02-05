using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Contracts.Truck.List;
using Web.Api.Database;
using Web.Api.Dtos;
using Web.Api.Entities;
using Web.Api.Shared;

namespace Web.Api.Features.Truck;

public class GetTruckList 
{
    public class Query : IRequest<Result<GetTruckListResponse>>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public TruckStatusEnum? TruckStatus { get; set; }
    }
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Page)
                .NotEmpty();
            RuleFor(x => x.PageSize)
                .NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Query, Result<GetTruckListResponse>>
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly ILogger<Handler> _logger;
        private readonly IValidator<Query> _validator;
        public Handler(ApplicationDBContext dbContext, ILogger<Handler> logger, IValidator<Query> validator)
        {
            _dbContext = dbContext;
            _logger = logger;
            _validator = validator;
        }

        public async Task<Result<GetTruckListResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                    return Result.Failure<GetTruckListResponse>(new Error("GetTruckList.Validation", validationResult.ToString()));

                var trucks = await _dbContext.Trucks
                    .Where(x=>string.IsNullOrWhiteSpace(request.Code) || x.Code.Contains(request.Code))
                    .Where(x=>string.IsNullOrWhiteSpace(request.Name) || x.Code.Contains(request.Name))
                    .Where(x=>!request.TruckStatus.HasValue || x.Status == request.TruckStatus)
                    .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                    .Select(x=> new TruckListItemDto()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Code = x.Code
                    })
                    .ToListAsync(cancellationToken);

                return new GetTruckListResponse() { Trucks = trucks};
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to perform GetTruckList");
                return Result.Failure<GetTruckListResponse>(new Error("GetTruckList.Exception", "Unable to get Truck List"));
            }
        }
    }
}

public class GetTruckListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/trucks/list", async (GetTruckListRequest request, ISender sender) =>
        {
            var query = request.Adapt<GetTruckList.Query>();

            var result = await sender.Send(query);
            if (result.IsFailure)
                return Results.BadRequest(result.Error);

            return Results.Ok(result.Value);
        }).WithTags("Trucks");
    }
}