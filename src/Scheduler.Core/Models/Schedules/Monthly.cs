namespace Scheduler.Core.Models.Schedules
{
    using System.Collections.Generic;
    using System.Linq;
    using Scheduler.Core.Enums;
    using Scheduler.Core.Models.Schedules.Base;

    public class Monthly : Recurring
    {
        private List<int> _daysOfMonth = new List<int>();
        public IReadOnlyList<int> DaysOfMonth => _daysOfMonth;
        public RelativeOccurrence? Relative { get; private set; }
        public bool IsRelative => Relative.HasValue;

        public void UseDaysOfMonth(System.Action<List<int>> configure)
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
            if (_daysOfMonth.Count == 0)
            {
                _daysOfMonth.Add(dayOfMonth);
            }
        }
    }
}
