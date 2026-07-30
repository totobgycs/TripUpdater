using Mediator;
using TripUpdater.Domain.Common;

namespace TripUpdater.Application.Updates.Commands.ProcessTripUpdates;

public sealed record ProcessTripUpdatesCommand(List<TripUpdateDto> Updates) : IRequest<Result<UpdateSummaryDto>>;
