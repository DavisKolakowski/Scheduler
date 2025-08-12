using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class ScheduleContextTests : BaseScheduleTests
{
    [Fact]
    public void ScheduleContext_CreateBuilder_ShouldReturnValidBuilder()
    {
        var clock = CreateClock(2025, 1, 1, 12, 0);
        var context = new ScheduleContext(clock);

        var builder = context.CreateBuilder(
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
        
        var context1 = new ScheduleContext(clock1);
        var context2 = new ScheduleContext(clock2);

        var schedule1 = context1.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Build();

        var schedule2 = context2.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Build();

        var next1 = schedule1.GetNextOccurrence();
        var next2 = schedule2.GetNextOccurrence();

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
        var context = new ScheduleContext(clock);
        var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

        var builder = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            timeZone);

        var schedule = builder.Build();

        Assert.NotNull(schedule);
        Assert.Equal(timeZone, schedule.Options.TimeZone);
    }

    [Fact]
    public void ScheduleContext_BuilderChaining_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = new ScheduleContext(clock);

        var schedule = context.CreateBuilder(
                TestDate(2025, 1, 1),
                TestTime(10, 0),
                TestTime(11, 0),
                TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        Assert.NotNull(schedule);
        Assert.Equal("Daily", schedule.Type);
    }

    [Fact]
    public void ScheduleContext_NullClock_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new ScheduleContext(null!));
    }

    [Fact]
    public void ScheduleContext_InvalidTimeRange_EndBeforeStart_ShouldCreateOvernightSchedule()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = new ScheduleContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(22, 0),
            TestTime(2, 0),
            TestTimeZone)
            .Build();

        Assert.NotNull(schedule);
        Assert.Equal("04:00", schedule.OccurrenceDuration);
    }

    [Fact]
    public void ScheduleContext_SameStartEndTime_ShouldCreateZeroDurationSchedule()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = new ScheduleContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(10, 0),
            TestTimeZone)
            .Build();

        Assert.NotNull(schedule);
        Assert.Equal("00:00", schedule.OccurrenceDuration);
    }
}