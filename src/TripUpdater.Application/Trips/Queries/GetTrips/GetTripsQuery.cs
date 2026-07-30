using Mediator;
using TripUpdater.Domain.Common;

namespace TripUpdater.Application.Trips.Queries.GetTrips;

public sealed record GetTripsQuery(
    int? LineNo,
    DateTimeOffset? From,
    DateTimeOffset? To) : IRequest<Result<List<TripDto>>>;
