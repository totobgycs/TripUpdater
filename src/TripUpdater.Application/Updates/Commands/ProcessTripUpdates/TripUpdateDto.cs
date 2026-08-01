using TripUpdater.Domain.Enums;

namespace TripUpdater.Application.Updates.Commands.ProcessTripUpdates;

public sealed record TripUpdateDto(int TripId, Status Status, DateTimeOffset? ActualArrivalTime);
