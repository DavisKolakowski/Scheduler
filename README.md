# Scheduler.Core

A powerful, flexible, and intuitive .NET scheduling library built with [NodaTime](https://nodatime.org/) for robust date and time handling. Create one-time events, recurring schedules, and complex scheduling patterns with timezone awareness and comprehensive calendar support.

[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-brightgreen.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Features

- ? **One-time and Recurring Schedules** - Create simple events or complex recurring patterns
- ? **Fluent Builder API** - Intuitive and discoverable schedule configuration
- ? **Complete Timezone Support** - Built on NodaTime for accurate timezone handling
- ? **DST Transition Handling** - Automatically handles daylight saving time changes
- ? **Rich Recurrence Patterns** - Daily, weekly, monthly, and yearly with intervals
- ? **Relative Date Support** - "First Friday", "Last Monday", "Third Tuesday", etc.
- ? **Calendar Awareness** - Leap years, month boundaries, and edge cases
- ? **JSON Serialization** - Full support for System.Text.Json with NodaTime
- ? **Comprehensive Testing** - 200+ unit tests covering edge cases
- ? **Performance Optimized** - Intelligent search ranges and efficient algorithms

## Quick Start

### Installation

```bash
dotnet add package Scheduler.Core
```

### Basic Usage

```csharp
using NodaTime;
using Scheduler.Core;

// Create a schedule context with current time
var clock = SystemClock.Instance;
var context = new ScheduleContext(clock);
var timeZone = DateTimeZoneProviders.Tzdb["America/New_York"];

// One-time event
var meeting = context.CreateBuilder(
    new LocalDate(2024, 12, 25),    // Christmas Day
    new LocalTime(14, 0),           // 2:00 PM
    new LocalTime(16, 0),           // 4:00 PM
    timeZone)
    .Build();

// Recurring schedule
var dailyStandup = context.CreateBuilder(
    new LocalDate(2024, 1, 1),
    new LocalTime(9, 0),            // 9:00 AM
    new LocalTime(9, 30),           // 9:30 AM
    timeZone)
    .Recurring()
    .Daily()
    .Build();

// Get next occurrence
var nextMeeting = meeting.GetNextOccurrence();
var nextStandup = dailyStandup.GetNextOccurrence();

// Get upcoming occurrences
var upcomingMeetings = dailyStandup.GetUpcomingOccurrences(10);
```

## Schedule Types

### One-Time Schedules

Perfect for events that happen only once:

```csharp
var appointment = context.CreateBuilder(
    new LocalDate(2024, 6, 15),
    new LocalTime(14, 30),
    new LocalTime(15, 30),
    timeZone)
    .Build();
```

### Daily Schedules

For events that repeat daily or with custom intervals:

```csharp
// Every day
var daily = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Daily()
    .Build();

// Every 3 days
var every3Days = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Daily(o => o.Interval = 3)
    .Build();

// With end date
var withEndDate = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring(new LocalDate(2024, 12, 31))
    .Daily()
    .Build();
```

### Weekly Schedules

For events that repeat weekly on specific days:

```csharp
// Every Tuesday (based on start date)
var weekly = context.CreateBuilder(
    new LocalDate(2024, 1, 2),      // Tuesday
    startTime, endTime, timeZone)
    .Recurring()
    .Weekly()
    .Build();

// Monday, Wednesday, Friday
var mwf = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 })))
    .Build();

// Bi-weekly (every 2 weeks)
var biweekly = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Weekly(o => {
        o.Interval = 2;
        o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 5 })); // Mon, Fri
    })
    .Build();
```

### Monthly Schedules

For events that repeat monthly on specific days or relative positions:

```csharp
// 15th of every month
var monthly = context.CreateBuilder(
    new LocalDate(2024, 1, 15),
    startTime, endTime, timeZone)
    .Recurring()
    .Monthly()
    .Build();

// Multiple days per month
var multipleDays = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 1, 15, 31 })))
    .Build();

// First Friday of every month
var firstFriday = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Friday))
    .Build();

// Last weekday of every month
var lastWeekday = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.Weekday))
    .Build();

// Quarterly (every 3 months)
var quarterly = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Monthly(o => o.Interval = 3)
    .Build();
```

### Yearly Schedules

For events that repeat yearly:

```csharp
// Every December 25th
var christmas = context.CreateBuilder(
    new LocalDate(2024, 12, 25),
    startTime, endTime, timeZone)
    .Recurring()
    .Yearly()
    .Build();

// Multiple months
var semiAnnual = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 6, 12 })))
    .Build();

// Thanksgiving (4th Thursday in November)
var thanksgiving = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Yearly(o => {
        o.UseMonthsOfYear(list => list.Add(11));
        o.UseRelative(RelativeIndex.Fourth, RelativePosition.Thursday);
    })
    .Build();

// Every 4 years (leap year pattern)
var every4Years = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Yearly(o => o.Interval = 4)
    .Build();
```

## Advanced Features

### Relative Positioning

Use natural language patterns for complex scheduling:

```csharp
// First Monday of each month
o.UseRelative(RelativeIndex.First, RelativePosition.Monday)

// Last Friday of each month
o.UseRelative(RelativeIndex.Last, RelativePosition.Friday)

// Second Tuesday of each month
o.UseRelative(RelativeIndex.Second, RelativePosition.Tuesday)

// Third weekday of each month
o.UseRelative(RelativeIndex.Third, RelativePosition.Weekday)

// Last weekend day of each month
o.UseRelative(RelativeIndex.Last, RelativePosition.WeekendDay)

// Last day of each month
o.UseRelative(RelativeIndex.Last, RelativePosition.Day)
```

### Timezone Handling

Built on NodaTime for robust timezone support:

```csharp
var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];
var pacificTime = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
var utc = DateTimeZone.Utc;

// Schedule respects timezone and handles DST automatically
var schedule = context.CreateBuilder(
    new LocalDate(2024, 3, 10),     // DST transition day
    new LocalTime(2, 30),           // Time that doesn't exist (spring forward)
    new LocalTime(3, 30),
    easternTime)
    .Build();
```

### Overnight Events

Handle events that span midnight:

```csharp
var nightShift = context.CreateBuilder(
    new LocalDate(2024, 1, 1),
    new LocalTime(22, 0),           // 10:00 PM
    new LocalTime(6, 0),            // 6:00 AM next day
    timeZone)
    .Recurring()
    .Daily()
    .Build();

Console.WriteLine(nightShift.OccurrenceDuration); // "08:00"
```

### Working with Occurrences

```csharp
var schedule = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Weekly()
    .Build();

// Get the next occurrence (including currently active ones)
var next = schedule.GetNextOccurrence();

// Get the most recent completed occurrence
var previous = schedule.GetPreviousOccurrence();

// Get upcoming occurrences
var upcoming = schedule.GetUpcomingOccurrences(10);

// Get completed occurrences (in reverse chronological order)
var completed = schedule.GetOccurrencesCompleted(5);

// Schedule properties
Console.WriteLine($"Type: {schedule.Type}");
Console.WriteLine($"Description: {schedule.Description}");
Console.WriteLine($"Duration: {schedule.OccurrenceDuration}");
```

## Real-World Examples

### Business Scenarios

```csharp
// Quarterly board meeting (first Friday of Jan, Apr, Jul, Oct)
var boardMeeting = context.CreateBuilder(
    new LocalDate(2024, 1, 5),      // First Friday of January
    new LocalTime(9, 0),
    new LocalTime(12, 0),
    timeZone)
    .Recurring()
    .Yearly(o => {
        o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 4, 7, 10 }));
        o.UseRelative(RelativeIndex.First, RelativePosition.Friday);
    })
    .Build();

// Bi-weekly payroll (every other Friday)
var payroll = context.CreateBuilder(
    new LocalDate(2024, 1, 5),      // First Friday
    new LocalTime(12, 0),
    new LocalTime(12, 0),           // Instant processing
    timeZone)
    .Recurring()
    .Weekly(o => {
        o.Interval = 2;
        o.UseDaysOfWeek(list => list.Add(5));
    })
    .Build();

// End of month billing (last day of each month)
var billing = context.CreateBuilder(
    new LocalDate(2024, 1, 31),
    new LocalTime(23, 59),
    new LocalTime(23, 59),
    timeZone)
    .Recurring()
    .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.Day))
    .Build();
```

### Maintenance and Operations

```csharp
// Weekly maintenance window (first Sunday of each month at 2 AM)
var maintenance = context.CreateBuilder(
    new LocalDate(2024, 1, 7),      // First Sunday
    new LocalTime(2, 0),
    new LocalTime(6, 0),
    timeZone)
    .Recurring()
    .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Sunday))
    .Build();

// Database backup (every 6 hours)
var backup = context.CreateBuilder(
    new LocalDate(2024, 1, 1),
    new LocalTime(0, 0),
    new LocalTime(0, 5),            // 5-minute backup window
    timeZone)
    .Recurring()
    .Daily(o => o.Interval = 1)     // Daily, but check every 6 hours in your app
    .Build();
```

### Educational and Personal

```csharp
// School semester (Mon/Wed/Fri for 16 weeks)
var semester = context.CreateBuilder(
    new LocalDate(2024, 8, 26),     // First Monday of semester
    new LocalTime(10, 0),
    new LocalTime(11, 30),
    timeZone)
    .Recurring(new LocalDate(2024, 12, 13))  // Last Friday of semester
    .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 })))
    .Build();

// Workout routine (every other day)
var workout = context.CreateBuilder(
    new LocalDate(2024, 1, 1),
    new LocalTime(6, 0),
    new LocalTime(7, 0),
    timeZone)
    .Recurring()
    .Daily(o => o.Interval = 2)
    .Build();
```

## JSON Serialization

Full support for JSON serialization with System.Text.Json and NodaTime:

```csharp
using System.Text.Json;
using NodaTime.Serialization.SystemTextJson;

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
};
jsonOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

// Serialize
string json = JsonSerializer.Serialize(schedule, jsonOptions);

// Deserialize
var deserializedSchedule = JsonSerializer.Deserialize<Schedule<DailyOptions>>(json, jsonOptions);
```

## Testing Support

The library includes comprehensive testing utilities:

```csharp
using NodaTime.Testing;

// Use FakeClock for deterministic testing
var fakeClock = new FakeClock(Instant.FromUtc(2024, 1, 1, 12, 0));
var context = new ScheduleContext(fakeClock);

var schedule = context.CreateBuilder(
    new LocalDate(2024, 1, 1),
    new LocalTime(14, 0),
    new LocalTime(15, 0),
    timeZone)
    .Recurring()
    .Daily()
    .Build();

// Test specific moments in time
var nextOccurrence = schedule.GetNextOccurrence();
Assert.NotNull(nextOccurrence);
```

## Architecture

### Key Components

- **`ScheduleContext`** - Main entry point for creating schedules
- **`ISchedule<TOptions>`** - Core schedule interface with occurrence methods
- **`Schedule<TOptions>`** - Main implementation handling all occurrence calculations
- **Builders** - Fluent API for schedule configuration
- **Options Classes** - Type-safe configuration for each schedule type

### Design Principles

- **Immutable Schedules** - Once created, schedules are immutable and thread-safe
- **Type Safety** - Strong typing for all schedule options and configurations
- **Timezone First** - All calculations respect timezone and DST rules
- **Performance** - Intelligent search ranges minimize calculation overhead
- **Testability** - Built with testing in mind using dependency injection

## Error Handling

The library gracefully handles edge cases:

- **Invalid Dates** - Automatically skips Feb 30, Apr 31, etc.
- **DST Transitions** - Uses NodaTime's lenient resolver for time gaps/overlaps
- **Leap Years** - Correctly handles Feb 29 in leap year calculations
- **Month Boundaries** - Handles varying month lengths (28-31 days)
- **Large Intervals** - Efficiently handles very large intervals without overflow

## Performance Considerations

- **Lazy Evaluation** - Occurrences are calculated on-demand
- **Smart Search Ranges** - Pattern-based search limits reduce unnecessary calculations
- **Efficient Algorithms** - Optimized algorithms for date calculations
- **Memory Efficient** - Minimal memory footprint with yielded results

## Dependencies

- **NodaTime** (?3.2.0) - Robust date and time handling
- **.NET Standard 2.1** - Wide compatibility across .NET platforms

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for detailed release notes.

---

## API Reference

### Core Interfaces

#### `ISchedule<TOptions>`

```csharp
public interface ISchedule<out TOptions> where TOptions : IScheduleOptions
{
    string Type { get; }
    string Description { get; }
    string OccurrenceDuration { get; }
    TOptions Options { get; }
    
    ZonedDateTime? GetNextOccurrence();
    ZonedDateTime? GetPreviousOccurrence();
    IEnumerable<ZonedDateTime> GetOccurrencesCompleted(int maxItems = 100);
    IEnumerable<ZonedDateTime> GetUpcomingOccurrences(int maxItems = 100);
}
```

### Schedule Options

#### `OneTimeOptions`
- One-time events with no recurrence

#### `DailyOptions`  
- `Interval` - Days between occurrences (default: 1)

#### `WeeklyOptions`
- `Interval` - Weeks between occurrences (default: 1)
- `DaysOfWeek` - List of days (1=Monday, 7=Sunday)

#### `MonthlyOptions`
- `Interval` - Months between occurrences (default: 1)
- `DaysOfMonth` - Specific days (1-31)
- `IsRelative` - Use relative positioning
- `Relative` - Relative occurrence specification

#### `YearlyOptions`
- `Interval` - Years between occurrences (default: 1)
- `Months` - List of months (1-12)
- `DaysOfMonth` - Specific days (1-31)
- `IsRelative` - Use relative positioning
- `Relative` - Relative occurrence specification

### Enums

#### `RelativeIndex`
- `First`, `Second`, `Third`, `Fourth`, `Last`

#### `RelativePosition`
- `Monday` through `Sunday`
- `Day` - Any day
- `Weekday` - Monday through Friday
- `WeekendDay` - Saturday and Sunday

---