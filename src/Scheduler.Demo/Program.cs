using NodaTime;
using Scheduler.Core.Enums;
using Scheduler.Core;
using Scheduler.Core.Frequencies;
using System.Text.Json;

namespace Scheduler.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing new Schedule system with JSON output:");
        Console.WriteLine();

        var schedules = new List<object>();

        Console.WriteLine("=== ONE-TIME EXAMPLE ===");
        var oneTimeBuilder = Schedule<OneTime>.CreateBuilder(
            new LocalDate(2024, 8, 5),
            new LocalTime(14, 0),
            new LocalTime(16, 0),
            DateTimeZoneProviders.Tzdb["America/New_York"]
        );
        var oneTimeSchedule = oneTimeBuilder.Build();
        
        var oneTimeData = new
        {
            StartDate = oneTimeSchedule.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = oneTimeSchedule.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            EndTime = oneTimeSchedule.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            ExpirationDate = oneTimeSchedule.ExpirationDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeZone = oneTimeSchedule.TimeZone.Id,
            Frequency = new
            {
                Type = "OneTime",
                Settings = new { }
            },
            Description = oneTimeSchedule.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(oneTimeData, new JsonSerializerOptions { WriteIndented = true }));
        schedules.Add(oneTimeData);
        Console.WriteLine();

        Console.WriteLine("=== DAILY, DEFAULT INTERVAL ===");
        var dailyBuilder = Schedule<Daily>.CreateBuilder(
            new LocalDate(2024, 8, 5),
            new LocalTime(8, 0),
            new LocalTime(10, 0),
            DateTimeZoneProviders.Tzdb["UTC"]
        );
        dailyBuilder.Configure(d => d.ExpirationDate = new LocalDate(2024, 8, 31));
        var dailySchedule = dailyBuilder.Build();
        
        var dailyData = new
        {
            StartDate = dailySchedule.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = dailySchedule.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            EndTime = dailySchedule.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            ExpirationDate = dailySchedule.ExpirationDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeZone = dailySchedule.TimeZone.Id,
            Frequency = new
            {
                Type = "Daily",
                Settings = new
                {
                    Interval = dailySchedule.Frequency.Interval
                }
            },
            Description = dailySchedule.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(dailyData, new JsonSerializerOptions { WriteIndented = true }));
        schedules.Add(dailyData);
        Console.WriteLine();

        Console.WriteLine("=== DAILY, INTERVAL > 1 ===");
        var daily2Builder = Schedule<Daily>.CreateBuilder(
            new LocalDate(2024, 8, 5),
            new LocalTime(8, 0),
            new LocalTime(10, 0),
            DateTimeZoneProviders.Tzdb["UTC"]
        );
        daily2Builder.Configure(d => {
            d.Interval = 3;
            d.ExpirationDate = new LocalDate(2024, 8, 31);
        });
        var daily2Schedule = daily2Builder.Build();
        
        var daily2Data = new
        {
            StartDate = daily2Schedule.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = daily2Schedule.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            EndTime = daily2Schedule.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            ExpirationDate = daily2Schedule.ExpirationDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeZone = daily2Schedule.TimeZone.Id,
            Frequency = new
            {
                Type = "Daily",
                Settings = new
                {
                    Interval = daily2Schedule.Frequency.Interval
                }
            },
            Description = daily2Schedule.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(daily2Data, new JsonSerializerOptions { WriteIndented = true }));
        schedules.Add(daily2Data);
        Console.WriteLine();

        Console.WriteLine("=== WEEKLY, WEEKDAYS ONLY ===");
        var weekdaysBuilder = Schedule<Weekly>.CreateBuilder(
            new LocalDate(2024, 8, 5), // Monday
            new LocalTime(13, 0),
            new LocalTime(15, 0),
            DateTimeZoneProviders.Tzdb["UTC"]
        );
        weekdaysBuilder.Configure(w => {
            w.ExpirationDate = new LocalDate(2024, 12, 31);
            w.Days.Add(1); // Monday
            w.Days.Add(2); // Tuesday
            w.Days.Add(3); // Wednesday
            w.Days.Add(4); // Thursday
            w.Days.Add(5); // Friday
        });
        var weekdaysSchedule = weekdaysBuilder.Build();
        
        var weekdaysData = new
        {
            StartDate = weekdaysSchedule.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = weekdaysSchedule.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            EndTime = weekdaysSchedule.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            ExpirationDate = weekdaysSchedule.ExpirationDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeZone = weekdaysSchedule.TimeZone.Id,
            Frequency = new
            {
                Type = "Weekly",
                Settings = new
                {
                    Interval = weekdaysSchedule.Frequency.Interval,
                    Days = weekdaysSchedule.Frequency.Days
                }
            },
            Description = weekdaysSchedule.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(weekdaysData, new JsonSerializerOptions { WriteIndented = true }));
        schedules.Add(weekdaysData);
        Console.WriteLine();

        Console.WriteLine("=== WEEKLY, WEEKEND DAYS ONLY ===");
        var weekendsBuilder = Schedule<Weekly>.CreateBuilder(
            new LocalDate(2024, 8, 10), // Saturday
            new LocalTime(11, 0),
            new LocalTime(13, 0),
            DateTimeZoneProviders.Tzdb["UTC"]
        );
        weekendsBuilder.Configure(w => {
            w.Days.Add(6); // Saturday
            w.Days.Add(7); // Sunday
        });
        var weekendsSchedule = weekendsBuilder.Build();
        
        var weekendsData = new
        {
            StartDate = weekendsSchedule.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = weekendsSchedule.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            EndTime = weekendsSchedule.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            ExpirationDate = weekendsSchedule.ExpirationDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeZone = weekendsSchedule.TimeZone.Id,
            Frequency = new
            {
                Type = "Weekly",
                Settings = new
                {
                    Interval = weekendsSchedule.Frequency.Interval,
                    Days = weekendsSchedule.Frequency.Days
                }
            },
            Description = weekendsSchedule.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(weekendsData, new JsonSerializerOptions { WriteIndented = true }));
        schedules.Add(weekendsData);
        Console.WriteLine();

        Console.WriteLine("=== WEEKLY, INTERVAL > 1 ===");
        var biweeklyBuilder = Schedule<Weekly>.CreateBuilder(
            new LocalDate(2024, 8, 5),
            new LocalTime(9, 0),
            new LocalTime(11, 0),
            DateTimeZoneProviders.Tzdb["UTC"]
        );
        biweeklyBuilder.Configure(w => {
            w.Interval = 2;
            w.Days.Add(1);
        });
        var biweeklySchedule = biweeklyBuilder.Build();
        
        var biweeklyData = new
        {
            StartDate = biweeklySchedule.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = biweeklySchedule.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            EndTime = biweeklySchedule.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            ExpirationDate = biweeklySchedule.ExpirationDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeZone = biweeklySchedule.TimeZone.Id,
            Frequency = new
            {
                Type = "Weekly",
                Settings = new
                {
                    Interval = biweeklySchedule.Frequency.Interval,
                    Days = biweeklySchedule.Frequency.Days
                }
            },
            Description = biweeklySchedule.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(biweeklyData, new JsonSerializerOptions { WriteIndented = true }));
        schedules.Add(biweeklyData);
        Console.WriteLine();

        Console.WriteLine("=== MONTHLY, DEFAULT INTERVAL ===");
        var monthlyBuilder = Schedule<Monthly>.CreateBuilder(
            new LocalDate(2024, 8, 5),
            new LocalTime(18, 0),
            new LocalTime(21, 0),
            DateTimeZoneProviders.Tzdb["UTC"]
        );
        var monthlySchedule = monthlyBuilder.Build();
        
        var monthlyData = new
        {
            StartDate = monthlySchedule.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = monthlySchedule.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            EndTime = monthlySchedule.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            ExpirationDate = monthlySchedule.ExpirationDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeZone = monthlySchedule.TimeZone.Id,
            Frequency = new
            {
                Type = "Monthly",
                Settings = new
                {
                    Interval = monthlySchedule.Frequency.Interval,
                    DayOfMonth = monthlySchedule.Frequency.DayOfMonth > 0 ? monthlySchedule.Frequency.DayOfMonth : monthlySchedule.StartDate.Day
                }
            },
            Description = monthlySchedule.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(monthlyData, new JsonSerializerOptions { WriteIndented = true }));
        schedules.Add(monthlyData);
        Console.WriteLine();

        Console.WriteLine("=== MONTHLY, INTERVAL > 1 ===");
        var bimonthlyBuilder = Schedule<Monthly>.CreateBuilder(
            new LocalDate(2024, 8, 5),
            new LocalTime(18, 0),
            new LocalTime(21, 0),
            DateTimeZoneProviders.Tzdb["UTC"]
        );
        bimonthlyBuilder.Configure(m => m.Interval = 2);
        var bimonthlySchedule = bimonthlyBuilder.Build();
        
        var bimonthlyData = new
        {
            StartDate = bimonthlySchedule.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = bimonthlySchedule.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            EndTime = bimonthlySchedule.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            ExpirationDate = bimonthlySchedule.ExpirationDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeZone = bimonthlySchedule.TimeZone.Id,
            Frequency = new
            {
                Type = "Monthly",
                Settings = new
                {
                    Interval = bimonthlySchedule.Frequency.Interval,
                    DayOfMonth = bimonthlySchedule.Frequency.DayOfMonth > 0 ? bimonthlySchedule.Frequency.DayOfMonth : bimonthlySchedule.StartDate.Day
                }
            },
            Description = bimonthlySchedule.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(bimonthlyData, new JsonSerializerOptions { WriteIndented = true }));
        schedules.Add(bimonthlyData);
        Console.WriteLine();

        Console.WriteLine("=== MONTHLY, RELATIVE (FIRST FRIDAY) ===");
        var relativeMonthlyBuilder = Schedule<Monthly>.CreateBuilder(
            new LocalDate(2024, 8, 5),
            new LocalTime(17, 0),
            new LocalTime(19, 0),
            DateTimeZoneProviders.Tzdb["UTC"]
        );
        relativeMonthlyBuilder.Configure(m => {
            m.UseRelative(DayOfWeekIndex.First, DayOfWeekType.Friday);
            m.ExpirationDate = new LocalDate(2024, 12, 31);
        });
        var relativeMonthlySchedule = relativeMonthlyBuilder.Build();
        
        var relativeMonthlyData = new
        {
            StartDate = relativeMonthlySchedule.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = relativeMonthlySchedule.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            EndTime = relativeMonthlySchedule.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            ExpirationDate = relativeMonthlySchedule.ExpirationDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeZone = relativeMonthlySchedule.TimeZone.Id,
            Frequency = new
            {
                Type = "Monthly",
                Settings = new
                {
                    Interval = relativeMonthlySchedule.Frequency.Interval,
                    RelativeOptions = relativeMonthlySchedule.Frequency.RelativeOptions != null ? new
                    {
                        RelativeIndex = (int)relativeMonthlySchedule.Frequency.RelativeOptions.RelativeIndex,
                        RelativeDayOfWeek = (int)relativeMonthlySchedule.Frequency.RelativeOptions.RelativeDayOfWeek
                    } : null
                }
            },
            Description = relativeMonthlySchedule.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(relativeMonthlyData, new JsonSerializerOptions { WriteIndented = true }));
        schedules.Add(relativeMonthlyData);
        Console.WriteLine();

        Console.WriteLine("=== YEARLY, RELATIVE (LAST WEEKEND DAY OF JUNE AND DECEMBER) ===");
        var yearlyBuilder = Schedule<Yearly>.CreateBuilder(
            new LocalDate(2024, 6, 29),
            new LocalTime(10, 0),
            new LocalTime(12, 0),
            DateTimeZoneProviders.Tzdb["UTC"]
        );
        yearlyBuilder.Configure(y => {
            y.Months.Add(6);  // June
            y.Months.Add(12); // December
            y.UseRelative(DayOfWeekIndex.Last, DayOfWeekType.WeekendDay);
        });
        var yearlySchedule = yearlyBuilder.Build();
        
        var yearlyData = new
        {
            StartDate = yearlySchedule.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            StartTime = yearlySchedule.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            EndTime = yearlySchedule.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            ExpirationDate = yearlySchedule.ExpirationDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TimeZone = yearlySchedule.TimeZone.Id,
            Frequency = new
            {
                Type = "Yearly",
                Settings = new
                {
                    Interval = yearlySchedule.Frequency.Interval,
                    Months = yearlySchedule.Frequency.Months,
                    RelativeOptions = yearlySchedule.Frequency.RelativeOptions != null ? new
                    {
                        RelativeIndex = (int)yearlySchedule.Frequency.RelativeOptions.RelativeIndex,
                        RelativeDayOfWeek = (int)yearlySchedule.Frequency.RelativeOptions.RelativeDayOfWeek
                    } : null
                }
            },
            Description = yearlySchedule.ToString()
        };
        
        Console.WriteLine(JsonSerializer.Serialize(yearlyData, new JsonSerializerOptions { WriteIndented = true }));
        schedules.Add(yearlyData);
        Console.WriteLine();

        Console.WriteLine("=== ALL SCHEDULES AS JSON ARRAY ===");
        Console.WriteLine(JsonSerializer.Serialize(schedules, new JsonSerializerOptions { WriteIndented = true }));
        
        Console.WriteLine("All examples completed!");
    }
}
