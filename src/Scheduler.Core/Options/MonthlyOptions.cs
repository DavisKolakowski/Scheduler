namespace Scheduler.Core.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NodaTime;

    using Scheduler.Core.Enums;
    using Scheduler.Core.Models;

    public class MonthlyOptions : RecurringOptions
    {
        private List<int> _daysOfMonth = new List<int>();

        public IReadOnlyList<int> DaysOfMonth => _daysOfMonth;

        public RelativeOccurrence? Relative { get; private set; }
        public bool IsRelative => Relative.HasValue;

        public void UseDaysOfMonth(Action<List<int>> configure)
        {
            var temp = new List<int>();
            configure?.Invoke(temp);

            _daysOfMonth = temp
                .Where(d => d >= 1 && d <= 31)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            Relative = null;
        }

        public void UseRelative(RelativeIndex index, RelativePosition position)
        {
            _daysOfMonth.Clear();
            Relative = new RelativeOccurrence(index, position);
        }

        internal void Initialize(int dayOfMonth)
        {
            if (_daysOfMonth.Count == 0 && dayOfMonth >= 1 && dayOfMonth <= 31)
            {
                _daysOfMonth.Add(dayOfMonth);
            }
        }
    }
}
