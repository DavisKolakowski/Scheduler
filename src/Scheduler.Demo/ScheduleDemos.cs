using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using NodaTime.Testing;

using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Enums;

namespace Scheduler.Demo;
public static class ScheduleDemos
{
    private static readonly IClock _clock = new FakeClock(Instant.FromUtc(2025, 8, 9, 19, 53, 05));
    private static readonly ScheduleContext _context = new ScheduleContext(_clock);
    private static readonly DateTimeZone _tz = DateTimeZoneProviders.Tzdb["America/New_York"];

    #region Demo Definitions

    // --- One-Time Schedules ---
    private static ISchedule<IScheduleOptions> Demo1_OneTimeEvent()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2024, 8, 20),
            new LocalTime(10, 0),
            new LocalTime(11, 30),
            _tz);

        var schedule = builder.Build();
        return schedule;
    }

    private static ISchedule<IScheduleOptions> Demo1a_OvernightOneTimeEvent()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2024, 8, 20),
            new LocalTime(22, 0),
            new LocalTime(2, 0),
            _tz);

        var schedule = builder.Build();
        return schedule;
    }

    // --- Daily Schedules ---
    private static ISchedule<IScheduleOptions> Demo2_DailyRecurring()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 8, 1),
            new LocalTime(9, 0),
            new LocalTime(17, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Daily();

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    private static ISchedule<IScheduleOptions> Demo3_Every3DaysWithEndDate()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 8, 1),
            new LocalTime(9, 0),
            new LocalTime(17, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring(new LocalDate(2025, 8, 30))
            .Daily(o => o.Interval = 3);

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    // --- Weekly Schedules ---
    private static ISchedule<IScheduleOptions> Demo4_WeeklyOnTuesdays()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 8, 5),
            new LocalTime(18, 0),
            new LocalTime(19, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Weekly();

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    private static ISchedule<IScheduleOptions> Demo5_WeeklyOnMonWedFri()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 8, 4),
            new LocalTime(12, 0),
            new LocalTime(13, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 })));

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    private static ISchedule<IScheduleOptions> Demo6_BiWeeklyOnSunMon()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 8, 3),
            new LocalTime(10, 0),
            new LocalTime(11, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Weekly(o =>
            {
                o.Interval = 2;
                o.UseDaysOfWeek(list => list.AddRange(new[] { 7, 1 }));
            });

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    // --- Monthly Schedules ---
    private static ISchedule<IScheduleOptions> Demo7_MonthlyOnThe15th()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 7, 15),
            new LocalTime(20, 0),
            new LocalTime(21, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Monthly();

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    private static ISchedule<IScheduleOptions> Demo7a_MonthlyOnMultipleDays()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 7, 1),
            new LocalTime(12, 0),
            new LocalTime(12, 30),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 1, 15, 31 })));

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    private static ISchedule<IScheduleOptions> Demo8_MonthlyOnFirstFriday()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 8, 1),
            new LocalTime(18, 0),
            new LocalTime(21, 30),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Friday));

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    private static ISchedule<IScheduleOptions> Demo9_QuarterlyOnLastWeekendDay()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 1, 1),
            new LocalTime(14, 0),
            new LocalTime(16, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Monthly(o =>
            {
                o.Interval = 3;
                o.UseRelative(RelativeIndex.Last, RelativePosition.WeekendDay);
            });

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    // --- Yearly Schedules ---
    private static ISchedule<IScheduleOptions> Demo10_YearlyOnDec25()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2024, 12, 25),
            new LocalTime(8, 0),
            new LocalTime(20, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Yearly();

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    private static ISchedule<IScheduleOptions> Demo11_YearlyInJunDecOn15th()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 6, 15),
            new LocalTime(10, 0),
            new LocalTime(16, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 6, 12 })));

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    private static ISchedule<IScheduleOptions> Demo11a_YearlyInFebOnMultipleDays()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2025, 2, 1),
            new LocalTime(9, 0),
            new LocalTime(10, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Yearly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 10, 20 })));

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    private static ISchedule<IScheduleOptions> Demo12_Every2YearsOnFirstWeekendDay()
    {
        var builder = _context.CreateBuilder(
            new LocalDate(2024, 1, 6),
            new LocalTime(9, 0),
            new LocalTime(17, 0),
            _tz);

        var frequencyBuilder = builder
            .Recurring()
            .Yearly(o =>
            {
                o.Interval = 2;
                o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 12 }));
                o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay);
            });

        var schedule = frequencyBuilder.Build();
        return schedule;
    }

    #endregion

    public static void RunAll()
    {
        Console.WriteLine($"--- All tests run against fixed time: {_clock.GetCurrentInstant()} ---");
        Console.WriteLine();
        RunDemo("1. One-Time Event (Past)", Demo1_OneTimeEvent());
        RunDemo("1a. Overnight One-Time Event (Past)", Demo1a_OvernightOneTimeEvent());
        RunDemo("2. Daily Recurring", Demo2_DailyRecurring());
        RunDemo("3. Every 3 Days with End Date", Demo3_Every3DaysWithEndDate());
        RunDemo("4. Weekly on Tuesdays", Demo4_WeeklyOnTuesdays());
        RunDemo("5. Weekly on Mon, Wed, Fri", Demo5_WeeklyOnMonWedFri());
        RunDemo("6. Bi-Weekly (Every 2 Weeks) on Sun & Mon", Demo6_BiWeeklyOnSunMon());
        RunDemo("7. Monthly on the 15th", Demo7_MonthlyOnThe15th());
        RunDemo("7a. Monthly on Multiple Days (1st, 15th, 31st)", Demo7a_MonthlyOnMultipleDays());
        RunDemo("8. Monthly on First Friday", Demo8_MonthlyOnFirstFriday());
        RunDemo("9. Quarterly on the Last Weekend Day", Demo9_QuarterlyOnLastWeekendDay());
        RunDemo("10. Yearly on December 25th", Demo10_YearlyOnDec25());
        RunDemo("11. Yearly in June & December on the 15th", Demo11_YearlyInJunDecOn15th());
        RunDemo("11a. Yearly in Feb on Multiple Days (10th, 20th)", Demo11a_YearlyInFebOnMultipleDays());
        RunDemo("12. Every 2 Years in Jan & Dec on the First Weekend Day", Demo12_Every2YearsOnFirstWeekendDay());
    }

    private static void RunDemo(string title, ISchedule<IScheduleOptions> schedule)
    {
        Console.WriteLine($"--- {title} ---");

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        jsonOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        string json = JsonSerializer.Serialize(schedule, schedule.GetType(), jsonOptions);

        Console.WriteLine("--- JSON Object ---");
        Console.WriteLine(json);

        Console.WriteLine("--- Query Results ---");
        var current = schedule.GetNextOccurrence();
        Console.WriteLine($"GetNextOccurrence():    {(current.HasValue ? current.Value.ToString("G", null) : "null")}");

        var previous = schedule.GetPreviousOccurrence();
        Console.WriteLine($"GetPreviousOccurrence():   {(previous.HasValue ? previous.Value.ToString("G", null) : "null")}");

        Console.WriteLine("GetUpcomingOccurrences(3):");
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();
        if (upcoming.Any())
        {
            foreach (var occ in upcoming) { Console.WriteLine($"  - {occ:G}"); }
        }
        else
        {
            Console.WriteLine("  (none)");
        }

        Console.WriteLine("GetOccurrencesCompleted(3):");
        var completed = schedule.GetOccurrencesCompleted(3).ToList();
        if (completed.Any())
        {
            foreach (var occ in completed) { Console.WriteLine($"  - {occ:G}"); }
        }
        else
        {
            Console.WriteLine("  (none)");
        }
        Console.WriteLine();
    }
}