using NodaTime;
using TripUpdater.Domain.Entities;
using TripUpdater.Domain.Enums;

namespace TripUpdater.Tests.Unit.Domain;

public class TripStatusCalculationTests
{
    private static readonly Instant Departure = Instant.FromUtc(2026, 7, 29, 8, 0, 0);
    private static readonly Instant OriginalArrival = Instant.FromUtc(2026, 7, 29, 8, 30, 0);

    [Fact]
    public void ValidateStatus_Cancelled_AcceptsAnything()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.ValidateStatus(Status.Cancelled, null);

        Assert.Equal(Status.Cancelled, status);
    }

    [Fact]
    public void ValidateStatus_Invalid_AcceptsAnything()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.ValidateStatus(Status.Invalid, OriginalArrival);

        Assert.Equal(Status.Invalid, status);
    }

    [Fact]
    public void ValidateStatus_OntimeWithoutArrival_ReturnsInvalid()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.ValidateStatus(Status.Ontime, null);

        Assert.Equal(Status.Invalid, status);
    }

    [Fact]
    public void ValidateStatus_OntimeWithinTwoMinutes_ReturnsOntime()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.ValidateStatus(Status.Ontime, OriginalArrival + Duration.FromSeconds(90));

        Assert.Equal(Status.Ontime, status);
    }

    [Fact]
    public void ValidateStatus_OntimeTooLate_ReturnsInvalid()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.ValidateStatus(Status.Ontime, OriginalArrival + Duration.FromMinutes(7));

        Assert.Equal(Status.Invalid, status);
    }

    [Fact]
    public void ValidateStatus_EarlyMoreThanTwoMinutes_ReturnsEarly()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.ValidateStatus(Status.Early, OriginalArrival - Duration.FromMinutes(5));

        Assert.Equal(Status.Early, status);
    }

    [Fact]
    public void ValidateStatus_EarlyWithinTwoMinutes_ReturnsInvalid()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.ValidateStatus(Status.Early, OriginalArrival - Duration.FromSeconds(90));

        Assert.Equal(Status.Invalid, status);
    }

    [Fact]
    public void ValidateStatus_LateMoreThanTwoMinutes_ReturnsLate()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.ValidateStatus(Status.Late, OriginalArrival + Duration.FromMinutes(7));

        Assert.Equal(Status.Late, status);
    }

    [Fact]
    public void ValidateStatus_LateWithinTwoMinutes_ReturnsInvalid()
    {
        var trip = Trip.Create(1, 1, Departure, OriginalArrival);

        var status = trip.ValidateStatus(Status.Late, OriginalArrival + Duration.FromSeconds(90));

        Assert.Equal(Status.Invalid, status);
    }
}
