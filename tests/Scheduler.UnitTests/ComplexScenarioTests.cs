using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Enums;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class ComplexScenarioTests : BaseScheduleTests
{
    [Fact]
    public void ComplexScenario_QuarterlyBusinessMeeting_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 3),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o =>
            {
                o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 4, 7, 10 }));
                o.UseRelative(RelativeIndex.First, RelativePosition.Friday);
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(8).ToList();

        Assert.Equal(8, upcoming.Count);

        var months = upcoming.Select(o => o.Month).ToList();
        var expectedMonths = new[] { 1, 4, 7, 10, 1, 4, 7, 10 };
        Assert.Equal(expectedMonths, months);

        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Friday, o.DayOfWeek));
        Assert.All(upcoming, o => Assert.True(o.Day <= 7));
    }

    [Fact]
    public void ComplexScenario_BiWeeklyPayroll_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 3, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 3),
            TestTime(12, 0),
            TestTime(12, 30),
            TestTimeZone)
            .Recurring()
            .Weekly(o =>
            {
                o.Interval = 2;
                o.UseDaysOfWeek(list => list.Add(5));
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(8).ToList();

        Assert.Equal(8, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Friday, o.DayOfWeek));

        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date, PeriodUnits.Days);
            Assert.Equal(14, diff.Days);
        }
    }

    [Fact]
    public void ComplexScenario_MonthlyLastWorkingDay_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 31),
            TestTime(17, 0),
            TestTime(17, 30),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.Weekday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(12).ToList();

        Assert.Equal(12, upcoming.Count);

        Assert.All(upcoming, o => 
            Assert.True(o.DayOfWeek >= IsoDayOfWeek.Monday && o.DayOfWeek <= IsoDayOfWeek.Friday));

        Assert.All(upcoming, o => Assert.True(o.Day >= 25));

        var months = upcoming.Select(o => o.Month).ToList();
        Assert.Equal(Enumerable.Range(1, 12), months);
    }

    [Fact]
    public void ComplexScenario_SchoolSemesterSchedule_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 14, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 16),
            TestTime(10, 0),
            TestTime(11, 30),
            TestTimeZone)
            .Recurring(TestDate(2025, 5, 15))
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 2, 4 })))
            .Build();

        var allOccurrences = schedule.GetUpcomingOccurrences(100).ToList();

        Assert.True(allOccurrences.Count > 0);
        Assert.True(allOccurrences.Count < 100);

        Assert.All(allOccurrences, o => 
            Assert.True(o.DayOfWeek == IsoDayOfWeek.Tuesday || o.DayOfWeek == IsoDayOfWeek.Thursday));

        Assert.All(allOccurrences, o => Assert.True(o.Date <= TestDate(2025, 5, 15)));

        Assert.All(allOccurrences, o => 
            Assert.True(o.Date >= TestDate(2025, 1, 16) && o.Date <= TestDate(2025, 5, 15)));
    }

    [Fact]
    public void ComplexScenario_MaintenanceWindow_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 5, 12, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 5),
            TestTime(2, 0),
            TestTime(6, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Sunday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(12).ToList();

        Assert.Equal(12, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Sunday, o.DayOfWeek));
        Assert.All(upcoming, o => Assert.Equal(2, o.Hour));
        Assert.All(upcoming, o => Assert.True(o.Day <= 7));
        Assert.All(upcoming, o => Assert.Equal("04:00", schedule.OccurrenceDuration));
    }

    [Fact]
    public void ComplexScenario_ConferenceCall_MultipleTimeZones_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 6, 12, 0);
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6),
            TestTime(15, 0),
            TestTime(16, 0),
            easternTime)
            .Recurring()
            .Weekly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);

        Assert.All(upcoming, o => 
        {
            Assert.Equal(IsoDayOfWeek.Monday, o.DayOfWeek);
            Assert.Equal(15, o.Hour);
            Assert.Equal(easternTime, o.Zone);
        });

        var utcTimes = upcoming.Select(o => o.ToInstant()).ToList();
        for (int i = 0; i < utcTimes.Count - 1; i++)
        {
            var diff = utcTimes[i + 1] - utcTimes[i];
            Assert.Equal(Duration.FromDays(7), diff);
        }
    }

    [Fact]
    public void ComplexScenario_LeapYearHandling_ShouldWork()
    {
        var clock = CreateClock(2024, 2, 29, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2024, 2, 29),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Recurring()
            .Yearly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(10).ToList();

        Assert.All(upcoming, o => 
        {
            Assert.Equal(2, o.Month);
            Assert.Equal(29, o.Day);
            
            var year = o.Year;
            var isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
            Assert.True(isLeapYear);
        });

        var years = upcoming.Select(o => o.Year).ToList();
        var expectedYears = new[] { 2024, 2028, 2032, 2036, 2040, 2044, 2048, 2052, 2056, 2060 };
        Assert.Equal(expectedYears.Take(upcoming.Count), years);
    }

    [Fact]
    public void ComplexScenario_HolidaySchedule_ShouldWork()
    {
        var clock = CreateClock(2024, 11, 28, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2024, 11, 28),
            TestTime(12, 0),
            TestTime(15, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o =>
            {
                o.UseMonthsOfYear(list => list.Add(11));
                o.UseRelative(RelativeIndex.Fourth, RelativePosition.Thursday);
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.Equal(5, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(11, o.Month));
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Thursday, o.DayOfWeek));
        Assert.All(upcoming, o => Assert.True(o.Day >= 22 && o.Day <= 28));

        Assert.Equal(2024, upcoming[0].Year);
        Assert.Equal(2025, upcoming[1].Year);
        Assert.Equal(2026, upcoming[2].Year);
    }

    [Fact]
    public void ComplexScenario_EndOfMonthBilling_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 31, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 31),
            TestTime(23, 59),
            TestTime(23, 59),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.Day))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(12).ToList();

        Assert.Equal(12, upcoming.Count);

        var expectedDays = new[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        
        for (int i = 0; i < 12; i++)
        {
            Assert.Equal(i + 1, upcoming[i].Month);
            Assert.Equal(expectedDays[i], upcoming[i].Day);
            Assert.Equal(23, upcoming[i].Hour);
            Assert.Equal(59, upcoming[i].Minute);
        }
    }

    [Fact]
    public void ComplexScenario_WorkshiftRotation_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 6, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6),
            TestTime(7, 0),
            TestTime(19, 0),
            TestTimeZone)
            .Recurring()
            .Daily(o => o.Interval = 3)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(10).ToList();

        Assert.Equal(10, upcoming.Count);

        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date, PeriodUnits.Days);
            Assert.Equal(3, diff.Days);
        }

        var daysOfWeek = upcoming.Select(o => o.DayOfWeek).ToList();
        Assert.Equal(IsoDayOfWeek.Monday, daysOfWeek[0]);
        Assert.Equal(IsoDayOfWeek.Thursday, daysOfWeek[1]);
        Assert.Equal(IsoDayOfWeek.Sunday, daysOfWeek[2]);
        Assert.Equal(IsoDayOfWeek.Wednesday, daysOfWeek[3]);

        Assert.All(upcoming, o => 
        {
            Assert.Equal(7, o.Hour);
            Assert.Equal("12:00", schedule.OccurrenceDuration);
        });
    }
}