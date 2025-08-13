namespace Scheduler.Core.Models
{
    using Scheduler.Core.Enums;

    public struct Relative
    {
        public RelativeIndex Index { get; }
        public RelativePosition Position { get; }

        public Relative(RelativeIndex index, RelativePosition position)
        {
            Index = index;
            Position = position;
        }
    }
}
