namespace TripUpdater.Application.Updates.Commands.ProcessTripUpdates;

public sealed record TripUpdateDto(int TripId, DateTimeOffset? DepartureTime, DateTimeOffset? ActualArrivalTime);
