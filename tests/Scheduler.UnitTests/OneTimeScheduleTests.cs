using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Factories;

namespace Scheduler.UnitTests;

public class OneTimeScheduleTests : BaseScheduleTests
{
    [Fact]
    public void OneTimeSchedule_SameDay_ShouldReturnCorrectOccurrence()
    {
        var clock = CreateClock(2024, 8, 20, 9, 0);
        var factory = new ScheduleFactory(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(11, 30),
            TestTimeZone)
            .OneTime()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var previousOccurrence = schedule.GetPreviousOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();
        var completed = schedule.GetOccurrencesCompleted(5).ToList();

        Assert.Equal("OneTime", schedule.Type);
        Assert.NotNull(nextOccurrence);
        Assert.Null(previousOccurrence);
        Assert.Single(upcoming);
        Assert.Empty(completed);
    }

    [Fact]
    public void OneTimeSchedule_InPast_ShouldReturnPreviousOccurrence()
    {
        var clock = CreateClock(2024, 8, 20, 16, 0);
        var factory = new ScheduleFactory(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(11, 30),
            TestTimeZone)
            .OneTime()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var previousOccurrence = schedule.GetPreviousOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();
        var completed = schedule.GetOccurrencesCompleted(5).ToList();

        Assert.Null(nextOccurrence);
        Assert.NotNull(previousOccurrence);
        Assert.Empty(upcoming);
        Assert.Single(completed);
    }

    [Fact]
    public void OneTimeSchedule_CurrentlyActive_ShouldReturnAsNext()
    {
        var clock = CreateClock(2024, 8, 20, 10, 30);
        var factory = new ScheduleFactory(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(11, 30),
            TestTimeZone)
            .OneTime()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var previousOccurrence = schedule.GetPreviousOccurrence();

        Assert.NotNull(nextOccurrence);
        Assert.Null(previousOccurrence);
    }

    [Fact]
    public void OneTimeSchedule_OvernightEvent_ShouldHandleCorrectly()
    {
        var clock = CreateClock(2024, 8, 20, 21, 0);
        var factory = new ScheduleFactory(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(22, 0),
            TestTime(2, 0),
            TestTimeZone)
            .OneTime()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var occurrenceDuration = schedule.OccurrenceLength;

        Assert.NotNull(nextOccurrence);
        Assert.Equal("04:00", occurrenceDuration);
    }

    [Fact]
    public void OneTimeSchedule_DifferentTimeZones_ShouldRespectTimeZone()
    {
        var utcClock = CreateClock(2024, 8, 20, 14, 0);
        var factory = new ScheduleFactory(utcClock);
        
        var nySchedule = factory.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(11, 0),
            DateTimeZoneProviders.Tzdb["America/New_York"])
            .OneTime()
            .Build();

        var laSchedule = factory.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(11, 0),
            DateTimeZoneProviders.Tzdb["America/Los_Angeles"])
            .OneTime()
            .Build();

        var nyNext = nySchedule.GetNextOccurrence();
        var laNext = laSchedule.GetNextOccurrence();

        Assert.NotNull(nyNext);
        Assert.NotNull(laNext);
    }

    [Fact]
    public void OneTimeSchedule_Description_ShouldBeCorrect()
    {
        var clock = CreateClock(2024, 8, 20, 9, 0);
        var factory = new ScheduleFactory(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(11, 30),
            TestTimeZone)
            .OneTime()
            .Build();

        var description = schedule.Description;

        Assert.Contains("Occurs once", description);
        Assert.Contains("August 20th, 2024", description);
        Assert.Contains("10:00 AM - 11:30 AM", description);
    }

    [Fact]
    public void OneTimeSchedule_ZeroMinuteDuration_ShouldWork()
    {
        var clock = CreateClock(2024, 8, 20, 9, 0);
        var factory = new ScheduleFactory(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(10, 0),
            TestTime(10, 0),
            TestTimeZone)
            .OneTime()
            .Build();

        var duration = schedule.OccurrenceLength;

        Assert.Equal("00:00", duration);
    }

    [Fact]
    public void OneTimeSchedule_LeapYear_February29_ShouldWork()
    {
        var clock = CreateClock(2024, 2, 28, 9, 0);
        var factory = new ScheduleFactory(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2024, 2, 29),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .OneTime()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();

        Assert.NotNull(nextOccurrence);
        Assert.Equal(29, nextOccurrence.Value.Day);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(23, 59)]
    [InlineData(12, 30)]
    public void OneTimeSchedule_VariousStartTimes_ShouldWork(int hour, int minute)
    {
        var clock = CreateClock(2024, 8, 19, 23, 0);
        var factory = new ScheduleFactory(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2024, 8, 20),
            TestTime(hour, minute),
            TestTime(hour, minute + 30 > 59 ? 59 : minute + 30),
            TestTimeZone)
            .OneTime()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();

        Assert.NotNull(nextOccurrence);
    }
}