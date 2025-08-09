namespace Scheduler.Core.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NodaTime;

    using Scheduler.Core.Enums;
    using Scheduler.Core.Models;

    public class YearlyOptions : RecurringOptions
    {
        private List<int> _months = new List<int>();
        private List<int> _daysOfMonth = new List<int>();

        public List<int> Months
        {
            get => _months;
            set
            {
                if (value == null)
                {
                    _months = new List<int>();
                    return;
                }
                _months = value.Where(m => m >= 1 && m <= 12).Distinct().OrderBy(m => m).ToList();
            }
        }

        public List<int> DaysOfMonth
        {
            get => _daysOfMonth;
            set
            {
                if (value == null)
                {
                    _daysOfMonth = new List<int>();
                    return;
                }
                _daysOfMonth = value.Where(d => d >= 1 && d <= 31).Distinct().OrderBy(d => d).ToList();
            }
        }

        public RelativeOccurrence? Relative { get; private set; }
        public bool IsRelative => Relative.HasValue;

        public void UseRelative(RelativeIndex index, RelativePosition position)
        {
            Relative = new RelativeOccurrence(index, position);
        }
    }
}
