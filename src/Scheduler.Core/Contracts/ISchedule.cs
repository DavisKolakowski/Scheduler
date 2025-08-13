namespace Scheduler.Core.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using NodaTime;

    using Scheduler.Core.Models;
    using Scheduler.Core.Models.Schedules.Base;

    public interface ISchedule<out TModel> where TModel : Frequency
    {
        /// <summary>
        /// Gets the frequency type of the schedule (e.g., "OneTime", "Daily").
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets a human-readable description of the schedule's occurrences.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the time span of a single occurrence.
        /// </summary>
        TimeSpan OccurrenceLength { get; }

        /// <summary>
        /// Gets the first occurrence of the schedule.
        /// </summary>
        ZonedDateTime? FirstOccurrence { get; }

        /// <summary>
        /// Gets the most recent occurrence that happened before the request time.
        /// </summary>
        ZonedDateTime? PreviousOccurrence { get; }

        /// <summary>
        /// Gets the next occurrence that will happen after the request time, or the one currently in progress.
        /// </summary>
        ZonedDateTime? NextOccurrence { get; }

        /// <summary>
        /// Gets the last occurrence of the schedule, if it has an end date.
        /// </summary>
        ZonedDateTime? LastOccurrence { get; }

        /// <summary>
        /// Gets the original options used to configure this schedule.
        /// </summary>
        TModel Model { get; }

        /// <summary>
        /// Gets the time at which the schedule occurrences are calculated.
        /// </summary>
        Instant RequestedAt { get; }

        /// <summary>
        /// Gets a list of occurrences that have fully completed between the schedule's start and the current time.
        /// </summary>
        IEnumerable<ZonedDateTime> GetCompletedOccurrences(int maxItems = 100);

        /// <summary>
        /// Gets a list of upcoming occurrences that are scheduled to happen after the current time.
        /// </summary>
        IEnumerable<ZonedDateTime> GetUpcomingOccurrences(int maxItems = 100);
    }
}
