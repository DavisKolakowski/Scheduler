# Scheduler.UnitTests

This is a comprehensive test suite for the Scheduler.Core library, created using xUnit and NodaTime.Testing.

## Test Structure

The test suite is organized into the following test classes:

### Core Test Classes

1. **BaseScheduleTests.cs** - Base class providing common test infrastructure and helper methods
2. **OneTimeScheduleTests.cs** - Tests for one-time (non-recurring) schedules
3. **DailyScheduleTests.cs** - Tests for daily recurring schedules
4. **WeeklyScheduleTests.cs** - Tests for weekly recurring schedules  
5. **MonthlyScheduleTests.cs** - Tests for monthly recurring schedules
6. **YearlyScheduleTests.cs** - Tests for yearly recurring schedules

### Specialized Test Classes

7. **ScheduleContextTests.cs** - Tests for the ScheduleContext and builder pattern
8. **SchedulePropertiesTests.cs** - Tests for schedule properties like Type, Description, Duration
9. **TimeZoneTests.cs** - Tests for time zone handling and DST transitions
10. **EdgeCaseTests.cs** - Tests for edge cases and boundary conditions
11. **ComplexScenarioTests.cs** - Tests for real-world complex scheduling scenarios
12. **EnumAndRelativeTests.cs** - Comprehensive tests for all enum values and relative positioning

## Test Coverage

The test suite covers:

### Schedule Types
- ? One-time schedules
- ? Daily recurring schedules (with intervals)
- ? Weekly recurring schedules (specific days, intervals)
- ? Monthly recurring schedules (specific days, relative positioning)
- ? Yearly recurring schedules (specific months/days, relative positioning)

### Features Tested
- ? Basic occurrence generation
- ? Time zone handling and DST transitions
- ? Leap year handling (February 29th)
- ? Month boundary transitions (28/30/31 day months)
- ? Year boundary transitions
- ? Relative positioning (First/Second/Third/Fourth/Last Friday, etc.)
- ? Multiple days of week/month configuration
- ? Start and end date boundaries
- ? Overnight schedules (end time before start time)
- ? Zero-duration schedules
- ? Schedule descriptions and formatting
- ? Occurrence duration calculations
- ? Active occurrence detection
- ? Previous/next occurrence retrieval
- ? Bulk occurrence retrieval (upcoming/completed)

### Edge Cases Tested
- ? Invalid dates (Feb 30, Apr 31, etc.)
- ? DST spring forward and fall back transitions
- ? Extreme time zones (UTC+14, UTC-11)
- ? Very large intervals
- ? Zero and negative intervals
- ? Empty configuration lists
- ? Invalid enum values
- ? Boundary date/time values
- ? Week 53 handling
- ? Leap year edge cases

### Real-World Scenarios
- ? Quarterly business meetings
- ? Bi-weekly payroll schedules
- ? Monthly last working day
- ? School semester schedules
- ? Maintenance windows
- ? Multi-timezone conference calls
- ? Holiday schedules (Thanksgiving, etc.)
- ? End-of-month billing
- ? Work shift rotations

## Running the Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=OneTimeScheduleTests"

# Run tests with verbose output
dotnet test --verbosity normal
```

### Visual Studio
1. Open Test Explorer (Test > Test Explorer)
2. Build the solution
3. Click "Run All Tests" or select specific tests to run

## Test Dependencies

The test project uses the following packages:
- **xunit** (v2.9.3) - Test framework
- **xunit.runner.visualstudio** (v3.1.3) - Visual Studio test runner
- **Microsoft.NET.Test.Sdk** (v17.14.1) - .NET test SDK
- **NodaTime.Testing** (v3.2.2) - NodaTime test helpers (FakeClock, etc.)
- **coverlet.collector** (v6.0.4) - Code coverage collection

## Test Patterns

### FakeClock Usage
All tests use NodaTime's `FakeClock` for deterministic time-based testing:

```csharp
var clock = CreateClock(2025, 1, 1, 12, 0); // Fixed time for consistent results
var context = new ScheduleContext(clock);
```

### Test Data Organization
Tests use the builder pattern extensively:

```csharp
var schedule = context.CreateBuilder(startDate, startTime, endTime, timeZone)
    .Recurring()
    .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 })))
    .Build();
```

### Assertion Helpers
The base test class provides helper methods for common assertions:

```csharp
AssertZonedDateTimeEqual(expected, actual);
AssertOccurrences(expectedList, actualList);
```

## Code Coverage

The test suite aims for comprehensive coverage of:
- All public API methods
- All enum values and combinations
- All configuration options
- All edge cases and error conditions
- All time zone scenarios
- All calendar system edge cases

Run `dotnet test --collect:"XPlat Code Coverage"` to generate detailed coverage reports.