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
using Scheduler.Core.Factories;
using Scheduler.Core.Models.Schedules;
using Scheduler.Core.Models.Schedules.Base;

namespace Scheduler.Demo;
public static class ScheduleDemos
{
    private static readonly IClock _clock = new FakeClock(Instant.FromUtc(2025, 8, 9, 19, 53, 05));
    private static readonly ScheduleFactory _factory = new ScheduleFactory(_clock);
    private static readonly DateTimeZone _tz = DateTimeZoneProviders.Tzdb["America/New_York"];

    #region Demo Definitions

    // --- One-Time Schedules ---
    private static ISchedule<OneTime> Demo1_OneTimeEvent()
    {
        var schedule = _factory.Create(
            new LocalDate(2024, 8, 20),
            new LocalTime(10, 0),
            new LocalTime(11, 30),
            _tz)
            .OneTime()
            .Build();
        return schedule;
    }

    private static ISchedule<OneTime> Demo1a_OvernightOneTimeEvent()
    {
        var schedule = _factory.Create(
            new LocalDate(2024, 8, 20),
            new LocalTime(22, 0),
            new LocalTime(2, 0),
            _tz)
            .OneTime()
            .Build();
        return schedule;
    }

    // --- Daily Schedules ---
    private static ISchedule<Daily> Demo2_DailyRecurring()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 8, 1),
            new LocalTime(9, 0),
            new LocalTime(17, 0),
            _tz)
            .Daily()
            .Build();
        return schedule;
    }

    private static ISchedule<Daily> Demo3_Every3DaysWithEndDate()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 8, 1),
            new LocalTime(9, 0),
            new LocalTime(17, 0),
            _tz,
            new LocalDate(2025, 8, 30))
            .Daily(o => o.Interval = 3)
            .Build();
        return schedule;
    }

    // --- Weekly Schedules ---
    private static ISchedule<Weekly> Demo4_WeeklyOnTuesdays()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 8, 5),
            new LocalTime(18, 0),
            new LocalTime(19, 0),
            _tz)
            .Weekly()
            .Build();
        return schedule;
    }

    private static ISchedule<Weekly> Demo5_WeeklyOnMonWedFri()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 8, 4),
            new LocalTime(12, 0),
            new LocalTime(13, 0),
            _tz)
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 })))
            .Build();
        return schedule;
    }

    private static ISchedule<Weekly> Demo6_BiWeeklyOnSunMon()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 8, 3),
            new LocalTime(10, 0),
            new LocalTime(11, 0),
            _tz)
            .Weekly(o =>
            {
                o.Interval = 2;
                o.UseDaysOfWeek(list => list.AddRange(new[] { 7, 1 }));
            })
            .Build();
        return schedule;
    }

    // --- Monthly Schedules ---
    private static ISchedule<Monthly> Demo7_MonthlyOnThe15th()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 7, 15),
            new LocalTime(20, 0),
            new LocalTime(21, 0),
            _tz)
            .Monthly()
            .Build();
        return schedule;
    }

    private static ISchedule<Monthly> Demo7a_MonthlyOnMultipleDays()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 7, 1),
            new LocalTime(12, 0),
            new LocalTime(12, 30),
            _tz)
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 1, 15, 31 })))
            .Build();
        return schedule;
    }

    private static ISchedule<Monthly> Demo8_MonthlyOnFirstFriday()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 8, 1),
            new LocalTime(18, 0),
            new LocalTime(21, 30),
            _tz)
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Friday))
            .Build();
        return schedule;
    }

    private static ISchedule<Monthly> Demo9_QuarterlyOnLastWeekendDay()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 1, 1),
            new LocalTime(14, 0),
            new LocalTime(16, 0),
            _tz)
            .Monthly(o =>
            {
                o.Interval = 3;
                o.UseRelative(RelativeIndex.Last, RelativePosition.WeekendDay);
            })
            .Build();
        return schedule;
    }

    // --- Yearly Schedules ---
    private static ISchedule<Yearly> Demo10_YearlyOnDec25()
    {
        var schedule = _factory.Create(
            new LocalDate(2024, 12, 25),
            new LocalTime(8, 0),
            new LocalTime(20, 0),
            _tz)
            .Yearly()
            .Build();
        return schedule;
    }

    private static ISchedule<Yearly> Demo11_YearlyInJunDecOn15th()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 6, 15),
            new LocalTime(10, 0),
            new LocalTime(16, 0),
            _tz)
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 6, 12 })))
            .Build();
        return schedule;
    }

    private static ISchedule<Yearly> Demo11a_YearlyInFebOnMultipleDays()
    {
        var schedule = _factory.Create(
            new LocalDate(2025, 2, 1),
            new LocalTime(9, 0),
            new LocalTime(10, 0),
            _tz)
            .Yearly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 10, 20 })))
            .Build();
        return schedule;
    }

    private static ISchedule<Yearly> Demo12_Every2YearsOnFirstWeekendDay()
    {
        var schedule = _factory.Create(
            new LocalDate(2024, 1, 6),
            new LocalTime(9, 0),
            new LocalTime(17, 0),
            _tz)
            .Yearly(o =>
            {
                o.Interval = 2;
                o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 12 }));
                o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay);
            })
            .Build();
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

    private static void RunDemo<TModel>(string title, ISchedule<TModel> schedule) where TModel : Schedule
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