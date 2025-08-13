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
        private readonly Duration _duration;
        private readonly ZonedDateTime? _expiration;

        public ScheduleContext(TModel model, IClock clock)
        {
            Model = model;
            _clock = clock;
            Type = model.GetType().Name.Replace(nameof(Schedule), string.Empty);
            _duration = Period.Between(model.StartTime, model.EndTime, PeriodUnits.Ticks).ToDuration();
            Description = DescriptionGenerator.Generate(Model);
            _expiration = CalculateExpiration();
            FirstOccurrence = CalculateFirstOccurrence();
            PreviousOccurrence = CalculatePreviousOccurrence();
            NextOccurrence = CalculateNextOccurrence();
            LastOccurrence = CalculateLastOccurrence();
            RequestedAt = _clock.GetCurrentInstant();
        }

        public string Type { get; }
        public string Description { get; }
        public TimeSpan OccurrenceLength => FormatDuration(_duration);
        public ZonedDateTime? FirstOccurrence { get; }       
        public ZonedDateTime? PreviousOccurrence { get; }
        public ZonedDateTime? NextOccurrence { get; }
        public ZonedDateTime? LastOccurrence { get; }
        public TModel Model { get; }
        public Instant RequestedAt { get; }

        public IEnumerable<ZonedDateTime> GetCompletedOccurrences(int maxItems = 100)
        {
            if (maxItems <= 0 || !FirstOccurrence.HasValue)
            {
                yield break;
            }

            var completed = GetOccurrences(FirstOccurrence.Value.ToInstant(), RequestedAt)
                           .Where(occ => occ.ToInstant().Plus(_duration) <= RequestedAt)
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

            var searchEnd = CalculateSearchEnd(RequestedAt, maxItems);

            var upcoming = GetOccurrences(RequestedAt, searchEnd)
                           .Where(occ => occ.ToInstant() >= RequestedAt)
                           .Take(maxItems);

            foreach (var occurrence in upcoming)
            {
                yield return occurrence;
            }
        }

        private ZonedDateTime? CalculateFirstOccurrence()
        {
            var start = Model.StartDate.At(Model.StartTime).InZoneStrictly(Model.TimeZone);
            return GetOccurrences(start.ToInstant(), CalculateSearchEnd(start.ToInstant(), 1)).FirstOrDefault();
        }

        private ZonedDateTime? CalculateNextOccurrence()
        {
            if (Model is OneTime && FirstOccurrence.HasValue)
            {
                var eventStart = FirstOccurrence.Value.ToInstant();
                var eventEnd = eventStart.Plus(_duration);

                if (eventStart > RequestedAt || (eventStart <= RequestedAt && eventEnd > RequestedAt))
                {
                    return FirstOccurrence;
                }

                return null;
            }

            var searchStart = RequestedAt.Minus(Duration.FromDays(1));
            var searchEnd = RequestedAt.Plus(Duration.FromSeconds(1));

            var activeOccurrence = GetOccurrences(searchStart, searchEnd)
                                   .FirstOrDefault(occ => occ.ToInstant() <= RequestedAt && occ.ToInstant().Plus(_duration) > RequestedAt);

            if (activeOccurrence != default)
            {
                return activeOccurrence;
            }

            var futureSearchEnd = CalculateSearchEnd(RequestedAt, 1);
            var next = GetOccurrences(RequestedAt.Plus(Duration.FromMilliseconds(1)), futureSearchEnd)
                       .FirstOrDefault();

            return next == default ? (ZonedDateTime?)null : next;
        }

        private ZonedDateTime? CalculatePreviousOccurrence()
        {
            if (!FirstOccurrence.HasValue) return null;

            if (Model is OneTime)
            {
                var eventStart = FirstOccurrence.Value.ToInstant();
                var eventEnd = eventStart.Plus(_duration);

                if (eventEnd <= RequestedAt)
                {
                    return FirstOccurrence;
                }

                return null;
            }

            var previous = GetOccurrences(FirstOccurrence.Value.ToInstant(), RequestedAt)
                           .LastOrDefault(occ => occ.ToInstant().Plus(_duration) <= RequestedAt);

            return previous == default ? (ZonedDateTime?)null : previous;
        }

        private ZonedDateTime? CalculateLastOccurrence()
        {
            if (!Model.EndDate.HasValue)
            {
                return null;
            }

            var expiration = Model.EndDate.Value.At(Model.EndTime).InZoneStrictly(Model.TimeZone);
            var searchStart = Model.StartDate.At(Model.StartTime).InZoneStrictly(Model.TimeZone).ToInstant();

            return GetOccurrences(searchStart, expiration.ToInstant().Plus(Duration.FromSeconds(1))).LastOrDefault();
        }

        private IEnumerable<ZonedDateTime> GetOccurrences(Instant start, Instant end)
        {
            if (end <= start)
            {
                yield break;
            }
            
            var zone = Model.TimeZone;
            var searchStart = start.InZone(zone);
            var searchEnd = end.InZone(zone);
            var searchLimit = Model.EndDate.HasValue ? Min(searchEnd.Date, Model.EndDate.Value) : searchEnd.Date;
            var iterDate = Max(searchStart.Date, Model.StartDate);

            switch (Model)
            {
                case OneTime o:
                    var oneTimeZdt = o.StartDate.At(o.StartTime).InZoneStrictly(zone);
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
                        yield return d.At(o.StartTime).InZoneStrictly(zone);
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
                            yield return d.At(o.StartTime).InZoneStrictly(zone);
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
                        if (monthsBetween % o.Interval != 0)
                        {
                            continue;
                        }

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
                                yield return occurrenceDate.Value.At(o.StartTime).InZoneStrictly(zone);
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
                                    yield return occurrenceDate.Value.At(o.StartTime).InZoneStrictly(zone);
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

        private LocalDate? FindRelativeDateInMonth(int year, int month, Relative relative)
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
            if (!candidateList.Any())
            {
                return null;
            }

            if (relative.Index == RelativeIndex.Last)
            {
                return candidateList.LastOrDefault();
            }

            int index = (int)relative.Index - 1;
            return index < candidateList.Count ? candidateList[index] : (LocalDate?)null;
        }

        private LocalDate? TryCreateDate(int year, int month, int day)
        {
            try 
            { 
                return new LocalDate(year, month, day); 
            }
            catch (ArgumentOutOfRangeException) 
            { 
                return null; 
            }
        }

        private T Min<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) < 0 ? a : b;
        private T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) > 0 ? a : b;

        private ZonedDateTime? CalculateExpiration()
        {
            if (Model is OneTime)
            {
                if (Model.EndTime.CompareTo(Model.StartTime) <= 0)
                {
                    return Model.StartDate.PlusDays(1).At(Model.EndTime).InZoneStrictly(Model.TimeZone);
                }
                else
                {
                    return Model.StartDate.At(Model.EndTime).InZoneStrictly(Model.TimeZone);
                }
            }
            if (Model.EndDate.HasValue)
            {
                return Model.EndDate.Value.At(Model.EndTime).InZoneStrictly(Model.TimeZone);
            }
            return null;
        }

        private TimeSpan FormatDuration(Duration duration)
        {
            if (duration.TotalTicks < 0)
            {
                duration += Duration.FromDays(1);
            }
            return duration.ToTimeSpan();
        }
    }
}
