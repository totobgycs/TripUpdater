using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using TripUpdater.Api.Endpoints;
using TripUpdater.Application.Updates.Commands.ProcessTripUpdates;
using TripUpdater.Domain.Common;

namespace TripUpdater.Api.Endpoints;

public sealed class UpdateEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/updates/trips").WithTags("Updates");

        group.MapPost("/", ProcessTripUpdates)
            .WithName("ProcessTripUpdates")
            .WithSummary("Process a batch of trip updates")
            .Produces<UpdateSummaryDto>()
            .ProducesProblem(400);
    }

    private static async Task<Ok<Result<UpdateSummaryDto>>> ProcessTripUpdates(
        ProcessTripUpdatesCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }
}
