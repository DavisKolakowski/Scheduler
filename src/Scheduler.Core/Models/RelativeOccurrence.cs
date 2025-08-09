namespace Scheduler.Core.Models
{
    using Scheduler.Core.Enums;

    public struct RelativeOccurrence
    {
        public RelativeIndex Index { get; }
        public RelativePosition Position { get; }

        public RelativeOccurrence(RelativeIndex index, RelativePosition position)
        {
            Index = index;
            Position = position;
        }
    }
}
