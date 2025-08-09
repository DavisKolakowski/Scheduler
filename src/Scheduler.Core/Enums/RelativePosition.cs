namespace Scheduler.Core.Enums
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    public enum RelativePosition : int
    {
        [EnumMember(Value = "Monday")]
        Monday = 1,
        [EnumMember(Value = "Tuesday")]
        Tuesday = 2,
        [EnumMember(Value = "Wednesday")]
        Wednesday = 3,
        [EnumMember(Value = "Thursday")]
        Thursday = 4,
        [EnumMember(Value = "Friday")]
        Friday = 5,
        [EnumMember(Value = "Saturday")]
        Saturday = 6,
        [EnumMember(Value = "Sunday")]
        Sunday = 7,
        [EnumMember(Value = "day")]
        Day = 8,
        [EnumMember(Value = "weekday")]
        Weekday = 9,
        [EnumMember(Value = "weekend day")]
        WeekendDay = 10
    }
}
