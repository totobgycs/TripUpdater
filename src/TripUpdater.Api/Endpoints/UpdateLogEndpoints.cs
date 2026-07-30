using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using TripUpdater.Api.Endpoints;
using TripUpdater.Application.UpdateLogs.Queries.GetUpdateLogs;
using TripUpdater.Domain.Common;

namespace TripUpdater.Api.Endpoints;

public sealed class UpdateLogEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/updatelogs").WithTags("UpdateLogs");

        group.MapGet("/", GetUpdateLogs)
            .WithName("GetUpdateLogs")
            .WithSummary("Get update logs with optional filters")
            .Produces<List<UpdateLogDto>>()
            .ProducesProblem(400);
    }

    private static async Task<Ok<Result<List<UpdateLogDto>>>> GetUpdateLogs(
        [AsParameters] GetUpdateLogsQuery query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}
