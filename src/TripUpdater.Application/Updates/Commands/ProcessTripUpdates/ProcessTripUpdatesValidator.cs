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

                update.RuleFor(u => u.Status)
                    .IsInEnum()
                    .Must(s => ValidStatuses.Contains(s))
                    .WithMessage("Status must be a valid trip status.");
            });
    }
}
