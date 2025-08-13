using NodaTime;

namespace Scheduler.UnitTests;

public class WeeklyScheduleTests : BaseScheduleTests
{
    [Fact]
    public void WeeklySchedule_BasicWeekly_ShouldReturnCorrectOccurrences()
    {
        var clock = CreateClock(2025, 8, 5, 8, 0);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 8, 5),
            TestTime(18, 0),
            TestTime(19, 0),
            TestTimeZone)
            .Weekly()
            .Build();

        var nextOccurrence = schedule.NextOccurrence;
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal("Weekly", schedule.Type);
        Assert.NotNull(nextOccurrence);
        Assert.Equal(4, upcoming.Count);
        
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Tuesday, o.DayOfWeek));
        
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date);
            Assert.Equal(7, diff.Days);
        }
    }

    [Fact]
    public void WeeklySchedule_MultipleDaysOfWeek_ShouldReturnCorrectOccurrences()
    {
        var clock = CreateClock(2025, 8, 4, 8, 0);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 8, 4),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        Assert.Equal(IsoDayOfWeek.Monday, upcoming[0].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Wednesday, upcoming[1].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Friday, upcoming[2].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Monday, upcoming[3].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Wednesday, upcoming[4].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Friday, upcoming[5].DayOfWeek);
    }

    [Fact]
    public void WeeklySchedule_BiWeekly_ShouldSkipAlternateWeeks()
    {
        var clock = CreateClock(2025, 8, 3, 8, 0);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 8, 3),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Weekly(o =>
            {
                o.Interval = 2;
                o.UseDaysOfWeek(list => list.AddRange(new[] { 7, 1 }));
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        Assert.Equal(3, upcoming[0].Day);
        Assert.Equal(4, upcoming[1].Day);
        Assert.Equal(17, upcoming[2].Day);
        Assert.Equal(18, upcoming[3].Day);
        Assert.Equal(31, upcoming[4].Day);
        Assert.Equal(1, upcoming[5].Day);
        Assert.Equal(9, upcoming[5].Month);
    }

    [Fact]
    public void WeeklySchedule_StartInMiddleOfWeek_ShouldHandleCorrectly()
    {
        var clock = CreateClock(2025, 8, 6, 8, 0);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 8, 6),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.Equal(IsoDayOfWeek.Wednesday, upcoming[0].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Friday, upcoming[1].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Monday, upcoming[2].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Wednesday, upcoming[3].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Friday, upcoming[4].DayOfWeek);
    }

    [Fact]
    public void WeeklySchedule_AllDaysOfWeek_ShouldWork()
    {
        var clock = CreateClock(2025, 8, 4, 8, 0);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 8, 4),
            TestTime(9, 0),
            TestTime(10, 0),
            TestTimeZone)
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(7).ToList();

        Assert.Equal(7, upcoming.Count);
        
        for (int i = 0; i < 7; i++)
        {
            Assert.Equal((IsoDayOfWeek)(i + 1), upcoming[i].DayOfWeek);
        }
    }

    [Fact]
    public void WeeklySchedule_InvalidDaysOfWeek_ShouldIgnore()
    {
        var clock = CreateClock(2025, 8, 4, 8, 0);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 8, 4),
            TestTime(9, 0),
            TestTime(10, 0),
            TestTimeZone)
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 0, 1, 8, 3, -1, 5 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(IsoDayOfWeek.Monday, upcoming[0].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Wednesday, upcoming[1].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Friday, upcoming[2].DayOfWeek);
    }

    [Fact]
    public void WeeklySchedule_SingleDaySpecified_ShouldWork()
    {
        var clock = CreateClock(2025, 8, 6, 8, 0);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 8, 6),
            TestTime(14, 0),
            TestTime(15, 0),
            TestTimeZone)
            .Weekly(o => o.UseDaysOfWeek(list => list.Add(3)))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Wednesday, occurrence.DayOfWeek);
        }
    }

    [Fact]
    public void WeeklySchedule_AcrossMonthBoundary_ShouldWork()
    {
        var clock = CreateClock(2025, 7, 28, 8, 0);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 7, 28),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 5 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        Assert.Equal(7, upcoming[0].Month);
        Assert.Equal(8, upcoming[1].Month);
        Assert.Equal(8, upcoming[2].Month);
        Assert.Equal(8, upcoming[3].Month);
    }

    [Fact]
    public void WeeklySchedule_WithEndDate_ShouldStopCorrectly()
    {
        var clock = CreateClock(2025, 8, 4, 8, 0);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 8, 4),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone,
            TestDate(2025, 8, 15))
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(20).ToList();

        Assert.True(upcoming.Count <= 6);
        Assert.True(upcoming.All(o => o.Date <= TestDate(2025, 8, 15)));
    }

    [Fact]
    public void WeeklySchedule_DuringActiveOccurrence_ShouldReturnCurrent()
    {
        var clock = CreateClock(2025, 8, 6, 12, 30);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 8, 6),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Weekly(o => o.UseDaysOfWeek(list => list.Add(3)))
            .Build();

        var nextOccurrence = schedule.NextOccurrence;

        Assert.NotNull(nextOccurrence);
        Assert.Equal(6, nextOccurrence.Value.Day);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void WeeklySchedule_EachDayOfWeek_ShouldWork(int dayOfWeek)
    {
        var clock = CreateClock(2025, 8, 4, 8, 0);
        var factory = CreateContext(clock);
        
        var schedule = factory.CreateBuilder(
            TestDate(2025, 8, 4),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Weekly(o => o.UseDaysOfWeek(list => list.Add(dayOfWeek)))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(3, upcoming.Count);
        foreach (var occurrence in upcoming)
        {
            Assert.Equal((IsoDayOfWeek)dayOfWeek, occurrence.DayOfWeek);
        }
    }

    [Fact]
    public void WeeklySchedule_Description_ShouldBeCorrect()
    {
        var clock = CreateClock(2025, 8, 5, 8, 0);
        var factory = CreateContext(clock);
        
        var weeklySchedule = factory.CreateBuilder(
            TestDate(2025, 8, 5),
            TestTime(18, 0),
            TestTime(19, 0),
            TestTimeZone)
            .Weekly()
            .Build();
            
        var multiDaySchedule = factory.CreateBuilder(
            TestDate(2025, 8, 5),
            TestTime(18, 0),
            TestTime(19, 0),
            TestTimeZone)
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 })))
            .Build();

        var weeklyDescription = weeklySchedule.Description;
        var multiDayDescription = multiDaySchedule.Description;

        Assert.Contains("weekly", weeklyDescription);
        Assert.Contains("on Tuesday", weeklyDescription);
        Assert.Contains("on Monday, Wednesday, and Friday", multiDayDescription);
        Assert.Contains("6:00 PM - 7:00 PM", weeklyDescription);
    }
}