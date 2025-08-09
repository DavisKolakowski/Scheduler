namespace Scheduler.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NodaTime;
    using NodaTime.TimeZones;

    using Scheduler.Core.Contracts;
    using Scheduler.Core.Enums;
    using Scheduler.Core.Models;
    using Scheduler.Core.Options;

    public static class OccurrenceCalculator
    {
        public static IEnumerable<ZonedDateTime> GetOccurrences(IScheduleOptions options, Instant start, Instant end)
        {
            var zone = options.TimeZone;
            var searchStart = start.InZone(zone);
            var searchEnd = end.InZone(zone);
            var searchLimit = options.EndDate.HasValue ? Min(searchEnd.Date, options.EndDate.Value) : searchEnd.Date;
            var iterDate = Max(searchStart.Date, options.StartDate);

            switch (options)
            {
                case OneTimeOptions o:
                    var oneTimeZdt = o.StartDate.At(o.StartTime).InZone(zone, Resolvers.LenientResolver);
                    if (ZonedDateTime.Comparer.Instant.Compare(oneTimeZdt, searchStart) >= 0 && ZonedDateTime.Comparer.Instant.Compare(oneTimeZdt, searchEnd) <= 0)
                    {
                        yield return oneTimeZdt;
                    }
                    break;

                case DailyOptions o:
                    var dailyDate = options.StartDate;
                    if (dailyDate < iterDate)
                    {
                        var periodsSince = Period.Between(dailyDate, iterDate, PeriodUnits.Days).Days;
                        var intervalsSince = (int)Math.Ceiling((double)periodsSince / o.Interval);
                        dailyDate = options.StartDate.PlusDays(intervalsSince * o.Interval);
                    }
                    for (var d = dailyDate; d <= searchLimit; d = d.PlusDays(o.Interval))
                    {
                        yield return d.At(o.StartTime).InZone(zone, Resolvers.LenientResolver);
                    }
                    break;

                case WeeklyOptions o:
                    var startOfWeek = options.StartDate.With(DateAdjusters.PreviousOrSame(IsoDayOfWeek.Monday));
                    for (var d = iterDate; d <= searchLimit; d = d.PlusDays(1))
                    {
                        var currentStartOfWeek = d.With(DateAdjusters.PreviousOrSame(IsoDayOfWeek.Monday));
                        var weeksBetween = Period.Between(startOfWeek, currentStartOfWeek, PeriodUnits.Weeks).Weeks;

                        if (weeksBetween % o.Interval == 0 && o.DaysOfWeek.Contains((int)d.DayOfWeek))
                        {
                            yield return d.At(o.StartTime).InZone(zone, Resolvers.LenientResolver);
                        }
                    }
                    break;

                case MonthlyOptions o:
                    var monthlyDate = new LocalDate(iterDate.Year, iterDate.Month, 1);
                    if (options.StartDate.Year > monthlyDate.Year || (options.StartDate.Year == monthlyDate.Year && options.StartDate.Month > monthlyDate.Month))
                    {
                        monthlyDate = new LocalDate(options.StartDate.Year, options.StartDate.Month, 1);
                    }

                    for (var d = monthlyDate; d <= searchLimit; d = d.PlusMonths(1))
                    {
                        var monthsBetween = Period.Between(options.StartDate.With(DateAdjusters.StartOfMonth), d, PeriodUnits.Months).Months;
                        if (monthsBetween % o.Interval != 0) continue;

                        IEnumerable<LocalDate?> occurrencesInMonth;
                        if (o.IsRelative && o.Relative.HasValue)
                        {
                            occurrencesInMonth = new[] { FindRelativeDateInMonth(d.Year, d.Month, d.Calendar, o.Relative.Value) };
                        }
                        else
                        {
                            occurrencesInMonth = o.DaysOfMonth.Select(day => TryCreateDate(d.Year, d.Month, day, d.Calendar));
                        }

                        foreach (var occurrenceDate in occurrencesInMonth)
                        {
                            if (occurrenceDate.HasValue && occurrenceDate.Value >= options.StartDate && occurrenceDate.Value <= searchLimit)
                            {
                                yield return occurrenceDate.Value.At(o.StartTime).InZone(zone, Resolvers.LenientResolver);
                            }
                        }
                    }
                    break;

                case YearlyOptions o:
                    var yearlyDate = options.StartDate;
                    if (yearlyDate.Year < iterDate.Year)
                    {
                        var yearsSince = iterDate.Year - yearlyDate.Year;
                        var intervalsSince = (int)Math.Ceiling((double)yearsSince / o.Interval);
                        yearlyDate = yearlyDate.PlusYears(intervalsSince * o.Interval);
                    }

                    for (var y = yearlyDate.Year; y <= searchLimit.Year; y += o.Interval)
                    {
                        foreach (var month in o.Months.OrderBy(m => m))
                        {
                            IEnumerable<LocalDate?> occurrencesInMonth;
                            if (o.IsRelative && o.Relative.HasValue)
                            {
                                occurrencesInMonth = new[] { FindRelativeDateInMonth(y, month, options.StartDate.Calendar, o.Relative.Value) };
                            }
                            else
                            {
                                occurrencesInMonth = o.DaysOfMonth.Select(day => TryCreateDate(y, month, day, options.StartDate.Calendar));
                            }

                            foreach (var occurrenceDate in occurrencesInMonth)
                            {
                                if (occurrenceDate.HasValue && occurrenceDate.Value >= options.StartDate && occurrenceDate.Value <= searchLimit)
                                {
                                    yield return occurrenceDate.Value.At(o.StartTime).InZone(zone, Resolvers.LenientResolver);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private static LocalDate? FindRelativeDateInMonth(int year, int month, CalendarSystem calendar, RelativeOccurrence relative)
        {
            var firstDayOfMonth = new LocalDate(year, month, 1, calendar);
            var lastDayOfMonth = firstDayOfMonth.PlusMonths(1).PlusDays(-1);
            var daysInMonth = lastDayOfMonth.Day;

            if (relative.Position == RelativePosition.Day)
            {
                int day = relative.Index == RelativeIndex.Last ? daysInMonth : (int)relative.Index;
                return day <= daysInMonth ? new LocalDate(year, month, day, calendar) : (LocalDate?)null;
            }

            IEnumerable<LocalDate> candidates;
            if (relative.Position == RelativePosition.Weekday)
            {
                candidates = Enumerable.Range(1, daysInMonth)
                    .Select(d => new LocalDate(year, month, d, calendar))
                    .Where(d => d.DayOfWeek >= IsoDayOfWeek.Monday && d.DayOfWeek <= IsoDayOfWeek.Friday);
            }
            else if (relative.Position == RelativePosition.WeekendDay)
            {
                candidates = Enumerable.Range(1, daysInMonth)
                    .Select(d => new LocalDate(year, month, d, calendar))
                    .Where(d => d.DayOfWeek == IsoDayOfWeek.Saturday || d.DayOfWeek == IsoDayOfWeek.Sunday);
            }
            else
            {
                var targetDayOfWeek = (IsoDayOfWeek)relative.Position;
                candidates = Enumerable.Range(1, daysInMonth)
                    .Select(d => new LocalDate(year, month, d, calendar))
                    .Where(d => d.DayOfWeek == targetDayOfWeek);
            }

            var candidateList = candidates.ToList();
            if (!candidateList.Any()) return null;

            if (relative.Index == RelativeIndex.Last) return candidateList.LastOrDefault();

            int index = (int)relative.Index - 1;
            return index < candidateList.Count ? candidateList[index] : (LocalDate?)null;
        }

        private static LocalDate? TryCreateDate(int year, int month, int day, CalendarSystem calendar)
        {
            try { return new LocalDate(year, month, day, calendar); }
            catch (ArgumentOutOfRangeException) { return null; }
        }

        private static T Min<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) < 0 ? a : b;
        private static T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) > 0 ? a : b;
    }
}
