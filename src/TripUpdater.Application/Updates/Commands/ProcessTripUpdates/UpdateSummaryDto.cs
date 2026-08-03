namespace TripUpdater.Application.Updates.Commands.ProcessTripUpdates;

public sealed record UpdateSummaryDto(
    int TotalUpdates,
    List<int> ProcessedTripIds,
    List<TripUpdateDto> UnprocessedUpdates);
