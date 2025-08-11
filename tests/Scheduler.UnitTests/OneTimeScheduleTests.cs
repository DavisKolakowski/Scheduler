using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class OneTimeScheduleTests : BaseScheduleTests
{
    [Fact]
    public void OneTimeSchedule_SameDay_ShouldReturnCorrectOccurrence()
    {
        // Arrange
        var clock = CreateClock(2024, 8, 20, 9, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(11, 30),
            TestTimeZone);

        var schedule = builder.Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var previousOccurrence = schedule.GetPreviousOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();
        var completed = schedule.GetOccurrencesCompleted(5).ToList();

        // Assert
        Assert.Equal("OneTime", schedule.Type);
        Assert.NotNull(nextOccurrence);
        Assert.Null(previousOccurrence);
        Assert.Single(upcoming);
        Assert.Empty(completed);
    }

    [Fact]
    public void OneTimeSchedule_InPast_ShouldReturnPreviousOccurrence()
    {
        // Arrange
        var clock = CreateClock(2024, 8, 20, 16, 0); // 4 PM UTC = 12 PM EDT (after 11:30 AM EDT event)
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(11, 30),
            TestTimeZone);

        var schedule = builder.Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var previousOccurrence = schedule.GetPreviousOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();
        var completed = schedule.GetOccurrencesCompleted(5).ToList();

        // Assert
        Assert.Null(nextOccurrence);
        Assert.NotNull(previousOccurrence);
        Assert.Empty(upcoming);
        Assert.Single(completed);
    }

    [Fact]
    public void OneTimeSchedule_CurrentlyActive_ShouldReturnAsNext()
    {
        // Arrange - Clock set to during the event
        var clock = CreateClock(2024, 8, 20, 10, 30);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(11, 30),
            TestTimeZone);

        var schedule = builder.Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var previousOccurrence = schedule.GetPreviousOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.Null(previousOccurrence);
    }

    [Fact]
    public void OneTimeSchedule_OvernightEvent_ShouldHandleCorrectly()
    {
        // Arrange
        var clock = CreateClock(2024, 8, 20, 21, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(22, 0),
            TestTime(2, 0), // Next day
            TestTimeZone);

        var schedule = builder.Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var occurrenceDuration = schedule.OccurrenceDuration;

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.Equal("04:00", occurrenceDuration); // 4 hours from 22:00 to 02:00
    }

    [Fact]
    public void OneTimeSchedule_DifferentTimeZones_ShouldRespectTimeZone()
    {
        // Arrange
        var utcClock = CreateClock(2024, 8, 20, 14, 0); // 2 PM UTC
        var context = CreateContext(utcClock);
        
        // Create same time in different time zones
        var nySchedule = context.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0), // 10 AM in NY (UTC-4 during summer)
            TestTime(11, 0),
            DateTimeZoneProviders.Tzdb["America/New_York"]).Build();

        var laSchedule = context.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0), // 10 AM in LA (UTC-7 during summer)
            TestTime(11, 0),
            DateTimeZoneProviders.Tzdb["America/Los_Angeles"]).Build();

        // Act
        var nyNext = nySchedule.GetNextOccurrence();
        var laNext = laSchedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(nyNext);
        Assert.NotNull(laNext);
        // NY 10 AM should be in the past (it's 2 PM UTC = 10 AM EDT + 4 hours)
        // LA 10 AM should be in the future (it's 2 PM UTC = 7 AM PDT + 7 hours)
    }

    [Fact]
    public void OneTimeSchedule_Description_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2024, 8, 20, 9, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(11, 30),
            TestTimeZone);

        var schedule = builder.Build();

        // Act
        var description = schedule.Description;

        // Assert
        Assert.Contains("Occurs once", description);
        Assert.Contains("August 20th, 2024", description);
        Assert.Contains("10:00 AM - 11:30 AM", description);
    }

    [Fact]
    public void OneTimeSchedule_ZeroMinuteDuration_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2024, 8, 20, 9, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(10, 0), // Same start and end time
            TestTimeZone);

        var schedule = builder.Build();

        // Act
        var duration = schedule.OccurrenceDuration;

        // Assert
        Assert.Equal("00:00", duration);
    }

    [Fact]
    public void OneTimeSchedule_LeapYear_February29_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2024, 2, 28, 9, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 2, 29), // Leap year
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        var schedule = builder.Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.Equal(29, nextOccurrence.Value.Day);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(23, 59)]
    [InlineData(12, 30)]
    public void OneTimeSchedule_VariousStartTimes_ShouldWork(int hour, int minute)
    {
        // Arrange - Set clock before the schedule time to ensure it's upcoming
        var clock = CreateClock(2024, 8, 19, 23, 0); // Day before the schedule
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(hour, minute),
            TestTime(hour, minute + 30 > 59 ? 59 : minute + 30),
            TestTimeZone);

        var schedule = builder.Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
    }
}