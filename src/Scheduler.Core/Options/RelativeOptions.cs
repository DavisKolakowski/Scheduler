using Scheduler.Core.Enums;

namespace Scheduler.Core.Options
{
    public class RelativeOptions
    {
        public DayOfWeekIndex RelativeIndex { get; private set; }
        public DayOfWeekType RelativeDayOfWeek { get; private set; }
        
        public RelativeOptions(DayOfWeekIndex relativeIndex, DayOfWeekType relativeDayOfWeek)
        {
            RelativeIndex = relativeIndex;
            RelativeDayOfWeek = relativeDayOfWeek;
        }
    }
}