using Scheduler.Core;
using Scheduler.Core.Enums;
using NodaTime;
using Xunit;

namespace Scheduler.UnitTests
{
    public class ScheduleBuilderTests
    {
        private readonly DateTimeZone _timeZone = DateTimeZone.Utc;

        [Fact]
        public void UseCase1_OneTimeOccurrence_NoRecurrence_NoEndDate()
        {
            // Arrange & Act
            var builder = Schedule.CreateBuilder(new LocalDate(2024, 1, 15), new LocalTime(15, 0), new LocalTime(17, 0), _timeZone);
            var schedule = builder.Build();

            // Assert
            Assert.NotNull(schedule);
            Assert.Null(schedule.Recurrence);
            Assert.Null(schedule.EndDate);
        }

        [Fact]
        public void UseCase2_OneTimeOccurrence_NoRecurrence_WithEndDate()
        {
            // Arrange & Act
            var builder = Schedule.CreateBuilder(new LocalDate(2024, 1, 15), new LocalTime(15, 0), new LocalTime(17, 0), _timeZone);
            builder.AddEndDate(new LocalDate(2024, 1, 15));
            var schedule = builder.Build();

            // Assert
            Assert.NotNull(schedule);
            Assert.Null(schedule.Recurrence);
            Assert.NotNull(schedule.EndDate);
            Assert.Equal(new LocalDate(2024, 1, 15), schedule.EndDate.Value);
        }

        [Fact]
        public void UseCase3_SimpleDailyRecurrence()
        {
            // Arrange & Act
            var builder = Schedule.CreateBuilder(new LocalDate(2024, 1, 15), new LocalTime(15, 0), new LocalTime(17, 0), _timeZone);
            builder.AddRecurrence(frequencyOptions =>
            {
                frequencyOptions.UseDaily();
            });
            var schedule = builder.Build();

            // Assert
            Assert.NotNull(schedule);
            Assert.NotNull(schedule.Recurrence);
            Assert.Equal(1, schedule.Recurrence.Interval);
        }

        [Fact]
        public void UseCase4_Every3Days()
        {
            // Arrange & Act
            var builder = Schedule.CreateBuilder(new LocalDate(2024, 1, 15), new LocalTime(15, 0), new LocalTime(17, 0), _timeZone);
            builder.AddRecurrence(frequencyOptions =>
            {
                frequencyOptions.UseDaily(dailyOptions =>
                {
                    dailyOptions.Interval = 3;
                });
            });
            var schedule = builder.Build();

            // Assert
            Assert.NotNull(schedule);
            Assert.NotNull(schedule.Recurrence);
            Assert.Equal(3, schedule.Recurrence.Interval);
        }

        [Fact]
        public void UseCase5_SimpleWeeklyRecurrence()
        {
            // Arrange & Act
            var builder = Schedule.CreateBuilder(new LocalDate(2024, 1, 10), new LocalTime(19, 0), new LocalTime(22, 0), _timeZone);
            builder.AddRecurrence(frequencyOptions =>
            {
                frequencyOptions.UseWeekly();
            });
            var schedule = builder.Build();

            // Assert
            Assert.NotNull(schedule);
            Assert.NotNull(schedule.Recurrence);
            Assert.Equal(1, schedule.Recurrence.Interval);
        }

        [Fact]
        public void UseCase6_WeekdaysOnly()
        {
            // Arrange & Act
            var builder = Schedule.CreateBuilder(new LocalDate(2024, 2, 1), new LocalTime(11, 30), new LocalTime(14, 0), _timeZone);
            builder.AddEndDate(new LocalDate(2024, 8, 1));
            builder.AddRecurrence(frequencyOptions =>
            {
                frequencyOptions.UseWeekly(weeklyOptions =>
                {
                    weeklyOptions.Days.Add(1); // Monday
                    weeklyOptions.Days.Add(2); // Tuesday
                    weeklyOptions.Days.Add(3); // Wednesday
                    weeklyOptions.Days.Add(4); // Thursday
                    weeklyOptions.Days.Add(5); // Friday
                });
            });
            var schedule = builder.Build();

            // Assert
            Assert.NotNull(schedule);
            Assert.NotNull(schedule.Recurrence);
            Assert.Equal(5, ((Scheduler.Core.Options.WeeklyOptions)schedule.Recurrence).Days.Count);
        }

        [Fact]
        public void UseCase10_MonthlyFirstFriday()
        {
            // Arrange & Act
            var builder = Schedule.CreateBuilder(new LocalDate(2024, 2, 2), new LocalTime(18, 0), new LocalTime(21, 0), _timeZone);
            builder.AddRecurrence(frequencyOptions =>
            {
                frequencyOptions.UseMonthly(monthlyOptions =>
                {
                    monthlyOptions.UseRelativeMonthly(DayOfWeekIndex.First, DayOfWeekType.Friday);
                });
            });
            var schedule = builder.Build();

            // Assert
            Assert.NotNull(schedule);
            Assert.NotNull(schedule.Recurrence);
            var monthlyOptions = (Scheduler.Core.Options.MonthlyOptions)schedule.Recurrence;
            Assert.True(monthlyOptions.UseRelative);
            Assert.Equal(DayOfWeekIndex.First, monthlyOptions.WeekIndex);
            Assert.Equal(DayOfWeekType.Friday, monthlyOptions.WeekDayType);
        }
    }
}