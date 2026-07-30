using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using TripUpdater.Api.Endpoints;
using TripUpdater.Application.Trips.Queries.GetTrips;
using TripUpdater.Domain.Common;

namespace TripUpdater.Api.Endpoints;

public sealed class TripEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/trips").WithTags("Trips");

        group.MapGet("/", GetTrips)
            .WithName("GetTrips")
            .WithSummary("Get trips with optional filters")
            .Produces<List<TripDto>>()
            .ProducesProblem(400);
    }

    private static async Task<Ok<Result<List<TripDto>>>> GetTrips(
        [AsParameters] GetTripsQuery query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}
