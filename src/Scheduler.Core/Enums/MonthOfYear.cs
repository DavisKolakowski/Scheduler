namespace Scheduler.Core.Enums
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    public enum MonthOfYear : int
    {
        [EnumMember(Value = "January")]
        January = 1,
        [EnumMember(Value = "February")]
        February = 2,
        [EnumMember(Value = "March")]
        March = 3,
        [EnumMember(Value = "April")]
        April = 4,
        [EnumMember(Value = "May")]
        May = 5,
        [EnumMember(Value = "June")]
        June = 6,
        [EnumMember(Value = "July")]
        July = 7,
        [EnumMember(Value = "August")]
        August = 8,
        [EnumMember(Value = "September")]
        September = 9,
        [EnumMember(Value = "October")]
        October = 10,
        [EnumMember(Value = "November")]
        November = 11,
        [EnumMember(Value = "December")]
        December = 12
    }
}
