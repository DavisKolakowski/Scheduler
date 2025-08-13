using NodaTime;
using Scheduler.Core.Contexts;
using Scheduler.Core.Factories;

namespace Scheduler.UnitTests;

public class ScheduleContextTests : BaseScheduleTests
{
    [Fact]
    public void ScheduleContext_CreateBuilder_ShouldReturnValidBuilder()
    {
        var clock = CreateClock(2025, 1, 1, 12, 0);
        var factory = CreateFactory(clock);

        var builder = factory.Create(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        Assert.NotNull(builder);
    }

    [Fact]
    public void ScheduleContext_WithDifferentClocks_ShouldRespectClock()
    {
        var clock1 = CreateClock(2025, 1, 1, 16, 0);
        var clock2 = CreateClock(2025, 1, 1, 20, 0);
        
        var factory1 = CreateFactory(clock1);
        var factory2 = CreateFactory(clock2);

        var schedule1 = factory1.Create(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .OneTime()
            .Build();

        var schedule2 = factory2.Create(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .OneTime()
            .Build();

        var next1 = schedule1.NextOccurrence;
        var next2 = schedule2.NextOccurrence;

        // Both events are in the past relative to each clock time
        Assert.Null(next1);
        Assert.Null(next2);
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Tokyo")]
    [InlineData("UTC")]
    public void ScheduleContext_DifferentTimeZones_ShouldWork(string timeZoneId)
    {
        var clock = CreateClock(2025, 1, 1, 12, 0);
        var factory = CreateFactory(clock);
        var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

        var schedule = factory.Create(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            timeZone)
            .OneTime()
            .Build();

        Assert.NotNull(schedule);
        Assert.Equal(timeZone, schedule.Model.TimeZone);
    }

    [Fact]
    public void ScheduleContext_BuilderChaining_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var factory = CreateFactory(clock);

        var schedule = factory.Create(
                TestDate(2025, 1, 1),
                TestTime(10, 0),
                TestTime(11, 0),
                TestTimeZone)
            .Daily()
            .Build();

        Assert.NotNull(schedule);
        Assert.Equal("Daily", schedule.Type);
    }

    [Fact]
    public void ScheduleContext_NullClock_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new ScheduleBuilderFactory(null!));
    }

    [Fact]
    public void ScheduleContext_InvalidTimeRange_EndBeforeStart_ShouldCreateOvernightSchedule()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var factory = CreateFactory(clock);

        var schedule = factory.Create(
            TestDate(2025, 1, 1),
            TestTime(22, 0),
            TestTime(2, 0),
            TestTimeZone)
            .OneTime()
            .Build();

        Assert.NotNull(schedule);
        Assert.Equal(TimeSpan.FromHours(4), schedule.OccurrenceLength);
    }

    [Fact]
    public void ScheduleContext_SameStartEndTime_ShouldCreateZeroDurationSchedule()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var factory = CreateFactory(clock);

        var schedule = factory.Create(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(10, 0),
            TestTimeZone)
            .OneTime()
            .Build();

        Assert.NotNull(schedule);
        Assert.Equal(TimeSpan.Zero, schedule.OccurrenceLength);
    }
}