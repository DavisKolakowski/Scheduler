namespace Scheduler.Core.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class WeeklyOptions : RecurringOptions
    {
        private List<int> _daysOfWeek = new List<int>();

        public IReadOnlyList<int> DaysOfWeek => _daysOfWeek;

        public void UseDaysOfWeek(Action<List<int>> configure)
        {
            var temp = new List<int>();
            configure?.Invoke(temp);

            _daysOfWeek = temp
                .Where(d => d >= 1 && d <= 7)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        internal void Initialize(int isoDayOfWeek)
        {
            if (_daysOfWeek.Count == 0 && isoDayOfWeek >= 1 && isoDayOfWeek <= 7)
            {
                _daysOfWeek.Add(isoDayOfWeek);
            }
        }
    }
}
