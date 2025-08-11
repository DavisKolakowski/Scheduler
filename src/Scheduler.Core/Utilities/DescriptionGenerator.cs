namespace Scheduler.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using NodaTime;

    using Scheduler.Core.Contracts;
    using Scheduler.Core.Enums;
    using Scheduler.Core.Models;
    using Scheduler.Core.Options;

    public static class DescriptionGenerator
    {
        public static string Generate(IScheduleOptions options)
        {
            return options switch
            {
                OneTimeOptions o => GenerateOneTimeDescription(o),
                DailyOptions o => GenerateRecurringDescription(o, "day", _ => null),
                WeeklyOptions o => GenerateRecurringDescription(o, "week", FormatWeeklyDetails),
                MonthlyOptions o => GenerateRecurringDescription(o, "month", FormatMonthlyDetails),
                YearlyOptions o => GenerateRecurringDescription(o, "year", FormatYearlyDetails),
                _ => $"A schedule of an unknown type starting on {options.StartDate}."
            };
        }

        private static string GenerateOneTimeDescription(OneTimeOptions options)
        {
            return $"Occurs once on {FormatDateWithOrdinal(options.StartDate)} at {FormatTimeRange(options.StartTime, options.EndTime)}";
        }

        private static string GenerateRecurringDescription<T>(T options, string baseUnit, Func<T, string?> formatDetailsFunc)
            where T : RecurringOptions
        {
            var sb = new StringBuilder();
            sb.Append($"Occurs {FormatInterval(options.Interval, baseUnit)}");

            var details = formatDetailsFunc(options);
            if (!string.IsNullOrEmpty(details))
            {
                sb.Append($" {details}");
            }

            sb.Append($" at {FormatTimeRange(options.StartTime, options.EndTime)}");
            sb.Append($" starting on {FormatDateWithOrdinal(options.StartDate)}");

            if (options.EndDate.HasValue)
            {
                sb.Append($" until {FormatDateWithOrdinal(options.EndDate.Value)}");
            }

            return sb.ToString();
        }

        private static string? FormatWeeklyDetails(WeeklyOptions options)
        {
            if (options.DaysOfWeek.Count == 0) return null;
            return $"on {FormatDayOfWeekList(options.DaysOfWeek)}";
        }

        private static string? FormatMonthlyDetails(MonthlyOptions options)
        {
            if (options.IsRelative && options.Relative.HasValue)
            {
                return $"on the {FormatRelativeOccurrence(options.Relative.Value)}";
            }
            if (options.DaysOfMonth.Any())
            {
                return $"on the {FormatDayOfMonthList(options.DaysOfMonth)}";
            }
            return null;
        }

        private static string? FormatYearlyDetails(YearlyOptions options)
        {
            var sb = new StringBuilder();
            if (options.Months.Any())
            {
                sb.Append($"in {FormatMonthList(options.Months)} ");
            }

            if (options.IsRelative && options.Relative.HasValue)
            {
                sb.Append($"on the {FormatRelativeOccurrence(options.Relative.Value)}");
            }
            else if (options.DaysOfMonth.Any())
            {
                sb.Append($"on the {FormatDayOfMonthList(options.DaysOfMonth)}");
            }

            return sb.ToString().Trim();
        }

        private static string FormatInterval(int interval, string unit)
        {
            if (interval == 1) return unit == "day" ? "daily" : $"{unit}ly";
            return $"every {interval} {unit}s";
        }

        private static string FormatTimeRange(LocalTime start, LocalTime end)
        {
            var culture = CultureInfo.InvariantCulture;
            return $"{start.ToString("h:mm tt", culture).ToLower()}-{end.ToString("h:mm tt", culture).ToLower()}";
        }

        private static string FormatDayOfWeekList(IReadOnlyList<int> days) => ToFormattedString(days.OrderBy(d => d).Select(d => ((IsoDayOfWeek)d).ToString()).ToList());
        private static string FormatMonthList(IReadOnlyList<int> months) => ToFormattedString(months.OrderBy(m => m).Select(m => CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m)).ToList());
        private static string FormatDayOfMonthList(IReadOnlyList<int> days) => ToFormattedString(days.OrderBy(d => d).Select(GetOrdinal).ToList());
        private static string FormatRelativeOccurrence(RelativeOccurrence occurrence)
        {
            var index = occurrence.Index.ToString().ToLower();
            var position = occurrence.Position.ToString();

            var formattedPosition = string.Concat(position.Select(c => char.IsUpper(c) ? " " + c : c.ToString())).TrimStart().ToLower();

            return $"{index} {formattedPosition}";
        }
        private static string FormatDateWithOrdinal(LocalDate date) => $"{date.ToString("MMMM", CultureInfo.InvariantCulture)} {GetOrdinal(date.Day)}, {date.Year}";
        private static string GetOrdinal(int num)
        {
            if (num <= 0) return num.ToString();
            switch (num % 100) { case 11: case 12: case 13: return num + "th"; }
            switch (num % 10) { case 1: return num + "st"; case 2: return num + "nd"; case 3: return num + "rd"; default: return num + "th"; }
        }
        private static string ToFormattedString(IReadOnlyList<string> items)
        {
            if (items.Count == 0) return string.Empty;
            if (items.Count == 1) return items[0];
            return string.Join(", ", items.Take(items.Count - 1)) + ", and " + items.Last();
        }
    }
}
