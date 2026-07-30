using TripUpdater.Domain.Entities;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Tests.Unit.Domain;

public class TripStatusCalculationTests
{
    private static readonly DateTimeOffset Departure = new(2026, 7, 29, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset OriginalArrival = new(2026, 7, 29, 8, 30, 0, TimeSpan.Zero);

    [Fact]
    public void CalculateStatus_NullArrival_ReturnsCancelled()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.CalculateStatus(null);

        Assert.Equal(Status.Cancelled, status);
    }

    [Fact]
    public void CalculateStatus_ArrivalBeforeDeparture_ReturnsInvalid()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.CalculateStatus(Departure.AddMinutes(-5));

        Assert.Equal(Status.Invalid, status);
    }

    [Fact]
    public void CalculateStatus_WithinTwoMinutes_ReturnsOntime()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.CalculateStatus(OriginalArrival.AddSeconds(90));

        Assert.Equal(Status.Ontime, status);
    }

    [Fact]
    public void CalculateStatus_MoreThanTwoMinutesEarly_ReturnsEarly()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.CalculateStatus(OriginalArrival.AddMinutes(-5));

        Assert.Equal(Status.Early, status);
    }

    [Fact]
    public void CalculateStatus_MoreThanTwoMinutesLate_ReturnsLate()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.CalculateStatus(OriginalArrival.AddMinutes(7));

        Assert.Equal(Status.Late, status);
    }
}
