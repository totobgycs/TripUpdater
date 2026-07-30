using Mediator;
using TripUpdater.Domain.Common;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Application.UpdateLogs.Queries.GetUpdateLogs;

public sealed record GetUpdateLogsQuery(
    DateTimeOffset? From,
    DateTimeOffset? To,
    Status? Status) : IRequest<Result<List<UpdateLogDto>>>;
