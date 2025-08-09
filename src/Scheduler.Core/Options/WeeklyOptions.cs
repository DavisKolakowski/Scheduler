namespace Scheduler.Core.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class WeeklyOptions : RecurringOptions
    {
        private List<int> _daysOfWeek = new List<int>();

        public List<int> DaysOfWeek
        {
            get => _daysOfWeek;
            set
            {
                if (value == null)
                {
                    _daysOfWeek = new List<int>();
                    return;
                }
                _daysOfWeek = value.Where(d => d >= 1 && d <= 7).Distinct().OrderBy(d => d).ToList();
            }
        }
    }
}
