using NodaTime;
using TripUpdater.Domain.Entities;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Tests.Unit.Domain;

public class TripStatusCalculationTests
{
    private static readonly Instant Departure = Instant.FromUtc(2026, 7, 29, 8, 0, 0);
    private static readonly Instant OriginalArrival = Instant.FromUtc(2026, 7, 29, 8, 30, 0);

    [Fact]
    public void CalculateStatus_NullArrival_ReturnsCancelled()
    {
        // Arrange
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());

        // Act
        var status = trip.CalculateStatus(null);

        // Assert
        Assert.Equal(Status.Cancelled, status);
    }

    [Fact]
    public void CalculateStatus_ExactArrival_ReturnsOntime()
    {
        // Arrange
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());

        // Act
        var status = trip.CalculateStatus(OriginalArrival);

        // Assert
        Assert.Equal(Status.Ontime, status);
    }

    [Fact]
    public void CalculateStatus_ArrivalEarlyWithinTwoMinutes_ReturnsOntime()
    {
        // Arrange
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());

        // Act
        var status = trip.CalculateStatus(OriginalArrival - Duration.FromSeconds(90));

        // Assert
        Assert.Equal(Status.Ontime, status);
    }

    [Fact]
    public void CalculateStatus_ArrivalExactlyTwoMinutesEarly_ReturnsEarly()
    {
        // Arrange
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());

        // Act
        var status = trip.CalculateStatus(OriginalArrival - Duration.FromMinutes(2));

        // Assert
        Assert.Equal(Status.Early, status);
    }

    [Fact]
    public void CalculateStatus_ArrivalEarlyMoreThanTwoMinutes_ReturnsEarly()
    {
        // Arrange
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());

        // Act
        var status = trip.CalculateStatus(OriginalArrival - Duration.FromMinutes(5));

        // Assert
        Assert.Equal(Status.Early, status);
    }

    [Fact]
    public void CalculateStatus_ArrivalLateWithinTwoMinutes_ReturnsOntime()
    {
        // Arrange
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());

        // Act
        var status = trip.CalculateStatus(OriginalArrival + Duration.FromSeconds(90));

        // Assert
        Assert.Equal(Status.Ontime, status);
    }

    [Fact]
    public void CalculateStatus_ArrivalExactlyTwoMinutesLate_ReturnsLate()
    {
        // Arrange
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());

        // Act
        var status = trip.CalculateStatus(OriginalArrival + Duration.FromMinutes(2));

        // Assert
        Assert.Equal(Status.Late, status);
    }

    [Fact]
    public void CalculateStatus_ArrivalLateMoreThanTwoMinutes_ReturnsLate()
    {
        // Arrange
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());

        // Act
        var status = trip.CalculateStatus(OriginalArrival + Duration.FromMinutes(7));

        // Assert
        Assert.Equal(Status.Late, status);
    }

    [Fact]
    public void CalculateStatus_ArrivalBeforeDeparture_ReturnsInvalid()
    {
        // Arrange
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());

        // Act
        var status = trip.CalculateStatus(Departure - Duration.FromMinutes(1));

        // Assert
        Assert.Equal(Status.Invalid, status);
    }

    [Fact]
    public void CalculateStatus_ArrivalAtDeparture_ReturnsEarly()
    {
        // Arrange — boundary: arrival equal to departure is valid (check is <, not <=).
        //           diff = -30 min, well beyond the -2 min Early threshold.
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());

        // Act
        var status = trip.CalculateStatus(Departure);

        // Assert
        Assert.Equal(Status.Early, status);
    }

    [Fact]
    public void CalculateStatus_WhenAlreadyCancelled_ReturnsCancelled()
    {
        // Arrange — a cancelled trip is terminal: a later arrival must not resurrect it.
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());
        trip.ApplyUpdate(null, null);

        // Act
        var status = trip.CalculateStatus(OriginalArrival);

        // Assert
        Assert.Equal(Status.Cancelled, status);
    }

    [Fact]
    public void CalculateStatus_WhenAlreadyInvalid_ReturnsInvalid()
    {
        // Arrange — an invalid trip is terminal: a later valid arrival must not resurrect it.
        var trip = Trip.Create(1, Departure, OriginalArrival, CreateLine());
        trip.ApplyUpdate(null, Departure - Duration.FromMinutes(1));

        // Act
        var status = trip.CalculateStatus(OriginalArrival);

        // Assert
        Assert.Equal(Status.Invalid, status);
    }

    private static Line CreateLine() => Line.Create(1, 1, "OP123", "LP456");
}
