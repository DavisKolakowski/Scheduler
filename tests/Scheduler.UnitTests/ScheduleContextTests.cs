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
        // Arrange
        var clock = CreateClock(2025, 1, 1, 12, 0);
        var context = new ScheduleContext(clock);

        // Act
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void ScheduleContext_CreateBuilderWithCalendarSystem_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 12, 0);
        var context = new ScheduleContext(clock);

        // Act
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone,
            CalendarSystem.Iso);

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void ScheduleContext_WithDifferentClocks_ShouldRespectClock()
    {
        // Arrange
        var clock1 = CreateClock(2025, 1, 1, 12, 0);
        var clock2 = CreateClock(2025, 1, 1, 18, 0);
        
        var context1 = new ScheduleContext(clock1);
        var context2 = new ScheduleContext(clock2);

        var schedule1 = context1.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone).Build();

        var schedule2 = context2.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone).Build();

        // Act
        var next1 = schedule1.GetNextOccurrence();
        var next2 = schedule2.GetNextOccurrence();

        // Assert
        // At 12:00, the 10-11 AM event should be in the past
        // At 18:00, the 10-11 AM event should be in the past
        // Both should have null next occurrence since it's a one-time event in the past
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
        // Arrange
        var clock = CreateClock(2025, 1, 1, 12, 0);
        var context = new ScheduleContext(clock);
        var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

        // Act
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            timeZone);

        var schedule = builder.Build();

        // Assert
        Assert.NotNull(schedule);
        Assert.Equal(timeZone, schedule.Options.TimeZone);
    }

    [Fact]
    public void ScheduleContext_BuilderChaining_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = new ScheduleContext(clock);

        // Act
        var schedule = context.CreateBuilder(
                TestDate(2025, 1, 1),
                TestTime(10, 0),
                TestTime(11, 0),
                TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        // Assert
        Assert.NotNull(schedule);
        Assert.Equal("Daily", schedule.Type);
    }

    [Fact]
    public void ScheduleContext_NullClock_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ScheduleContext(null!));
    }

    [Fact]
    public void ScheduleContext_InvalidTimeRange_EndBeforeStart_ShouldCreateOvernightSchedule()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = new ScheduleContext(clock);

        // Act - End time before start time (overnight)
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(22, 0), // 10 PM
            TestTime(2, 0),  // 2 AM next day
            TestTimeZone).Build();

        // Assert
        Assert.NotNull(schedule);
        Assert.Equal("04:00", schedule.OccurrenceDuration); // 4 hours (22:00 to 02:00)
    }

    [Fact]
    public void ScheduleContext_SameStartEndTime_ShouldCreateZeroDurationSchedule()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = new ScheduleContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(10, 0), // Same time
            TestTimeZone).Build();

        // Assert
        Assert.NotNull(schedule);
        Assert.Equal("00:00", schedule.OccurrenceDuration);
    }
}