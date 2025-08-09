namespace Scheduler.Core.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using NodaTime;

    using Scheduler.Core.Models;

    public interface ISchedule<out TOptions> where TOptions : IScheduleOptions
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
        /// Gets the duration of a single occurrence, formatted as a string (e.g., "08:30").
        /// </summary>
        string OccurrenceDuration { get; }

        /// <summary>
        /// Gets the original options used to configure this schedule.
        /// </summary>
        TOptions Options { get; }

        /// <summary>
        /// Gets the next occurrence that will happen after the current time, or the one currently in progress.
        /// </summary>
        ZonedDateTime? GetNextOccurrence();

        /// <summary>
        /// Gets the most recent occurrence that happened before the current time.
        /// </summary>
        ZonedDateTime? GetPreviousOccurrence();

        /// <summary>
        /// Gets a list of occurrences that have fully completed between the schedule's start and the current time.
        /// </summary>
        IEnumerable<ZonedDateTime> GetOccurrencesCompleted(int maxItems = 100);

        /// <summary>
        /// Gets a list of upcoming occurrences that are scheduled to happen after the current time.
        /// </summary>
        IEnumerable<ZonedDateTime> GetUpcomingOccurrences(int maxItems = 100);
    }
}
