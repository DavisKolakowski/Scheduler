namespace Scheduler.Core.Enums
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    public enum RelativeIndex : int
    {
        [EnumMember(Value = "first")]
        First = 1,
        [EnumMember(Value = "second")]
        Second = 2,
        [EnumMember(Value = "third")]
        Third = 3,
        [EnumMember(Value = "fourth")]
        Fourth = 4,
        [EnumMember(Value = "last")]
        Last = -1
    }
}
