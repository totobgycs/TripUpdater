using System.Net;
using System.Net.Http.Json;
using TripUpdater.Application.Updates.Commands.ProcessTripUpdates;
using TripUpdater.Domain.Enums;
using TripUpdater.Tests.Fixtures;

namespace TripUpdater.Tests.Integration;

public class TripEndpointsIntegrationTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task PostUpdates_ValidBatch_ReturnsSummary()
    {
        var request = new ProcessTripUpdatesCommand(
        [
            new TripUpdateDto(1001, new DateTimeOffset(2026, 7, 29, 8, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 7, 29, 8, 31, 0, TimeSpan.Zero)),
            new TripUpdateDto(1002, new DateTimeOffset(2026, 7, 29, 8, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 7, 29, 8, 40, 0, TimeSpan.Zero)),
            new TripUpdateDto(1003, new DateTimeOffset(2026, 7, 29, 8, 0, 0, TimeSpan.Zero), null)
        ]);

        var response = await _client.PostAsJsonAsync("/updates/trips", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<ResultEnvelope<UpdateSummaryDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.IsSuccess);
        Assert.Equal(3, wrapper.Value!.TotalUpdates);
        Assert.Contains(1001, wrapper.Value.ProcessedTripIds);
    }

    [Fact]
    public async Task GetTrips_WithFilter_ReturnsMatchingTrips()
    {
        var response = await _client.GetAsync("/trips?lineNo=1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<ResultEnvelope<List<TripDto>>>(TestContext.Current.CancellationToken);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.IsSuccess);
        Assert.NotNull(wrapper.Value);
        Assert.NotEmpty(wrapper.Value);
        Assert.All(wrapper.Value, t => Assert.Equal(1, t.LineNo));
    }

    private sealed record ResultEnvelope<T>(bool IsSuccess, T? Value, IReadOnlyList<object>? Errors);
    private sealed record TripDto(int TripId, Guid LineId, int LineNo, DateTimeOffset DepartureTime, DateTimeOffset OriginalArrivalTime, DateTimeOffset? ArrivalTime, int Status);
}
