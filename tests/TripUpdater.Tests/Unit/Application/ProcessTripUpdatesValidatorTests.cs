using FluentValidation.TestHelper;
using TripUpdater.Application.Updates.Commands.ProcessTripUpdates;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Tests.Unit.Application;

public class ProcessTripUpdatesValidatorTests
{
    private readonly ProcessTripUpdatesValidator _validator = new();

    [Fact]
    public void Validate_EmptyUpdates_ReturnsError()
    {
        var command = new ProcessTripUpdatesCommand([]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Updates);
    }

    [Fact]
    public void Validate_ValidUpdates_Passes()
    {
        var command = new ProcessTripUpdatesCommand(
        [
            new TripUpdateDto(1001, DateTimeOffset.UtcNow - TimeSpan.FromMinutes(5), DateTimeOffset.UtcNow),
            new TripUpdateDto(1002, DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10), null),
            new TripUpdateDto(1002, null, null)
        ]);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
