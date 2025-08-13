namespace Scheduler.Core.Models.Frequencies
{
    using System.Collections.Generic;
    using System.Linq;

    using Scheduler.Core.Models.Frequencies.Base;

    public class Weekly : Recurring
    {
        private List<int> _daysOfWeek = new List<int>();
        public IReadOnlyList<int> DaysOfWeek => _daysOfWeek;

        public void UseDaysOfWeek(System.Action<List<int>> configure)
        {
            var temp = new List<int>();
            configure?.Invoke(temp);

            _daysOfWeek = temp
                .Where(d => d >= 1 && d <= 7)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        internal void Initialize(int dayOfWeek)
        {
            if (_daysOfWeek.Count == 0)
            {
                _daysOfWeek.Add(dayOfWeek);
            }
        }
    }
}
