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
    using Scheduler.Core.Models.Schedules;
    using Scheduler.Core.Models.Schedules.Base;

    public static class DescriptionGenerator
    {
        public static string Generate(Frequency model)
        {
            return model switch
            {
                OneTime o => GenerateOneTimeDescription(o),
                Daily o => GenerateRecurringDescription(o, "day", _ => null),
                Weekly o => GenerateRecurringDescription(o, "week", FormatWeeklyDetails),
                Monthly o => GenerateRecurringDescription(o, "month", FormatMonthlyDetails),
                Yearly o => GenerateRecurringDescription(o, "year", FormatYearlyDetails),
                _ => $"A schedule of an unknown type starting on {model.StartDate}."
            };
        }

        private static string GenerateOneTimeDescription(OneTime model)
        {
            return $"Occurs once on {FormatDateWithOrdinal(model.StartDate)} at {FormatTimeRange(model.StartTime, model.EndTime)}";
        }

        private static string GenerateRecurringDescription<T>(T model, string baseUnit, Func<T, string?> formatDetailsFunc)
            where T : Recurring
        {
            var sb = new StringBuilder();
            sb.Append($"Occurs {FormatInterval(model.Interval, baseUnit)}");

            var details = formatDetailsFunc(model);
            if (!string.IsNullOrEmpty(details))
            {
                sb.Append($" {details}");
            }

            sb.Append($" at {FormatTimeRange(model.StartTime, model.EndTime)}");
            sb.Append($" starting on {FormatDateWithOrdinal(model.StartDate)}");

            if (model.EndDate.HasValue)
            {
                sb.Append($" until {FormatDateWithOrdinal(model.EndDate.Value)}");
            }

            return sb.ToString();
        }

        private static string? FormatWeeklyDetails(Weekly model)
        {
            if (model.DaysOfWeek.Count == 0) return null;
            return $"on {FormatDayOfWeekList(model.DaysOfWeek)}";
        }

        private static string? FormatMonthlyDetails(Monthly model)
        {
            if (model.IsRelative && model.Relative.HasValue)
            {
                return $"on the {FormatRelative(model.Relative.Value)}";
            }
            if (model.DaysOfMonth.Any())
            {
                return $"on the {FormatDayOfMonthList(model.DaysOfMonth)}";
            }
            return null;
        }

        private static string? FormatYearlyDetails(Yearly model)
        {
            var sb = new StringBuilder();
            if (model.Months.Any())
            {
                sb.Append($"in {FormatMonthList(model.Months)} ");
            }

            if (model.IsRelative && model.Relative.HasValue)
            {
                sb.Append($"on the {FormatRelative(model.Relative.Value)}");
            }
            else if (model.DaysOfMonth.Any())
            {
                sb.Append($"on the {FormatDayOfMonthList(model.DaysOfMonth)}");
            }

            return sb.ToString().Trim();
        }

        private static string FormatInterval(int interval, string unit)
        {
            if (interval == 1)
            {
                return unit == "day" ? "daily" : $"{unit}ly";
            }
            return $"every {interval} {unit}s";
        }

        private static string FormatTimeRange(LocalTime start, LocalTime end)
        {
            var culture = CultureInfo.InvariantCulture;
            return $"{start.ToString("h:mm tt", culture).ToUpper()} - {end.ToString("h:mm tt", culture).ToUpper()}";
        }

        private static string FormatDayOfWeekList(IReadOnlyList<int> days) => ToFormattedString(days.OrderBy(d => d).Select(d => ((IsoDayOfWeek)d).ToString()).ToList());
        private static string FormatMonthList(IReadOnlyList<int> months) => ToFormattedString(months.OrderBy(m => m).Select(m => CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m)).ToList());
        private static string FormatDayOfMonthList(IReadOnlyList<int> days) => ToFormattedString(days.OrderBy(d => d).Select(GetOrdinal).ToList());
        private static string FormatRelative(Relative occurrence)
        {
            var index = occurrence.Index.ToString().ToLower();
            var position = occurrence.Position.ToString();

            var formattedPosition = string.Concat(position.Select(c => char.IsUpper(c) ? " " + c : c.ToString())).TrimStart().ToLower();

            return $"{index} {formattedPosition}";
        }
        private static string FormatDateWithOrdinal(LocalDate date) => $"{date.ToString("MMMM", CultureInfo.InvariantCulture)} {GetOrdinal(date.Day)}, {date.Year}";
        private static string GetOrdinal(int num)
        {
            if (num <= 0)
            {
                return num.ToString();
            }
            switch (num % 100) 
            { 
                case 11: 
                case 12: 
                case 13: 
                    return num + "th"; 
            }
            switch (num % 10) 
            { 
                case 1: 
                    return num + "st"; 
                case 2: 
                    return num + "nd"; 
                case 3: 
                    return num + "rd"; 
                default: 
                    return num + "th"; }
        }
        private static string ToFormattedString(IReadOnlyList<string> items)
        {
            if (items.Count == 0)
            {
                return string.Empty;
            }
            if (items.Count == 1)
            {
                return items[0];
            }
            if (items.Count == 2)
            {
                return items[0] + " and " + items[1];
            }
            return string.Join(", ", items.Take(items.Count - 1)) + ", and " + items.Last();
        }
    }
}
