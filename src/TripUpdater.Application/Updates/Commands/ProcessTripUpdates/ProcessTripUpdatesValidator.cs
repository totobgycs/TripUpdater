using FluentValidation;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Application.Updates.Commands.ProcessTripUpdates;

public sealed class ProcessTripUpdatesValidator : AbstractValidator<ProcessTripUpdatesCommand>
{
    private static readonly Status[] ValidStatuses =
    [
        Status.Ontime,
        Status.Early,
        Status.Late,
        Status.Cancelled,
        Status.Invalid
    ];

    public ProcessTripUpdatesValidator()
    {
        RuleFor(x => x.Updates)
            .NotEmpty()
            .WithMessage("At least one trip update is required.");

        RuleForEach(x => x.Updates)
            .ChildRules(update =>
            {
                update.RuleFor(u => u.TripId)
                    .GreaterThan(0)
                    .WithMessage("TripId must be positive.");
                update.RuleFor(u => u.DepartureTime)
                    .Must(dt => dt is null || dt.Value > DateTimeOffset.MinValue)
                    .WithMessage("DepartureTime must be a valid date.");
                update.RuleFor(u => u.ActualArrivalTime)
                    .Must(dt => dt is null || dt.Value > DateTimeOffset.MinValue)
                    .WithMessage("ActualArrivalTime must be a valid date.");
            });
    }
}
