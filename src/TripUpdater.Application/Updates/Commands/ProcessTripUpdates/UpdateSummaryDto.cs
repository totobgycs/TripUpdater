namespace TripUpdater.Application.Updates.Commands.ProcessTripUpdates;

public sealed record UpdateSummaryDto(
    int TotalUpdates,
    int Ontime,
    int Early,
    int Late,
    int Cancelled,
    int Invalid,
    List<int> ProcessedTripIds);
