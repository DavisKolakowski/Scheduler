namespace Scheduler.Core.Models
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
    using Scheduler.Core.Options;
    using Scheduler.Core.Utilities;

    public class Schedule<TOptions> : ISchedule<TOptions> where TOptions : IScheduleOptions
    {
        private readonly IClock _clock;
        private readonly ZonedDateTime _firstOccurrence;
        private readonly Duration _occurrenceDuration;
        private readonly ZonedDateTime? _expiration;

        public Schedule(TOptions options, IClock clock)
        {
            Options = options;
            _clock = clock;
            Type = options.GetType().Name.Replace("Options", string.Empty);
            _firstOccurrence = options.StartDate.At(options.StartTime).InZone(options.TimeZone, Resolvers.LenientResolver);
            _occurrenceDuration = Period.Between(options.StartTime, options.EndTime, PeriodUnits.Ticks).ToDuration();
            Description = DescriptionGenerator.Generate(Options);
            _expiration = CalculateExpiration();
        }

        public string Type { get; }
        public string Description { get; }
        public string OccurrenceDuration => FormatDuration(_occurrenceDuration);
        public TOptions Options { get; }

        public ZonedDateTime? GetNextOccurrence()
        {
            var now = _clock.GetCurrentInstant();

            var activeOccurrence = GetOccurrences(now.Minus(_occurrenceDuration), now.Plus(Duration.FromSeconds(1)))
                                   .FirstOrDefault(occ => occ.ToInstant() <= now && occ.ToInstant().Plus(_occurrenceDuration) > now);

            if (activeOccurrence != default)
            {
                return activeOccurrence;
            }

            var next = GetUpcomingOccurrences(1).FirstOrDefault();
            return next == default ? (ZonedDateTime?)null : next;
        }

        public ZonedDateTime? GetPreviousOccurrence()
        {
            var now = _clock.GetCurrentInstant();
            var previous = GetOccurrences(_firstOccurrence.ToInstant(), now)
                           .LastOrDefault(occ => occ.ToInstant().Plus(_occurrenceDuration) <= now);

            return previous == default ? (ZonedDateTime?)null : previous;
        }

        public IEnumerable<ZonedDateTime> GetOccurrencesCompleted(int maxItems = 100)
        {
            var now = _clock.GetCurrentInstant();
            return GetOccurrences(_firstOccurrence.ToInstant(), now)
                   .Where(occ => occ.ToInstant().Plus(_occurrenceDuration) <= now)
                   .Reverse()
                   .Take(maxItems);
        }

        public IEnumerable<ZonedDateTime> GetUpcomingOccurrences(int maxItems = 100)
        {
            var now = _clock.GetCurrentInstant();
            var searchEnd = _expiration?.ToInstant() ?? now.Plus(Duration.FromDays(365 * 10));
            var upcoming = GetOccurrences(now, searchEnd)
                           .Where(occ => occ.ToInstant() >= now)
                           .Take(maxItems);
            return upcoming;
        }

        private IEnumerable<ZonedDateTime> GetOccurrences(Instant start, Instant end)
        {
            if (end <= start) return Enumerable.Empty<ZonedDateTime>();
            return OccurrenceCalculator.GetOccurrences(Options, start, end);
        }

        private ZonedDateTime? CalculateExpiration()
        {
            if (Options is OneTimeOptions)
            {
                if (Options.EndTime.CompareTo(Options.StartTime) <= 0)
                {
                    return Options.StartDate.PlusDays(1).At(Options.EndTime).InZone(Options.TimeZone, Resolvers.LenientResolver);
                }
                else
                {
                    return Options.StartDate.At(Options.EndTime).InZone(Options.TimeZone, Resolvers.LenientResolver);
                }
            }
            if (Options.EndDate.HasValue)
            {
                return Options.EndDate.Value.At(Options.EndTime).InZone(Options.TimeZone, Resolvers.LenientResolver);
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
