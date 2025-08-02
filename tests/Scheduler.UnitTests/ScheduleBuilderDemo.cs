using Scheduler.Core;
using Scheduler.Core.Enums;
using NodaTime;
using System;

namespace Scheduler.UnitTests
{
    /// <summary>
    /// Demo class showing all the use cases working with the Schedule Builder API
    /// </summary>
    public class ScheduleBuilderDemo
    {
        private readonly DateTimeZone _timeZone = DateTimeZone.Utc;

        public void DemonstrateAllUseCases()
        {
            // USE CASE 1: One-Time Occurrence (No Recurrence, No End Date)
            var useCase1 = Schedule.CreateBuilder(new LocalDate(2024, 1, 15), new LocalTime(15, 0), new LocalTime(17, 0), _timeZone)
                .Build();

            // USE CASE 2: One-Time Occurrence (No Recurrence, With End Date)
            var useCase2 = Schedule.CreateBuilder(new LocalDate(2024, 1, 15), new LocalTime(15, 0), new LocalTime(17, 0), _timeZone)
                .AddEndDate(new LocalDate(2024, 1, 15))
                .Build();

            // USE CASE 3: Simple Daily (No Configuration Needed)
            var useCase3 = Schedule.CreateBuilder(new LocalDate(2024, 1, 15), new LocalTime(15, 0), new LocalTime(17, 0), _timeZone)
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseDaily(); // Defaults: Interval = 1
                })
                .Build();

            // USE CASE 4: Every 3 Days
            var useCase4 = Schedule.CreateBuilder(new LocalDate(2024, 1, 15), new LocalTime(15, 0), new LocalTime(17, 0), _timeZone)
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseDaily(dailyOptions =>
                    {
                        dailyOptions.Interval = 3;
                    });
                })
                .Build();

            // USE CASE 5: Simple Weekly (Same Day as Start Date)
            var useCase5 = Schedule.CreateBuilder(new LocalDate(2024, 1, 10), new LocalTime(19, 0), new LocalTime(22, 0), _timeZone) // Wednesday
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseWeekly(); // Defaults: Interval = 1, occurs on Wednesdays
                })
                .Build();

            // USE CASE 6: Weekdays Only
            var useCase6 = Schedule.CreateBuilder(new LocalDate(2024, 2, 1), new LocalTime(11, 30), new LocalTime(14, 0), _timeZone)
                .AddEndDate(new LocalDate(2024, 8, 1))
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseWeekly(weeklyOptions =>
                    {
                        weeklyOptions.Days.Add(1); // Monday
                        weeklyOptions.Days.Add(2); // Tuesday
                        weeklyOptions.Days.Add(3); // Wednesday
                        weeklyOptions.Days.Add(4); // Thursday
                        weeklyOptions.Days.Add(5); // Friday
                    });
                })
                .Build();

            // USE CASE 7: Bi-weekly Sunday & Monday
            var useCase7 = Schedule.CreateBuilder(new LocalDate(2024, 3, 3), new LocalTime(10, 0), new LocalTime(15, 0), _timeZone)
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseWeekly(weeklyOptions =>
                    {
                        weeklyOptions.Interval = 2;
                        weeklyOptions.Days.Add(7); // Sunday
                        weeklyOptions.Days.Add(1); // Monday
                    });
                })
                .Build();

            // USE CASE 8: Simple Monthly (Same Date Each Month)
            var useCase8 = Schedule.CreateBuilder(new LocalDate(2024, 1, 15), new LocalTime(18, 0), new LocalTime(21, 0), _timeZone)
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseMonthly(); // Defaults: Interval = 1, absolute date (15th each month)
                })
                .Build();

            // USE CASE 9: Every 2 Months on Same Date
            var useCase9 = Schedule.CreateBuilder(new LocalDate(2024, 1, 15), new LocalTime(18, 0), new LocalTime(21, 0), _timeZone)
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseMonthly(monthlyOptions =>
                    {
                        monthlyOptions.Interval = 2; // Still absolute to 15th, but every 2 months
                    });
                })
                .Build();

            // USE CASE 10: Monthly - First Friday
            var useCase10 = Schedule.CreateBuilder(new LocalDate(2024, 2, 2), new LocalTime(18, 0), new LocalTime(21, 0), _timeZone)
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseMonthly(monthlyOptions =>
                    {
                        monthlyOptions.UseRelativeMonthly(DayOfWeekIndex.First, DayOfWeekType.Friday);
                    });
                })
                .Build();

            // USE CASE 11: Quarterly - Last Weekend Day
            var useCase11 = Schedule.CreateBuilder(new LocalDate(2024, 1, 28), new LocalTime(14, 0), new LocalTime(16, 0), _timeZone)
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseMonthly(monthlyOptions =>
                    {
                        monthlyOptions.Interval = 3;
                        monthlyOptions.UseRelativeMonthly(DayOfWeekIndex.Last, DayOfWeekType.WeekendDay);
                    });
                })
                .Build();

            // USE CASE 12: Simple Yearly (Same Date Each Year)
            var useCase12 = Schedule.CreateBuilder(new LocalDate(2024, 12, 20), new LocalTime(18, 0), new LocalTime(23, 0), _timeZone)
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseYearly(); // Defaults: Interval = 1, December 20th each year
                })
                .Build();

            // USE CASE 13: Every 2 Years - January & December - First Weekend Day
            var useCase13 = Schedule.CreateBuilder(new LocalDate(2024, 1, 6), new LocalTime(9, 0), new LocalTime(17, 0), _timeZone)
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseYearly(yearlyOptions =>
                    {
                        yearlyOptions.Interval = 2;
                        yearlyOptions.UseRelativeYearly(relativeYearlyOptions =>
                        {
                            relativeYearlyOptions.Months.Add(1);  // January
                            relativeYearlyOptions.Months.Add(12); // December
                        })
                        .OnDaysOfWeek(DayOfWeekIndex.First, DayOfWeekType.WeekendDay);
                    });
                })
                .Build();

            // USE CASE 14: Yearly - Multiple Months (No Day Restriction)
            var useCase14 = Schedule.CreateBuilder(new LocalDate(2024, 6, 15), new LocalTime(10, 0), new LocalTime(16, 0), _timeZone)
                .AddRecurrence(frequencyOptions =>
                {
                    frequencyOptions.UseYearly(yearlyOptions =>
                    {
                        yearlyOptions.UseRelativeYearly(relativeYearlyOptions =>
                        {
                            relativeYearlyOptions.Months.Add(6);  // June
                            relativeYearlyOptions.Months.Add(12); // December
                        });
                        // No OnDaysOfWeek means same date (15th) in those months
                    });
                })
                .Build();

            // All use cases are now successfully created!
            Console.WriteLine("All 14 use cases have been successfully implemented!");
        }
    }
}