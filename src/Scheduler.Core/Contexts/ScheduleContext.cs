using Scheduler.Core.Models;

namespace Scheduler.Core.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;

    using NodaTime;
    using NodaTime.Text;
    using NodaTime.TimeZones;

    using Scheduler.Core.Contracts;
    using Scheduler.Core.Enums;
    using Scheduler.Core.Models.Schedules;
    using Scheduler.Core.Models.Schedules.Base;
    using Scheduler.Core.Utilities;

    public class ScheduleContext<TModel> : ISchedule<TModel> where TModel : Schedule
    {
        private readonly IClock _clock;
        private readonly ZonedDateTime _firstOccurrence;
        private readonly Duration _occurrenceDuration;
        private readonly ZonedDateTime? _expiration;

        public ScheduleContext(TModel model, IClock clock)
        {
            Model = model;
            _clock = clock;
            Type = model.GetType().Name.Replace("Schedule", string.Empty);
            _firstOccurrence = model.StartDate.At(model.StartTime).InZone(model.TimeZone, Resolvers.LenientResolver);
            _occurrenceDuration = Period.Between(model.StartTime, model.EndTime, PeriodUnits.Ticks).ToDuration();
            Description = DescriptionGenerator.Generate(Model);
            _expiration = CalculateExpiration();
        }

        public string Type { get; }
        public string Description { get; }
        public string OccurrenceDuration => FormatDuration(_occurrenceDuration);
        public TModel Model { get; }

        public ZonedDateTime? GetNextOccurrence()
        {
            var now = _clock.GetCurrentInstant();

            if (Model is OneTime)
            {
                var eventStart = _firstOccurrence.ToInstant();
                var eventEnd = eventStart.Plus(_occurrenceDuration);

                if (eventStart > now || eventStart <= now && eventEnd > now)
                {
                    return _firstOccurrence;
                }

                return null;
            }

            var searchStart = now.Minus(Duration.FromDays(1));
            var searchEnd = now.Plus(Duration.FromSeconds(1));

            var activeOccurrence = GetOccurrences(searchStart, searchEnd)
                                   .FirstOrDefault(occ => occ.ToInstant() <= now && occ.ToInstant().Plus(_occurrenceDuration) > now);

            if (activeOccurrence != default)
            {
                return activeOccurrence;
            }

            var futureSearchEnd = CalculateSearchEnd(now, 1);
            var next = GetOccurrences(now.Plus(Duration.FromMilliseconds(1)), futureSearchEnd)
                       .FirstOrDefault();

            return next == default ? (ZonedDateTime?)null : next;
        }

        public ZonedDateTime? GetPreviousOccurrence()
        {
            var now = _clock.GetCurrentInstant();

            if (Model is OneTime)
            {
                var eventStart = _firstOccurrence.ToInstant();
                var eventEnd = eventStart.Plus(_occurrenceDuration);

                if (eventEnd <= now)
                {
                    return _firstOccurrence;
                }

                return null;
            }

            var previous = GetOccurrences(_firstOccurrence.ToInstant(), now)
                           .LastOrDefault(occ => occ.ToInstant().Plus(_occurrenceDuration) <= now);

            return previous == default ? (ZonedDateTime?)null : previous;
        }

        public IEnumerable<ZonedDateTime> GetOccurrencesCompleted(int maxItems = 100)
        {
            if (maxItems <= 0) yield break;

            var now = _clock.GetCurrentInstant();

            var completed = GetOccurrences(_firstOccurrence.ToInstant(), now)
                           .Where(occ => occ.ToInstant().Plus(_occurrenceDuration) <= now)
                           .Reverse()
                           .Take(maxItems);

            foreach (var occurrence in completed)
            {
                yield return occurrence;
            }
        }

        public IEnumerable<ZonedDateTime> GetUpcomingOccurrences(int maxItems = 100)
        {
            if (maxItems <= 0) yield break;

            var now = _clock.GetCurrentInstant();
            var searchEnd = CalculateSearchEnd(now, maxItems);

            var upcoming = GetOccurrences(now, searchEnd)
                           .Where(occ => occ.ToInstant() >= now)
                           .Take(maxItems);

            foreach (var occurrence in upcoming)
            {
                yield return occurrence;
            }
        }

        private IEnumerable<ZonedDateTime> GetOccurrences(Instant start, Instant end)
        {
            if (end <= start) yield break;
            
            var zone = Model.TimeZone;
            var searchStart = start.InZone(zone);
            var searchEnd = end.InZone(zone);
            var searchLimit = Model.EndDate.HasValue ? Min(searchEnd.Date, Model.EndDate.Value) : searchEnd.Date;
            var iterDate = Max(searchStart.Date, Model.StartDate);

            switch (Model)
            {
                case OneTime o:
                    var oneTimeZdt = o.StartDate.At(o.StartTime).InZone(zone, Resolvers.LenientResolver);
                    if (ZonedDateTime.Comparer.Instant.Compare(oneTimeZdt, searchStart) >= 0 && 
                        ZonedDateTime.Comparer.Instant.Compare(oneTimeZdt, searchEnd) <= 0)
                    {
                        yield return oneTimeZdt;
                    }
                    break;

                case Daily o:
                    var dailyDate = Model.StartDate;
                    if (dailyDate < iterDate)
                    {
                        var periodsSince = Period.Between(dailyDate, iterDate, PeriodUnits.Days).Days;
                        var intervalsSince = (int)Math.Ceiling((double)periodsSince / o.Interval);
                        dailyDate = Model.StartDate.PlusDays(intervalsSince * o.Interval);
                    }
                    for (var d = dailyDate; d <= searchLimit; d = d.PlusDays(o.Interval))
                    {
                        yield return d.At(o.StartTime).InZone(zone, Resolvers.LenientResolver);
                    }
                    break;

                case Weekly o:
                    var startOfWeek = Model.StartDate.With(DateAdjusters.PreviousOrSame(IsoDayOfWeek.Monday));
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

                case Monthly o:
                    var monthlyDate = new LocalDate(iterDate.Year, iterDate.Month, 1);
                    if (Model.StartDate.Year > monthlyDate.Year || Model.StartDate.Year == monthlyDate.Year && Model.StartDate.Month > monthlyDate.Month)
                    {
                        monthlyDate = new LocalDate(Model.StartDate.Year, Model.StartDate.Month, 1);
                    }

                    for (var d = monthlyDate; d <= searchLimit; d = d.PlusMonths(o.Interval))
                    {
                        var monthsBetween = Period.Between(Model.StartDate.With(DateAdjusters.StartOfMonth), d, PeriodUnits.Months).Months;
                        if (monthsBetween % o.Interval != 0) continue;

                        IEnumerable<LocalDate?> occurrencesInMonth;
                        if (o.IsRelative && o.Relative.HasValue)
                        {
                            occurrencesInMonth = new[] { FindRelativeDateInMonth(d.Year, d.Month, o.Relative.Value) };
                        }
                        else
                        {
                            occurrencesInMonth = o.DaysOfMonth.Select(day => TryCreateDate(d.Year, d.Month, day));
                        }

                        foreach (var occurrenceDate in occurrencesInMonth)
                        {
                            if (occurrenceDate.HasValue && occurrenceDate.Value >= Model.StartDate && occurrenceDate.Value <= searchLimit)
                            {
                                yield return occurrenceDate.Value.At(o.StartTime).InZone(zone, Resolvers.LenientResolver);
                            }
                        }
                    }
                    break;

                case Yearly o:
                    var yearlyDate = Model.StartDate;
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
                                occurrencesInMonth = new[] { FindRelativeDateInMonth(y, month, o.Relative.Value) };
                            }
                            else
                            {
                                occurrencesInMonth = o.DaysOfMonth.Select(day => TryCreateDate(y, month, day));
                            }

                            foreach (var occurrenceDate in occurrencesInMonth)
                            {
                                if (occurrenceDate.HasValue && occurrenceDate.Value >= Model.StartDate && occurrenceDate.Value <= searchLimit)
                                {
                                    yield return occurrenceDate.Value.At(o.StartTime).InZone(zone, Resolvers.LenientResolver);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private Instant CalculateSearchEnd(Instant now, int maxItems)
        {
            if (_expiration.HasValue)
            {
                return _expiration.Value.ToInstant();
            }

            switch (Model)
            {
                case OneTime _:
                    return now.Plus(Duration.FromDays(1));

                case Daily daily:
                    var daysAhead = maxItems * daily.Interval;
                    return now.Plus(Duration.FromDays(daysAhead + 1));

                case Weekly weekly:
                    var weeksNeeded = Math.Max(1, maxItems / Math.Max(1, weekly.DaysOfWeek.Count));
                    var weeksAhead = weeksNeeded * weekly.Interval;
                    return now.Plus(Duration.FromDays(weeksAhead * 7 + 7));

                case Monthly monthly:
                    var monthsNeeded = Math.Max(1, maxItems / Math.Max(1, monthly.DaysOfMonth.Count));
                    var monthsAhead = monthsNeeded * monthly.Interval;
                    return now.Plus(Duration.FromDays(monthsAhead * 32 + 32));

                case Yearly yearly:
                    var occurrencesPerYear = yearly.Months.Count * Math.Max(1, yearly.DaysOfMonth.Count);
                    if (yearly.IsRelative) occurrencesPerYear = yearly.Months.Count;
                    
                    var yearsNeeded = Math.Max(1, maxItems / Math.Max(1, occurrencesPerYear));
                    var yearsAhead = yearsNeeded * yearly.Interval;
                    return now.Plus(Duration.FromDays(yearsAhead * 366 + 366));

                default:
                    return now.Plus(Duration.FromDays(365 * 2));
            }
        }

        private LocalDate? FindRelativeDateInMonth(int year, int month, RelativeOccurrence relative)
        {
            var firstDayOfMonth = new LocalDate(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.PlusMonths(1).PlusDays(-1);
            var daysInMonth = lastDayOfMonth.Day;

            if (relative.Position == RelativePosition.Day)
            {
                int day = relative.Index == RelativeIndex.Last ? daysInMonth : (int)relative.Index;
                return day <= daysInMonth ? new LocalDate(year, month, day) : (LocalDate?)null;
            }

            IEnumerable<LocalDate> candidates;
            if (relative.Position == RelativePosition.Weekday)
            {
                candidates = Enumerable.Range(1, daysInMonth)
                    .Select(d => new LocalDate(year, month, d))
                    .Where(d => d.DayOfWeek >= IsoDayOfWeek.Monday && d.DayOfWeek <= IsoDayOfWeek.Friday);
            }
            else if (relative.Position == RelativePosition.WeekendDay)
            {
                candidates = Enumerable.Range(1, daysInMonth)
                    .Select(d => new LocalDate(year, month, d))
                    .Where(d => d.DayOfWeek == IsoDayOfWeek.Saturday || d.DayOfWeek == IsoDayOfWeek.Sunday);
            }
            else
            {
                var targetDayOfWeek = (IsoDayOfWeek)relative.Position;
                candidates = Enumerable.Range(1, daysInMonth)
                    .Select(d => new LocalDate(year, month, d))
                    .Where(d => d.DayOfWeek == targetDayOfWeek);
            }

            var candidateList = candidates.ToList();
            if (!candidateList.Any()) return null;

            if (relative.Index == RelativeIndex.Last) return candidateList.LastOrDefault();

            int index = (int)relative.Index - 1;
            return index < candidateList.Count ? candidateList[index] : (LocalDate?)null;
        }

        private LocalDate? TryCreateDate(int year, int month, int day)
        {
            try { return new LocalDate(year, month, day); }
            catch (ArgumentOutOfRangeException) { return null; }
        }

        private T Min<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) < 0 ? a : b;
        private T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) > 0 ? a : b;

        private ZonedDateTime? CalculateExpiration()
        {
            if (Model is OneTime)
            {
                if (Model.EndTime.CompareTo(Model.StartTime) <= 0)
                {
                    return Model.StartDate.PlusDays(1).At(Model.EndTime).InZone(Model.TimeZone, Resolvers.LenientResolver);
                }
                else
                {
                    return Model.StartDate.At(Model.EndTime).InZone(Model.TimeZone, Resolvers.LenientResolver);
                }
            }
            if (Model.EndDate.HasValue)
            {
                return Model.EndDate.Value.At(Model.EndTime).InZone(Model.TimeZone, Resolvers.LenientResolver);
            }
            return null;
        }

        private string FormatDuration(Duration duration)
        {
            if (duration.TotalTicks < 0)
            {
                duration += Duration.FromDays(1);
            }
            var pattern = DurationPattern.CreateWithInvariantCulture("HH:mm");
            return pattern.Format(duration);
        }
    }
}
